using SquashClash.Runtime.App;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    // DEV only (asmdef SQUASH_DEV || UNITY_EDITOR). Sets both players' energy at the start of the NEXT point, to feel the amber and red states.
    // The value stays pending if no rally or match is running, and applies to the first point of the next one. It only shows with fatigue.enabled = 1.
    public sealed class FatigueTestSection
    {
        public VisualElement Root {get;}=new Foldout{name="fatigue-test-section",text="Cansancio de prueba",value=true};
        public FatigueTestSection(AppBootstrap app)
        {
            Root.style.flexShrink=0;
            var help=new Label("Fija la energía de ambos al inicio del PRÓXIMO punto (una vez). Se nota solo con fatigue.enabled=1 y en Rally/Partido.");
            help.style.whiteSpace=WhiteSpace.Normal;Root.Add(help);
            var row=new VisualElement();row.style.flexDirection=FlexDirection.Row;row.style.flexShrink=0;Root.Add(row);
            Add(app,row,"fatigue-test-40","Energía 40",40);Add(app,row,"fatigue-test-10","Energía 10",10);Add(app,row,"fatigue-test-100","Energía 100",100);
        }
        private static void Add(AppBootstrap app,VisualElement row,string name,string text,double energy)
        {
            var button=new Button(()=>DevControlLog.Run(app,name,()=>app.Rally.SetTestEnergy(energy))){name=name,text=text};
            button.style.minHeight=44;button.style.flexGrow=1;button.style.flexBasis=0;button.style.minWidth=0;button.style.whiteSpace=WhiteSpace.Normal;row.Add(button);
        }
    }
}
