namespace Timberborn.Bridge.Core;

public sealed class MeasuredProduct
{
    public string Good { get; set; } = "";
    public double AmountPerCycle { get; set; }
}
public sealed class MeasuredTime
{
    public string State { get; set; } = "";
    public double Hours { get; set; }
}
public sealed class RecipeDerivedOutput
{
    public string Good { get; set; } = "";
    public double Amount { get; set; }
    public double? PerGameHour { get; set; }
}
public sealed class ProductionMeasurement
{
    public long CompletedCycles { get; set; }
    public double MeasuredHours { get; set; }
    public MeasuredProduct[] Products { get; set; } = Array.Empty<MeasuredProduct>();
    public double SciencePerCycle { get; set; }
    public MeasuredTime[] Times { get; set; } = Array.Empty<MeasuredTime>();
    public double? CyclesPerHour => MeasuredHours > 0 ? CompletedCycles / MeasuredHours : null;
    public double RecipeDerivedScience => CompletedCycles * SciencePerCycle;
    public double? RecipeDerivedSciencePerHour => MeasuredHours>0?RecipeDerivedScience/MeasuredHours:null;
    public RecipeDerivedOutput[] RecipeDerivedProducts => Products.Select(p => new RecipeDerivedOutput {
        Good=p.Good, Amount=CompletedCycles*p.AmountPerCycle,
        PerGameHour=MeasuredHours>0?(double?)(CompletedCycles*p.AmountPerCycle/MeasuredHours):null
    }).ToArray();
    public string? LargestObservedStall => Times.Where(t=>t.State!="producing"&&t.Hours>0)
        .OrderByDescending(t=>t.Hours).ThenBy(t=>t.State,StringComparer.Ordinal).FirstOrDefault()?.State;
    public ProductionMeasurement Copy() => new() { CompletedCycles=CompletedCycles,MeasuredHours=MeasuredHours,
        SciencePerCycle=SciencePerCycle,Products=Products.Select(p=>new MeasuredProduct{Good=p.Good,AmountPerCycle=p.AmountPerCycle}).ToArray(),
        Times=Times.Select(t=>new MeasuredTime{State=t.State,Hours=t.Hours}).ToArray() };
}

// Bounded aggregate, no per-tick history. Durations are sampled evidence, not causal attribution.
public sealed class ProductionMeasurementTracker
{
    public const double MaxClassifiedIntervalHours = 0.25;
    public static readonly string[] States = {"producing","paused","staffing","outside_working_hours","ingredients","storage","power","multiple_blockers","unknown"};
    private readonly ProductionMeasurement value;
    private double lastHours, lastProgress;
    private long lastCycles;
    private string lastState="unknown";
    private bool initialized;
    public ProductionMeasurementTracker(MeasuredProduct[] products,double science)
    {
        value=new ProductionMeasurement {Products=products.Select(p=>new MeasuredProduct{Good=p.Good,AmountPerCycle=p.AmountPerCycle}).ToArray(),
            SciencePerCycle=science,Times=States.Select(s=>new MeasuredTime{State=s}).ToArray()};
        if(!Valid(value,0))throw new ArgumentException("invalid_measurement_recipe");
    }
    public ProductionMeasurement Observe(double hours,double progress,long cycles,string[] blockers)
    {
        if(!Finite(hours)||!Finite(progress)||hours<0||progress<0||cycles<value.CompletedCycles||cycles>1000000000||
            initialized&&hours<lastHours)throw new ArgumentException("invalid_measurement_sample");
        // Render updates may observe production after the clock sample at the same game time.
        // Retain the interval baseline until game time advances; otherwise that progress is lost.
        if(initialized&&hours==lastHours) {
            value.CompletedCycles=cycles;
            return value.Copy();
        }
        string state=Classify(blockers);
        if(initialized) {
            double delta=hours-lastHours;
            // Long gaps and state transitions cannot be assigned to one cause.
            string interval=delta>MaxClassifiedIntervalHours?"unknown":
                cycles>lastCycles||progress>lastProgress?"producing":state==lastState?state:"unknown";
            value.Times.Single(t=>t.State==interval).Hours+=delta;
            value.MeasuredHours+=delta;
        }
        initialized=true;lastHours=hours;lastProgress=progress;lastCycles=cycles;lastState=state;
        value.CompletedCycles=cycles;
        return value.Copy();
    }
    private static string Classify(string[] blockers)
    {
        var categories=blockers.Select(b=>b switch {
            "building_paused"=>"paused", "no_assigned_workers"=>"staffing", "outside_working_hours"=>"outside_working_hours",
            "ingredients_not_ready" or "fuel_not_ready"=>"ingredients", "output_capacity_unavailable"=>"storage",
            "consumer_not_powered" or "consumer_inactive"=>"power", _=>"unknown"
        }).Distinct().ToArray();
        if(categories.Contains("unknown"))return "unknown";
        return categories.Length==0?"unknown":categories.Length==1?categories[0]:"multiple_blockers";
    }
    public static bool Valid(ProductionMeasurement? m,double elapsed)
    {
        if(!Finite(elapsed)||elapsed<0||m is null||m.CompletedCycles<0||m.CompletedCycles>1000000000||!Finite(m.MeasuredHours)||m.MeasuredHours<0||m.MeasuredHours>elapsed+0.000001||
            m.MeasuredHours>0&&m.MeasuredHours<0.000000001||
            !Finite(m.SciencePerCycle)||m.SciencePerCycle<0||m.SciencePerCycle>1000000||m.Products is null||m.Products.Length>64||
            m.Products.Any(p=>p is null||!BuildingPolicy.ValidTemplate(p.Good)||!Finite(p.AmountPerCycle)||p.AmountPerCycle<=0||p.AmountPerCycle>1000000)||
            m.Products.Select(p=>p.Good).Distinct().Count()!=m.Products.Length||m.Times is null||m.Times.Length!=States.Length||
            m.Times.Any(t=>t is null||!Finite(t.Hours)||t.Hours<0)||!m.Times.Select(t=>t.State).SequenceEqual(States))return false;
        return Math.Abs(m.Times.Sum(t=>t.Hours)-m.MeasuredHours)<0.000001;
    }
    public static bool Follows(ProductionMeasurement old,ProductionMeasurement next) =>
        next.CompletedCycles>=old.CompletedCycles&&next.MeasuredHours>=old.MeasuredHours&&next.SciencePerCycle==old.SciencePerCycle&&
        old.Products.Select(p=>(p.Good,p.AmountPerCycle)).SequenceEqual(next.Products.Select(p=>(p.Good,p.AmountPerCycle)))&&
        old.Times.Zip(next.Times,(a,b)=>b.Hours>=a.Hours).All(x=>x);
    private static bool Finite(double n)=>!double.IsNaN(n)&&!double.IsInfinity(n);
}
