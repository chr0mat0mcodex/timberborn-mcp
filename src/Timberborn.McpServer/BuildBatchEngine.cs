using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

// Explicit checkpoint transitions. Every write-ahead state has a read-only recovery path.
public sealed class BuildBatchEngine(IBuildBatchPort port, Action<BuildBatchJob> save)
{
    private static void Stop(BuildBatchJob j, string reason) { j.State = "stopped"; j.Reason = reason; }
    private static void NextRegionOrStop(BuildBatchJob j, string reason) {
        if (j.RegionIndex + 1 >= BuildBatchState.Regions(j.Spec).Length) {
            Stop(j, j.Spec.AdditionalRegions is { Length: > 0 } ? "no_candidate_in_authorized_regions" : reason);
            return;
        }
        // Only called before submission or after fully verified clearance. Never abandon a pending write/run.
        j.PreviousRegions.Add(new(j.RegionIndex, reason, j.ClearanceUsed, j.Targets.Length));
        j.RegionIndex++; j.ClearanceUsed = false; j.Targets = []; j.RemovalCursor = 0;
        j.Selection = null; j.Search = []; j.Completion = null; j.ClearanceRun = null;
        j.State = "planning"; j.Reason = "";
    }
    private async Task<bool> Paused(BuildBatchJob j, CancellationToken ct) {
        if ((await port.Current(j.Spec, ct)).CurrentSpeed == 0) return true;
        Stop(j, "external_simulation_not_paused"); return false;
    }
    private static bool Inside(BuildBatchSpec s, NativeRemovalTarget t) => t.Position.X >= s.X && t.Position.X < s.X + s.Width &&
        t.Position.Y >= s.Y && t.Position.Y < s.Y + s.Height && t.Position.Z == s.Z;
    private static bool Same(NativeRemovalTarget a, NativeRemovalTarget b) => a.Id == b.Id && a.Template == b.Template && a.Position == b.Position && a.Kind == b.Kind;
    private void Checkpoint(BuildBatchJob j, string state) { j.State = state; j.Reason = ""; save(j); }

    // true means wait for the existing game operation before the next transition.
    public async Task<bool> Step(BuildBatchJob j, CancellationToken ct)
    {
        if (j.Terminal) return false;
        var s = j.ActiveSpec;
        switch (j.State) {
            case "planning": {
                if (j.Index == s.Items.Length) { j.State = "completed"; break; }
                if (!await Paused(j, ct)) break;
                if (await port.Prerequisite(s, j.Index, ct) is { } reason) { Stop(j, reason); break; }
                var choices = new List<BatchSelection>(); var searches = new List<BatchSearch>();
                foreach (int rotation in s.Rotations) {
                    var plan = await port.Plan(s, j.Index, rotation, ct);
                    searches.Add(new(rotation, plan.CheckedCandidates, plan.RejectedCandidates, plan.StopReason));
                    for (int i = 0; i < plan.Options.Length; i++) {
                        var o = plan.Options[i];
                        if (o.PlanKey is null) throw new InvalidDataException("missing_plan_key");
                        choices.Add(new(rotation, i, o.PlanKey, o.Origin, o.NewRoadCells.Length));
                    }
                }
                j.Search = searches.ToArray();
                j.Selection = choices.OrderBy(c => c.NewPaths).ThenBy(c => c.Rotation).ThenBy(c => c.Origin.Y).ThenBy(c => c.Origin.X).FirstOrDefault();
                if (j.Selection is null) {
                    if (!j.ClearanceUsed && s.Clearing != "none") j.State = "discovering";
                    else NextRegionOrStop(j, "no_candidate_in_authorized_region");
                    break;
                }
                Checkpoint(j, "pending_build"); // Never re-submit this actionId on recovery.
                NativeProjectExecution receipt;
                try { receipt = await port.Submit(j, ct); }
                catch (BridgeRejectionException ex) {
                    Stop(j, "build_rejected:" + ex.Code); break;
                }
                if (receipt.State == "stopped") { Stop(j, "build_stopped:" + receipt.Reason); break; }
                if (receipt.State == "unconfirmed") { j.Reason = receipt.Reason; throw new IOException("build_unconfirmed"); }
                j.State = "building"; break;
            }
            case "discovering": {
                if (!await Paused(j, ct)) break;
                var targets = (await port.Targets(s, ct)).Where(t => t.Kind == "vegetation" && Inside(s, t) &&
                    (s.Clearing == "all_vegetation" || t.Vegetation?.LifeState == "dead")).OrderBy(t => t.Id).ToArray();
                if (targets.Length == 0) { NextRegionOrStop(j, "no_removable_vegetation"); break; }
                if (targets.Length + j.PreviousClearTargets > s.MaxClearTargets || targets.Any(t => !t.CanDelete || t.Mode != "demolition_mark")) { Stop(j, "clearance_limit_or_unsupported_target"); break; }
                j.Targets = targets; j.RemovalCursor = 0; j.ClearanceUsed = true; j.State = "marking"; break;
            }
            case "marking": {
                if (j.RemovalCursor == j.Targets.Length) { j.State = "verify_clearance"; break; }
                if (!await Paused(j, ct)) break;
                var t = j.Targets[j.RemovalCursor];
                if (t.Marked) { j.RemovalCursor++; break; }
                Checkpoint(j, "pending_mark");
                var result = await port.Mark(s, t, ct);
                if (result.Outcome != "applied") throw new IOException("mark_unconfirmed");
                j.RemovalCursor++; j.State = "marking"; break;
            }
            case "pending_mark": {
                var t = j.Targets[j.RemovalCursor];
                var found = (await port.Targets(s, ct)).SingleOrDefault(x => x.Id == t.Id);
                if (found is not null && (!Same(t, found) || !found.Marked)) { Stop(j, "mark_unconfirmed_no_retry"); break; }
                j.RemovalCursor++; j.State = "marking"; break;
            }
            case "verify_clearance": {
                if (!await Paused(j, ct)) break;
                var existing = await port.Targets(s, ct);
                if (!j.Targets.Any(t => existing.Any(x => x.Id == t.Id))) { j.State = "planning"; break; }
                Checkpoint(j, "pending_clearance_run");
                j.ClearanceRun = await port.ClearanceRun(j, true, ct);
                j.State = "clearance_wait"; break;
            }
            case "pending_clearance_run":
            case "clearance_wait": {
                // Missing after write-ahead does not authorize a new start. The caller returns the read error.
                var run = j.ClearanceRun = await port.ClearanceRun(j, false, ct);
                if (run.RunId != j.ClearanceRunId || run.RequestedSpeed != s.Speed || run.MaxRealSeconds != s.MaxRealSeconds ||
                    Math.Abs(run.TargetGameHours - run.StartGameHours - s.ClearanceHours) > 0.0000001) { Stop(j, "clearance_run_conflict"); break; }
                if (!run.Terminal) { save(j); return true; }
                if (run.State != "completed" || !run.PauseConfirmed || !await Paused(j, ct)) { if (!j.Terminal) Stop(j, "clearance_simulation_stopped"); break; }
                var existing = await port.Targets(s, ct);
                if (j.Targets.Any(t => existing.Any(x => x.Id == t.Id))) Stop(j, "clearance_budget_exhausted");
                else j.State = "planning";
                break;
            }
            case "pending_build":
            case "pending_build_run":
            case "building": {
                string previous = j.State;
                var c = j.Completion = await port.Completion(j, false, ct);
                if (c.Outcome == "awaiting_order") { j.State = "building"; save(j); return true; }
                if (c.Outcome == "awaiting_simulation") {
                    if (previous == "pending_build_run") { Stop(j, "simulation_start_unconfirmed_no_retry"); break; }
                    Checkpoint(j, "pending_build_run");
                    c = j.Completion = await port.Completion(j, true, ct);
                    if (c.Outcome == "simulation_unconfirmed") throw new IOException("simulation_unconfirmed");
                }
                if (c.Outcome == "finished_accessible") {
                    if (j.Selection is null || c.ActionId != j.ActionId || c.Steps.Length == 0 || c.Steps.Any(t => t.Finished != true) || c.CurrentSpeed != 0)
                        throw new InvalidDataException("invalid_batch_completion");
                    j.Finished.Add(new(j.Index, j.ActionId, s.Items[j.Index].Template, j.Selection.Origin, c.Outcome, j.RegionIndex));
                    j.Index++; j.Selection = null;
                    j.State = j.Index == s.Items.Length ? "completed" : "planning";
                    break;
                }
                if (c.Outcome is "running" or "awaiting_order") { j.State = "building"; save(j); return true; }
                Stop(j, c.Outcome); break;
            }
            default: throw new InvalidDataException("unknown_batch_state");
        }
        save(j); return false;
    }
}
