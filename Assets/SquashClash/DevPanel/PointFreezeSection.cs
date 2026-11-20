using SquashClash.Runtime.App;
using SquashClash.Runtime.Visual;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    // DEV only (asmdef SQUASH_DEV || UNITY_EDITOR). Point-end freeze (PO, 19 nov): holds only the rendered pose for 100 ms when a point closes,
    // inside the pause that already exists. Default on; off is exactly the previous behaviour. Not persisted.
    public sealed class PointFreezeSection
    {
        public VisualElement Root {get;}=new Foldout{name="point-freeze-section",text="Congelar fin de punto",value=true};
        public PointFreezeSection(AppBootstrap app)
        {
            Root.style.flexShrink=0;
            var help=new Label("Al cerrarse el punto congela solo el render 100 ms, dentro de la pausa; no cambia simulación ni reloj.");
            help.style.whiteSpace=WhiteSpace.Normal;Root.Add(help);
            var row=new VisualElement();row.style.flexDirection=FlexDirection.Row;row.style.flexShrink=0;Root.Add(row);
            Add(app,row,"point-freeze-on","Congelar sí",true);Add(app,row,"point-freeze-off","Congelar no",false);
        }
        private static void Add(AppBootstrap app,VisualElement row,string name,string text,bool on)
        {
            var button=new Button(()=>DevControlLog.Run(app,name,()=>PointEndFreeze.Enabled=on)){name=name,text=text};
            button.style.minHeight=44;button.style.flexGrow=1;button.style.flexBasis=0;button.style.minWidth=0;button.style.whiteSpace=WhiteSpace.Normal;row.Add(button);
        }
    }
}
