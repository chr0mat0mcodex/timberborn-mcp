namespace Timberborn.Backend.Native;

// Native booleans, never localized status matching. No forecast or production proof.
public static class WaterOperationAssessment
{
    public static BottleneckFinding[] Read(NativeOperation operation)
    {
        if(!operation.Finished) return [];
        var water=operation.Water;
        if(water is null) return [new("water","water_operation_unknown","unknown",null,
            "Überflutung und Ansaugzustand nicht beobachtet; fehlender Status ist kein Trockenheitsnachweis.")];
        var result=new List<BottleneckFinding>();
        if(water.Flooded==true) result.Add(new("water","building_flooded","blocker",null,
            "Gebäude nativ überflutet. Trockenen Ersatzstandort und Wasser-/Nahrungsreserve prüfen; mehr Personal behebt die Überflutung nicht. Abfluss nur nach Rückstauprüfung verändern."));
        if(water.IntakeBlocked==true) result.Add(new("water","water_intake_blocked","blocker",null,
            "Native Ansaugführung blockiert. Ansaugposition und Hindernis prüfen, bevor Personal oder Lieferwege geändert werden."));
        if(water.IntakeUnderwater==false) result.Add(new("water","water_intake_dry","blocker",null,
            "Am nativen Ansaugpunkt fehlt Wasser. Pegel und Ansaugtiefe prüfen; keine Produktion aus fertigem Bau oder Besetzung ableiten."));
        if(water.IntakeUnderwater==true && water.IntakeContamination>0.00001f) result.Add(new("water","water_intake_contaminated","warning",null,
            "Kontamination am Ansaugpunkt beobachtet. Pumpentyp, Rezept und tatsächliche Ausgabe prüfen; dies ist keine Aussage über Trinkbarkeit oder Förderleistung."));
        if(water.InputComponentPresent && !water.FloodComponentPresent) result.Add(new("water","flood_state_unknown","unknown",null,
            "Ansaugung beobachtet, aber keine native Überflutungskomponente vorhanden; trockenen Betrieb nicht unterstellen."));
        return result.ToArray();
    }
}
