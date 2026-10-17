using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record BottleneckFinding(string Area,string Code,string Severity,string? Good,string NextStep);

public static class BottleneckAssessment
{
    public static BottleneckFinding[] Read(NativeOperation d)
    {
        var findings=new List<BottleneckFinding>(WaterOperationAssessment.Read(d));
        void Add(string area,string code,string severity,string step,string? good=null)=>findings.Add(new(area,code,severity,good,step));
        var e=d.Bottleneck;
        if(e is null||e.State!="observed") {
            Add("observation",e?.FailureStage??e?.State??"unavailable","unknown",e?.FailureStage is not null?"Diagnose an der ausgewiesenen Beobachtungsstelle fehlgeschlagen; keine Bau- oder Lieferentscheidung daraus ableiten.":"Fertige Manufaktur mit ausgewähltem Rezept prüfen.");return findings.ToArray();
        }
        if(d.Paused==true)Add("operation","building_paused","blocker","Gebäude aktivieren, wenn Produktion gewünscht ist.");
        if(e.EntranceBlocked==true||e.DistrictConnected==false)
            Add("transport","building_access_blocked","blocker","Gebäudeeingang und Distriktweg herstellen; danach Zugang erneut prüfen.");
        if(e.DistrictConnected is null)Add("transport","district_access_unknown","unknown","Distriktanschluss mit inspect_building_access prüfen.");
        if(!e.Complete)Add("transport","endpoint_scan_incomplete","unknown","Nicht geprüfte Quellen/Lager gezielt untersuchen; fehlende Treffer sind kein vollständiger Negativnachweis.");
        if(d.Workplace is {} w) {
            if(w.Assigned==0)Add("staff","no_assigned_workers","blocker",w.Desired==0?"Sollpersonal erhöhen und Besetzung kontrollieren.":"Freie passende Arbeitskräfte und Arbeitsplatzpriorität prüfen; Sollpersonal ist bereits angefordert.");
            else if(w.Understaffed)Add("staff","understaffed","warning","Besetzung gegenüber Sollpersonal prüfen; Mehrpersonal kann Leistung erhöhen, ist kein belegter Vollstillstand.");
            if(w.WorkingHours==false)Add("staff","outside_working_hours","blocker","Bis zur Arbeitszeit laufen lassen oder Arbeitszeit bewusst ändern.");
            else if(w.WorkingHours is null)Add("staff","working_hours_unknown","unknown","Arbeitszeit separat prüfen.");
        } else Add("staff","workplace_unavailable","unknown","Arbeitsplatzanforderungen dieses Gebäudetyps separat prüfen.");
        foreach(var g in e.Goods) {
            bool product=g.Role=="product";
            bool blocked=product?d.Manufacturing?.OutputSpace==false:
                g.Role=="fuel"?d.Manufacturing?.ConsumesFuel==true&&d.Manufacturing.HasFuel==false:
                d.Manufacturing?.HasIngredients==false;
            // Native readiness is authoritative: a running cycle can already have consumed its inputs.
            if(!blocked||!product&&g.LocalUnreservedStock>=g.Amount)continue;
            string area=product?"storage":"ingredients";
            if(!product&&g.GlobalAvailable<g.Amount&&g.LocalStock<g.Amount) {
                Add(area,"global_available_shortage","blocker",g.GlobalAll>=g.Amount?
                    "Bestand ist global vorhanden, aber nicht ausreichend verfügbar. Reservierungen und gebundene Bestände prüfen.":
                    "Zutat/Brennstoff nachproduzieren oder beschaffen; globaler Bestand reicht für den Bedarf nicht aus.",g.Good);
            }
            if(g.ReachableEndpoints>0)
                Add(product?"storage":"transport",product?"output_full_destination_available":"input_missing_source_available","warning",
                    product?"Passendes erreichbares Lager hat Platz. Abtransport, Arbeitskräfte und Lagerpriorität prüfen; erst danach zusätzlichen Lagerbau erwägen.":
                    "Erreichbare Quelle hat freien Bestand. Lieferung, Arbeitskräfte und Prioritäten prüfen; Weg allein belegt keinen Transport.",g.Good);
            else if(g.UnknownEndpoints>0||!e.Complete)
                Add("transport","route_coverage_unknown","unknown","Quellen/Lagerzugänge gezielt prüfen; noch keinen Wegabriss oder Neubau aus dieser Teilabdeckung ableiten.",g.Good);
            else if(g.DisconnectedEndpoints>0)
                Add("transport","matching_endpoints_disconnected","blocker","Vorhandene passende Quelle bzw. Lager an dasselbe Wegenetz anschließen und Verbindung erneut prüfen.",g.Good);
            else Add(area,product?"no_eligible_storage":"no_public_source","warning",
                product?"Lager für dieses Produkt auswählen, Annahmemodus und freien unreservierten Platz herstellen; nötigenfalls Lager bauen.":
                "Öffentlich abgebbare Quelle bereitstellen; Produktionsausgabe, Lagergut, Reservierungen und Freigabe prüfen.",g.Good);
            if(g.CarriedToProcessors>0&&!product)Add("transport","global_goods_in_transit","info","Lieferung abwarten und erneut lesen; globale Transportmenge ist diesem Gebäude nicht zugeordnet.",g.Good);
        }
        if(d.Power is {ComponentPresent:true,IsConsumer:true} p) {
            if(p.Active==false)Add("power","consumer_inactive","warning","Zuerst Pause, Personal und Zutaten beheben; inaktiver Verbraucher beweist keinen defekten Kraftanschluss.");
            else if(p.Active is null||p.NetworkValid!=true)Add("power","network_unknown","unknown","Kraftanschluss und gültiges Netz separat prüfen; fehlender Nachweis ist keine bewiesene Trennung.");
            else {
                if(p.Powered==false)Add("power","consumer_not_powered","blocker","Verbraucher und Kraftübertragung prüfen; gültiges Netz allein belegt keine Versorgung.");
                if(p.Supply<p.Demand)Add("power","network_power_deficit","warning","Erzeugung erhöhen oder aktive Last reduzieren; aktuelle Unterdeckung belegt keine bestimmte Windursache.");
            }
        } else if(d.Power is null)Add("power","power_unavailable","unknown","Kraftbedarf und Versorgung separat prüfen.");
        if(findings.Count==0)Add("operation","no_observed_bottleneck","info","Begrenzten Produktionsnachweis starten; diese Momentaufnahme belegt keinen abgeschlossenen Zyklus.");
        return findings.ToArray();
    }
}

public static class BottleneckContract
{
    public static void Validate(BottleneckEvidence e)
    {
        if(e.State is not ("observed" or "unavailable" or "unfinished" or "not_manufactory" or "recipe_missing")||
            e.FailureStage is not null&&(e.State!="unavailable"||e.FailureStage is not ("access" or "local_inventory" or "global_stock" or "endpoint_inventory" or "road"))||
            e.CandidateBuildings<0||e.InspectedBuildings<0||e.InspectedBuildings>128||e.InspectedBuildings>e.CandidateBuildings||
            e.State=="observed"&&(e.InspectedBuildings!=Math.Min(128,e.CandidateBuildings)||e.Complete!=(e.CandidateBuildings<=128))||
            e.Goods is null||e.Goods.Length>32||e.State!="observed"&&e.Goods.Length!=0)
            throw new InvalidDataException("invalid_bottleneck_coverage");
        var keys=new HashSet<(string,string)>();
        foreach(var g in e.Goods) {
            if(g is null||!BuildingPolicy.ValidTemplate(g.Good)||g.Role is not ("ingredient" or "fuel" or "product")||g.Amount<1||
                !keys.Add((g.Role,g.Good))||g.LocalStock<0||g.LocalUnreservedStock<0||g.LocalUnreservedStock>g.LocalStock||
                g.GlobalAvailable<0||g.GlobalAll<0||g.CarriedToProcessors<0||g.MatchingEndpoints<0||g.MatchingEndpoints>e.InspectedBuildings||
                g.ReachableEndpoints<0||g.DisconnectedEndpoints<0||g.UnknownEndpoints<0||
                (long)g.ReachableEndpoints+g.DisconnectedEndpoints+g.UnknownEndpoints!=g.MatchingEndpoints||
                g.ReachableExamples is null||g.ReachableExamples.Length!=Math.Min(3,g.ReachableEndpoints)||
                g.ReachableExamples.Distinct().Count()!=g.ReachableExamples.Length||
                g.ReachableExamples.Any(id=>!Guid.TryParseExact(id,"D",out var guid)||guid==Guid.Empty))
                throw new InvalidDataException("invalid_bottleneck_good");
        }
    }
}
