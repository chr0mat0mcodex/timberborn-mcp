using System.Collections.Specialized;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.Tests;

public sealed class ProductionRunContractTests
{
    private const string Session = "11111111-1111-4111-8111-111111111111";
    private const string Id = "22222222-2222-4222-8222-222222222222";
    private static SimulationRunRequest Request(bool start = false)
    {
        var query = new NameValueCollection { { "session", Session }, { "runId", Id } };
        if (start)
        {
            query.Add("duration", "2"); query.Add("unit", "hours");
            query.Add("speed", "3"); query.Add("expectedSpeed", "0");
            query.Add("maxRealSeconds", "60"); query.Add("goodTargets", "Plank:4");
        }
        return SimulationRunRequest.Parse("/agent-api/v1/" + (start ? "simulation-run-for" : "simulation-run"), query);
    }

    private static NativeSimulationRun Run(int? available = 0) => new(
        Id, "running", "advancing", 24, 26, 24.5, .5, 0, 2, 60, 3, 3, false, false, false,
        GoodTargets: [new() { Good = "Plank", Amount = 4, Available = available }]);
    private static ProductionDiagnostic Diagnostic() => new()
    {
        Good = "Plank", Producers = 1, Evidence = ["producer_ready"], MissingIngredients = [],
    };

    [Theory]
    [InlineData("unknown")]
    [InlineData("reached")]
    [InlineData("time_limit")]
    [InlineData("science_unknown")]
    [InlineData("diagnostics")]
    [InlineData("no_targets")]
    public void AcceptedObservationsRetainTheirDistinctMeaning(string scenario)
    {
        var run = scenario switch
        {
            "unknown" => Run(null) with { Reason = "production_observation_unavailable" },
            "reached" => Run(4) with { ProductionTargetsReached = true, Reason = "production_targets_reached" },
            "time_limit" => Run() with { TargetReached = true, Reason = "production_time_limit" },
            "science_unknown" => Run(4) with { StopAtScience = 10, ObservedScience = null },
            "diagnostics" => Run() with { DiagnosticsObservedGameHours = 0, ProductionDiagnostics = [Diagnostic()] },
            "no_targets" => Run() with { GoodTargets = null },
            _ => throw new ArgumentException(),
        };
        ProductionRunContract.Validate(run, Request());
    }

    [Theory]
    [InlineData("null_target", "invalid_production_targets")]
    [InlineData("duplicate", "invalid_production_targets")]
    [InlineData("negative_stock", "invalid_production_targets")]
    [InlineData("legacy_mix", "invalid_production_targets")]
    [InlineData("unknown_reached", "invalid_production_outcome")]
    [InlineData("time_without_target", "invalid_production_outcome")]
    [InlineData("changed_request", "production_target_mismatch")]
    [InlineData("unexpected_diagnostics", "unexpected_production_diagnostics")]
    [InlineData("missing_time", "invalid_production_diagnostics")]
    [InlineData("null_diagnostic", "invalid_production_diagnostics")]
    [InlineData("wrong_good", "invalid_production_diagnostics")]
    [InlineData("unsupported_evidence", "invalid_production_evidence")]
    [InlineData("null_evidence", "invalid_production_evidence")]
    public void RejectionsRetainExactErrorAndShortCircuitOrder(string defect, string expected)
    {
        var run = Run();
        switch (defect)
        {
            // These also contain a later contradiction, which must not mask the first rejection.
            case "null_target": run = run with { GoodTargets = [null!], ProductionTargetsReached = true }; break;
            case "duplicate": run = run with { GoodTargets = [run.GoodTargets![0], run.GoodTargets![0]] }; break;
            case "negative_stock": run = Run(-1); break;
            case "legacy_mix": run = run with { StopAtAvailableLogs = 1 }; break;
            case "unknown_reached": run = Run(null) with { ProductionTargetsReached = true, StopAtScience = 10 }; break;
            case "time_without_target": run = run with { Reason = "production_time_limit" }; break;
            case "changed_request": run = run with { StopAtScience = 10, ProductionDiagnostics = [null!] }; break;
            case "unexpected_diagnostics": run = run with { GoodTargets = null, DiagnosticsObservedGameHours = double.NaN }; break;
            case "missing_time": run = run with { ProductionDiagnostics = [Diagnostic()] }; break;
            case "null_diagnostic": run = run with { DiagnosticsObservedGameHours = 0, ProductionDiagnostics = [null!] }; break;
            case "wrong_good":
                var wrong = Diagnostic(); wrong.Good = "Gear"; wrong.Evidence = null!;
                run = run with { DiagnosticsObservedGameHours = 0, ProductionDiagnostics = [wrong] }; break;
            case "unsupported_evidence":
                var unsupported = Diagnostic(); unsupported.Evidence = ["invented_cause"];
                run = run with { DiagnosticsObservedGameHours = 0, ProductionDiagnostics = [unsupported] }; break;
            case "null_evidence":
                var missing = Diagnostic(); missing.Evidence = null!;
                run = run with { DiagnosticsObservedGameHours = 0, ProductionDiagnostics = [missing] }; break;
            default: throw new ArgumentException();
        }
        var exception = Assert.Throws<InvalidDataException>(() => ProductionRunContract.Validate(run, Request(start: defect == "changed_request")));
        Assert.Equal(expected, exception.Message);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(-1d)]
    public void InvalidDiagnosticTimeKeepsDefaultExceptionMessage(double time)
    {
        var exception = Assert.Throws<InvalidDataException>(() => ProductionRunContract.Validate(
            Run() with { DiagnosticsObservedGameHours = time }, Request()));
        Assert.Equal(new InvalidDataException().Message, exception.Message);
    }

    [Fact]
    public void StartBindsTargetsWhileStatusDoesNot()
    {
        ProductionRunContract.Validate(Run(), Request(start: true));
        var different = Run() with { GoodTargets = [new() { Good = "Gear", Amount = 4, Available = 0 }] };
        ProductionRunContract.Validate(different, Request());
        var exception = Assert.Throws<InvalidDataException>(() => ProductionRunContract.Validate(different, Request(start: true)));
        Assert.Equal("production_target_mismatch", exception.Message);
    }
}
