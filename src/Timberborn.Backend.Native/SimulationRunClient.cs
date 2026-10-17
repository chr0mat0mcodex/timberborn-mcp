using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record NativeSimulationRun(string RunId, string State, string Reason, double StartGameHours,
    double TargetGameHours, double ObservedGameHours, double ElapsedGameHours, double OvershootHours,
    double ElapsedRealSeconds, int MaxRealSeconds, int RequestedSpeed, float ObservedSpeed,
    bool PauseConfirmed, bool TargetReached, bool Terminal,
    int? StopAtAvailableLogs = null, int? ObservedAvailableLogs = null, bool StockTargetReached = false,
    ProductionTargetState[]? GoodTargets = null, int? StopAtScience = null, int? ObservedScience = null,
    bool ProductionTargetsReached = false, ProductionDiagnostic[]? ProductionDiagnostics = null, double? DiagnosticsObservedGameHours = null,
    string[]? BuildingIds = null, BuildingProductionState[]? BuildingProduction = null, bool BuildingProductionReached=false, long BuildingJournalCursor=0, OperationBatch? CurrentBuildingOperations=null, double? BottleneckObservedGameHours=null, string? BottleneckState=null, bool MeasureProduction=false);

public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<NativeSimulationRun>> SimulationRun(SimulationRunRequest r,
        System.Collections.Specialized.NameValueCollection query, CancellationToken ct)
    {
        // Reparse at the transport boundary; never accept an arbitrary route/query.
        var checkedRequest = SimulationRunRequest.Parse("/agent-api/v1/" + r.Route, query);
        if (checkedRequest.RunId != r.RunId || checkedRequest.Session != r.Session || checkedRequest.Diagnose != r.Diagnose) throw new ArgumentException();
        string suffix = string.Join("&", query.AllKeys.Select(k => Uri.EscapeDataString(k!) + "=" + Uri.EscapeDataString(query[k]!)));
        var e = await Get<NativeSimulationRun>(r.Route + "?" + suffix, ct, r.Writes ? HttpMethod.Post : HttpMethod.Get, maxResponseBytes:2*1024*1024);
        var d = e.Data;
        if (!r.Diagnose && (e.BridgeVersion is not ("0.47.0" or "0.48.0" or "0.49.0" or "0.50.0" or "0.51.0" or "0.52.0" or "0.53.0" or "0.54.0" or "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0" or "0.59.0" or "0.60.0" or "0.61.0" or "0.62.0" or "0.63.0" or "0.64.0" or "0.65.0" or "0.66.0") || d.CurrentBuildingOperations is not null ||
            d.BottleneckState is not null || d.BottleneckObservedGameHours is not null ||
            d.ProductionDiagnostics is not null || d.DiagnosticsObservedGameHours is not null))
            throw new InvalidDataException("Lightweight status requires supported lightweight status without diagnostics");
        ProductionRunContract.Validate(d,r);
        BuildingProductionRunContract.Validate(d,r,e.BridgeVersion);
        if (r.Starts && r.StopAtAvailableLogs != d.StopAtAvailableLogs ||
            d.StopAtAvailableLogs is null && (d.ObservedAvailableLogs is not null || d.StockTargetReached) ||
            d.StopAtAvailableLogs is not null && (d.StopAtAvailableLogs is < 1 or > 1000000 || d.ObservedAvailableLogs is null or < 0 ||
                d.StockTargetReached != (d.ObservedAvailableLogs >= d.StopAtAvailableLogs)) ||
            d.Reason == "stock_target_reached" && !d.StockTargetReached)
            throw new InvalidDataException("Invalid stock condition receipt");
        bool terminal = d.State is "completed" or "cancelled" or "interrupted" or "failed";
        if (e.BridgeVersion is not ("0.22.0" or "0.23.0" or "0.23.1" or "0.23.2" or "0.24.0" or "0.24.1" or "0.25.0" or "0.26.0" or "0.27.0" or "0.28.0" or "0.28.1" or "0.29.0" or "0.29.1" or "0.29.2" or "0.29.3" or "0.30.0" or "0.31.0" or "0.31.1" or "0.31.2" or "0.31.3" or "0.31.4" or "0.32.0" or "0.32.1" or "0.33.0" or "0.33.1" or "0.34.0" or "0.35.0" or "0.35.1" or "0.35.2" or "0.35.3" or "0.35.4" or "0.35.5" or "0.36.0" or "0.37.0" or "0.38.0" or "0.39.0" or "0.40.0" or "0.41.0" or "0.42.0" or "0.43.0" or "0.44.0" or "0.45.0" or "0.46.0" or "0.47.0" or "0.48.0" or "0.49.0" or "0.50.0" or "0.51.0" or "0.52.0" or "0.53.0" or "0.54.0" or "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0" or "0.59.0" or "0.60.0" or "0.61.0" or "0.62.0" or "0.63.0" or "0.64.0" or "0.65.0" or "0.66.0") || e.SessionId != r.Session || d.RunId != r.RunId ||
            d.State is not ("starting" or "running" or "pausing" or "completed" or "cancelled" or "interrupted" or "failed") ||
            d.Reason is not ("production_measurement_complete" or "building_production_proven" or "building_production_time_limit" or "building_configuration_changed" or "building_unavailable" or "building_proof_observation_unavailable" or "building_proof_journal_gap" or "awaiting_speed" or "advancing" or "game_speed_locked" or "target_reached" or "stock_target_reached" or "stock_observation_unavailable" or "production_targets_reached" or "production_time_limit" or "production_observation_unavailable" or "speed_changed" or "pause_unconfirmed" or "speed_unconfirmed" or "clock_reversed" or "real_time_limit" or "simulation_stalled" or "cancel_requested" or "explicit_speed_change" or "session_ended_pause_unconfirmed" or "speed_command_failed") ||
            new[] { d.StartGameHours, d.TargetGameHours, d.ObservedGameHours, d.ElapsedGameHours, d.OvershootHours, d.ElapsedRealSeconds }.Any(n => !double.IsFinite(n) || n < 0) ||
            !float.IsFinite(d.ObservedSpeed) || d.ObservedSpeed < 0 || d.RequestedSpeed is not (1 or 3 or 7) ||
            d.MaxRealSeconds is < 30 or > 86400 || d.TargetGameHours < d.StartGameHours || d.TargetGameHours - d.StartGameHours > 672 ||
            d.Terminal != terminal || d.PauseConfirmed != (d.ObservedSpeed == 0) || d.TargetReached != (d.ObservedGameHours >= d.TargetGameHours) ||
            d.ElapsedGameHours != Math.Max(0, d.ObservedGameHours - d.StartGameHours) || d.OvershootHours != Math.Max(0, d.ObservedGameHours - d.TargetGameHours) ||
            d.State == "completed" && (!(d.TargetReached || d.StockTargetReached || d.ProductionTargetsReached || d.BuildingProductionReached) || !d.PauseConfirmed) || d.State == "cancelled" && !d.PauseConfirmed ||
            r.Starts && (d.RequestedSpeed != r.Speed || d.MaxRealSeconds != r.MaxRealSeconds ||
                Math.Abs(d.TargetGameHours - (r.Route == "simulation-run-for" ? d.StartGameHours + r.DurationHours : r.TargetHours)) > 0.0000001))
            throw new InvalidDataException("Invalid simulation run receipt");
        if(d.BottleneckState is {} diagnosisState) {
            if(r.Route!="simulation-run"||(d.BuildingIds?.Length??0)==0||diagnosisState is not ("observed" or "unavailable")||
                d.BottleneckObservedGameHours is not {} observed||!double.IsFinite(observed)||observed<0||
                (diagnosisState=="observed")!=(d.CurrentBuildingOperations is not null))throw new InvalidDataException("Invalid current bottleneck report");
            if(d.CurrentBuildingOperations is {} batch) {
                if(batch.Items is null||batch.NativeReads!=1||batch.Limitations is null||batch.Items.Length!=d.BuildingIds!.Length)
                    throw new InvalidDataException("Invalid run operation batch");
                var items=new List<OperationBatchItem>();
                for(int i=0;i<batch.Items.Length;i++) {
                    var item=batch.Items[i];
                    if(item is null||item.Id!=d.BuildingIds![i]||item.State is not ("observed" or "not_found")||
                        (item.State=="observed")!=(item.Operation is not null))throw new InvalidDataException("Invalid current operation correlation");
                    if(item.Operation is {} operation) {
                        ValidateOperation(operation,item.Id,e.BridgeVersion);
                        if(operation.Bottleneck is not {} evidence)throw new InvalidDataException("Missing current bottleneck evidence");
                        BottleneckContract.Validate(evidence);
                        item=item with{Operation=operation with{Assessment=OperationAssessmentBuilder.Read(operation)}};
                    }
                    items.Add(item);
                }
                e=e with{Data=d with{CurrentBuildingOperations=batch with{Items=items.ToArray()}}};
            }
        } else if(d.CurrentBuildingOperations is not null||d.BottleneckObservedGameHours is not null||
            e.BridgeVersion is ("0.43.0" or "0.44.0" or "0.45.0" or "0.46.0" or "0.47.0" or "0.48.0" or "0.49.0" or "0.50.0" or "0.51.0" or "0.52.0" or "0.53.0" or "0.54.0" or "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0" or "0.59.0" or "0.60.0" or "0.61.0" or "0.62.0" or "0.63.0" or "0.64.0" or "0.65.0" or "0.66.0")&&r.Route=="simulation-run"&&r.Diagnose&&(d.BuildingIds?.Length??0)>0)
            throw new InvalidDataException("Missing current bottleneck state");
        return e;
    }
}
