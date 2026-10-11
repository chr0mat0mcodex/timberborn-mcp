namespace Timberborn.Backend.Native;

// Derived only from this validated observation. Never parse localized status text
// or turn an absent component / idle job into a fabricated production failure.
public static class OperationAssessmentBuilder
{
    public static OperationAssessment Read(NativeOperation d)
    {
        var blockers=new List<string>(); var warnings=new List<string>(); var unknown=new List<string>();
        if(!d.Finished) return new(["construction_unfinished"],[],["operation_not_observed"],false);
        if(d.Paused==true) blockers.Add("building_paused");
        if(d.Workplace is {} w) {
            if(w.Assigned==0) blockers.Add("no_assigned_workers");
            else if(w.Understaffed) warnings.Add("understaffed");
            if(w.WorkingHours==false) blockers.Add("outside_working_hours");
            else if(w.WorkingHours is null) unknown.Add("working_hours_unknown");
        }
        if(d.Manufacturing is {} m) {
            if(!m.HasRecipe) blockers.Add("recipe_missing");
            else {
                if(m.HasIngredients==false) blockers.Add("ingredients_not_ready");
                if(m.ConsumesFuel==true && m.HasFuel==false) blockers.Add("fuel_not_ready");
                if(m.OutputSpace==false) blockers.Add("output_capacity_unavailable");
                if(!m.Ready) warnings.Add("manufactory_not_ready");
            }
        } else unknown.Add("manufacturing_not_observed");
        if(d.Power is {} p) {
            if(p.IsConsumer==true) {
                if(p.Active==false) warnings.Add("consumer_inactive");
                else if(p.Active is null) unknown.Add("consumer_activity_unknown");
                else if(p.NetworkValid!=true) unknown.Add("power_network_unconfirmed");
                else {
                    if(p.Powered==false) blockers.Add("consumer_not_powered");
                    if(p.Supply<p.Demand) warnings.Add("network_power_deficit");
                }
            }
        } else unknown.Add("power_not_observed");
        return new(blockers.ToArray(),warnings.ToArray(),unknown.ToArray(),false);
    }
}
