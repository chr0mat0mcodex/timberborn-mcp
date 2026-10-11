using System.Text.Json;
using Timberborn.Bridge.Core;

namespace Timberborn.Backend.Native;

public static class ConstructionWorkAccessContract
{
    public static void Validate(JsonElement report, bool eligible)
    {
        bool required=report.GetProperty("required").GetBoolean();
        string state=report.GetProperty("state").GetString() ?? "";
        int count=report.GetProperty("candidateCount").GetInt32();
        var cells=report.GetProperty("reachableCells").EnumerateArray().ToArray();
        if(state is not ("road_reachable" or "no_work_cell" or "no_connected_work_cell" or "final_work_access_blocked" or "helper_runtime_check") ||
            report.GetProperty("source").GetString()!="geometric_foundation_perimeter_and_native_district_road" ||
            report.GetProperty("nativeConstructionProven").GetBoolean() || count is <0 or >512 ||
            cells.Length>16 || cells.Length>count ||
            required==(state=="helper_runtime_check") ||
            state=="road_reachable" && cells.Length==0 ||
            state!="road_reachable" && cells.Length!=0 ||
            state=="no_work_cell" && count!=0 ||
            state=="no_connected_work_cell" && count==0 ||
            !required && count!=0 || eligible&&!ConstructionWorkCells.Accepts(required,state))
            throw new InvalidDataException("invalid_construction_work_access");
        var seen=new HashSet<(int,int,int)>();
        foreach(var cell in cells)
        {
            int x=cell.GetProperty("x").GetInt32(),y=cell.GetProperty("y").GetInt32(),z=cell.GetProperty("z").GetInt32();
            if(x is <0 or >4095 || y is <0 or >4095 || z is <0 or >4095 || !seen.Add((x,y,z)))
                throw new InvalidDataException("invalid_work_access_cell");
        }
    }
}
