using Timberborn.Bridge.Core;
using Timberborn.SingletonSystem;
using Timberborn.UILayoutSystem;
using UnityEngine;
using UnityEngine.UIElements;
namespace Timberborn.AgentBridge;

public sealed class ActivityLogWindow(UILayout layout, ActivityLog log) : ILoadableSingleton, IUpdatableSingleton, IUnloadableSingleton
{
    private VisualElement? root, window;
    private ScrollView? scroll;
    private Button? toggle;
    private bool visible, onlyActions;
    private long renderedRevision=-1;
    public bool Visible=>visible;
    public bool Attached=>root?.panel is not null;
    public void Load()
    {
        root=new VisualElement { name="timberborn-mcp-log" };
        root.style.alignItems=Align.FlexEnd;
        window=new VisualElement();
        window.style.width=460;window.style.maxWidth=new Length(85,LengthUnit.Percent);
        window.style.backgroundColor=new Color(.10f,.14f,.16f,.98f);
        window.style.paddingLeft=12;window.style.paddingRight=12;window.style.paddingTop=10;window.style.paddingBottom=10;
        window.style.marginBottom=6;window.style.display=DisplayStyle.None;
        window.Add(Text("MCP-Aufrufe",18));
        window.Add(Text("Aktionsbegründungen des Agenten · nur diese Spielsitzung",11));
        var controls=new VisualElement();controls.style.flexDirection=FlexDirection.Row;
        var filter=new Button();filter.text="Alle Aufrufe";filter.clicked+=()=>{onlyActions=!onlyActions;filter.text=onlyActions?"Nur Aktionen":"Alle Aufrufe";renderedRevision=-1;Refresh();};
        controls.Add(filter);
        controls.Add(new Button(()=>{log.Clear();Refresh();}) {text="Leeren"});
        controls.Add(new Button(()=>SetVisible(false)) {text="Schließen"});
        window.Add(controls);
        scroll=new ScrollView(ScrollViewMode.Vertical);
        scroll.style.height=340;window.Add(scroll);
        root.Add(window);
        toggle=new Button(()=>SetVisible(!visible)) {text="MCP-Log"};toggle.style.minWidth=95;root.Add(toggle);
        layout.AddBottomRight(root,50);
    }
    private static Label Text(string value,int size=12)
    {
        var label=new Label(value){enableRichText=false};
        label.style.whiteSpace=WhiteSpace.Normal;label.style.fontSize=size;label.style.color=Color.white;
        return label;
    }
    private void SetVisible(bool show){visible=show;if(window is not null)window.style.display=show?DisplayStyle.Flex:DisplayStyle.None;renderedRevision=-1;Refresh();}
    private static bool IsAction(string tool)=>tool.StartsWith("set_",StringComparison.Ordinal)||tool.StartsWith("place_",StringComparison.Ordinal)||tool.StartsWith("validate_",StringComparison.Ordinal)||tool.StartsWith("remove_",StringComparison.Ordinal)||tool=="demolish_building";
    public void UpdateSingleton()=>Refresh();
    private void Refresh()
    {
        if(root is null || renderedRevision==log.Revision)return;
        if(toggle is not null)toggle.text=$"MCP-Log ({log.Snapshot().Length})";
        if(!visible || scroll is null){renderedRevision=log.Revision;return;}
        scroll.Clear();
        foreach(var e in log.Snapshot().AsEnumerable().Reverse().Where(e=>!onlyActions||IsAction(e.Tool))) {
            var row=new VisualElement();row.style.marginBottom=10;row.style.paddingBottom=7;
            row.Add(Text($"{e.StartedAtUtc.ToLocalTime():HH:mm:ss}  {e.Tool}",13));
            string state=e.State switch {"running"=>"Läuft / Abschluss noch unbestätigt", "ok"=>"Erfolgreich", "applied"=>"Angewendet", "rejected"=>"Abgewiesen", "unconfirmed"=>"Unbestätigt – Zustand prüfen", "cancelled"=>"Abgebrochen", _=>"Fehler – Zustand prüfen"};
            row.Add(Text(state));
            if(e.Summary.Length>0)row.Add(Text(e.Summary,11));
            row.Add(Text(e.Reasoning.Length>0?"Begründung: "+e.Reasoning:"Keine Begründung mitgegeben.",12));
            scroll.Add(row);
        }
        if(scroll.childCount==0)scroll.Add(Text("Noch keine passenden MCP-Aufrufe."));
        renderedRevision=log.Revision;
    }
    public void Unload(){root?.RemoveFromHierarchy();root=null;window=null;scroll=null;toggle=null;visible=false;log.Clear();}
}
