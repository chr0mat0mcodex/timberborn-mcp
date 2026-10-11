namespace Timberborn.Bridge.Core;

// Current observations only. A path is not evidence of a completed delivery.
public sealed class BottleneckEvidence
{
    public string State { get; set; } = "";
    public string? FailureStage { get; set; }
    public bool? DistrictConnected { get; set; }
    public bool? EntranceBlocked { get; set; }
    public GoodFlowEvidence[] Goods { get; set; } = Array.Empty<GoodFlowEvidence>();
    public int CandidateBuildings { get; set; }
    public int InspectedBuildings { get; set; }
    public bool Complete { get; set; }
    // The native outer envelope uses Newtonsoft defaults, not a camel-case resolver.
    // Explicit wire names must also survive serializers with no naming policy.
    public object ToWire() => new {
        state=State, failureStage=FailureStage, districtConnected=DistrictConnected,
        entranceBlocked=EntranceBlocked, candidateBuildings=CandidateBuildings,
        inspectedBuildings=InspectedBuildings, complete=Complete,
        goods=Goods.Select(g=>new {good=g.Good,role=g.Role,amount=g.Amount,
            localStock=g.LocalStock,localUnreservedStock=g.LocalUnreservedStock,
            globalAvailable=g.GlobalAvailable,globalAll=g.GlobalAll,carriedToProcessors=g.CarriedToProcessors,
            matchingEndpoints=g.MatchingEndpoints,reachableEndpoints=g.ReachableEndpoints,
            disconnectedEndpoints=g.DisconnectedEndpoints,unknownEndpoints=g.UnknownEndpoints,
            reachableExamples=g.ReachableExamples}).ToArray()
    };
}

public sealed class GoodFlowEvidence
{
    public string Good { get; set; } = "";
    public string Role { get; set; } = ""; // ingredient, fuel, product
    public int Amount { get; set; }
    public int LocalStock { get; set; }
    public int LocalUnreservedStock { get; set; }
    public int GlobalAvailable { get; set; }
    public int GlobalAll { get; set; }
    public int CarriedToProcessors { get; set; }
    public int MatchingEndpoints { get; set; }
    public int ReachableEndpoints { get; set; }
    public int DisconnectedEndpoints { get; set; }
    public int UnknownEndpoints { get; set; }
    public string[] ReachableExamples { get; set; } = Array.Empty<string>();
}
