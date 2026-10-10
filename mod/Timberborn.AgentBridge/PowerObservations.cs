using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.MechanicalSystem;
using Timberborn.TemplateSystem;
using UnityEngine;

namespace Timberborn.AgentBridge;

public sealed class PowerObservations(EntityRegistry entities)
{
    internal static object Vec(Vector3Int p) => new { x=p.x,y=p.y,z=p.z };
    internal static object[] Ports(MechanicalNode node)
    {
        if(node.Transputs.IsDefault || node.Transputs.Length>64) throw new InvalidOperationException("power_ports_unavailable");
        return PortData(node.Transputs,false);
    }
    // Preview nodes have no initialized runtime transput collection. Construct detached
    // geometry descriptors through the public API; never Connect or register them.
    internal static Transput[] PreviewPorts(TemplateSpec template,BlockObject block)
    {
        if(!template.HasSpec<TransputProviderSpec>())return Array.Empty<Transput>();
        var provider=template.GetSpec<TransputProviderSpec>();
        if(provider.Transputs.IsDefault||provider.Transputs.Length>64)throw new InvalidOperationException("power_spec_unavailable");
        var node=block.GetComponent<MechanicalNode>()??throw new InvalidOperationException("power_preview_node_missing");
        var ports=new List<Transput>();
        foreach(var spec in provider.Transputs) foreach(var direction in spec.Directions) {
            if(ports.Count>=64)throw new InvalidOperationException("power_ports_limit");
            ports.Add(new Transput(node,spec,direction,block));
        }
        return ports.ToArray();
    }
    internal static object[] PortData(IEnumerable<Transput> ports,bool preview)=>ports.Select(p=>(object)new {position=Vec(p.Coordinates),target=Vec(p.Target),direction=p.Direction.ToString(),
            reversed=p.ReversedRotation,finished=!preview&&p.IsFinished,connected=!preview&&p.Connected,
            connectedEntityId=!preview&&p.ConnectedNode is { } other ? other.GetComponent<EntityComponent>().EntityId.ToString("D") : null}).ToArray();
    public object Read(string id)
    {
        var e=entities.Entities.SingleOrDefault(e=>!e.Deleted&&e.Initialized&&e.EntityId.ToString("D")==id);
        if(e is null || !e.TryGetComponent<BlockObject>(out var block) || block.IsPreview) throw new BridgeRejectionException("state_conflict");
        var node=e.GetComponent<MechanicalNode>();
        object? network=null;
        if(node is not null && node.Graph is { } graph && graph.Valid)
        {
            var members=graph.Nodes.Select(n=>n.GetComponent<EntityComponent>().EntityId.ToString("D")).OrderBy(s=>s,StringComparer.Ordinal).Take(513).ToArray();
            if(members.Length>512) throw new InvalidOperationException("power_network_limit");
            network=new {anchorId=members.FirstOrDefault(),members,powerSupply=graph.PowerSupply,powerDemand=graph.PowerDemand,
                powerSurplus=graph.PowerSurplus,powered=graph.Powered,batteryCharge=graph.BatteryCharge,batteryCapacity=graph.BatteryCapacity};
        }
        return new {id,template=e.GetComponent<TemplateSpec>().TemplateName,position=Vec(block.Coordinates),finished=block.IsFinished,
            supported=node is not null,ports=node is null?Array.Empty<object>():Ports(node),network,
            active=node is null?(bool?)null:node.Active,powered=node is null?(bool?)null:node.Powered,
            actualInput=node?.Actuals is { } actual?(int?)actual.PowerInput:null,
            actualOutput=node?.Actuals is { } output?(int?)output.PowerOutput:null,
            limitations=new[]{"current_native_mechanical_state_not_future_supply","network_anchor_is_member_id_not_persistent_network_identity","connected_is_not_staffing_or_production","unfinished_ports_do_not_prove_finished_network","no_power_state_mutation"}};
    }
}
