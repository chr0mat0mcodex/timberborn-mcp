using System.Globalization;

namespace Timberborn.Bridge.Core;

public sealed class ProductionTarget
{
    public string Good { get; set; } = "";
    public int Amount { get; set; }
}

public sealed class ProductionTargetState
{
    public string Good { get; set; } = "";
    public int Amount { get; set; }
    public int? Available { get; set; }
}

public sealed class ProductionSample
{
    public Dictionary<string, int?> Goods { get; set; } = new(StringComparer.Ordinal);
    public int? Science { get; set; }
}

public sealed class ProductionDiagnostic
{
    public string Good { get; set; } = "";
    public int Producers { get; set; }
    public bool Complete { get; set; }
    public string[] Evidence { get; set; } = Array.Empty<string>();
    public string[] MissingIngredients { get; set; } = Array.Empty<string>();
}

public static class ProductionTargets
{
    // Compact transport encoding; the MCP input remains a structured array.
    public static ProductionTarget[] Parse(string value)
    {
        if (value.Length > 1500) throw new ArgumentException("invalid_production_targets");
        var rows = value.Split(';');
        if (rows.Length is < 1 or > 8) throw new ArgumentException("invalid_production_targets");
        var result = rows.Select(row => {
            var fields = row.Split(':');
            if (fields.Length != 2 || !BuildingPolicy.ValidTemplate(fields[0]) ||
                !int.TryParse(fields[1], NumberStyles.None, CultureInfo.InvariantCulture, out int amount) || amount is < 1 or > 1000000)
                throw new ArgumentException("invalid_production_targets");
            return new ProductionTarget { Good = fields[0], Amount = amount };
        }).ToArray();
        if (result.Select(t => t.Good).Distinct(StringComparer.Ordinal).Count() != result.Length)
            throw new ArgumentException("duplicate_production_target");
        return result.OrderBy(t => t.Good, StringComparer.Ordinal).ToArray();
    }

    public static bool Observe(SimulationRunStatus status, ProductionSample? sample)
    {
        foreach (var target in status.GoodTargets)
            target.Available = sample is not null && sample.Goods.TryGetValue(target.Good, out var stock) && stock >= 0 ? stock : null;
        int? science = sample?.Science;
        status.ObservedScience = status.StopAtScience is not null && science is >= 0 ? science : null;
        status.ProductionTargetsReached = status.HasProductionTargets &&
            status.GoodTargets.All(t => t.Available >= t.Amount) &&
            (status.StopAtScience is null || status.ObservedScience >= status.StopAtScience);
        return status.GoodTargets.All(t => t.Available is >= 0) &&
            (status.StopAtScience is null || status.ObservedScience is >= 0);
    }
}
