using SquashClash.Runtime.App;
using SquashClash.Runtime.Menu;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    // DEV only (asmdef SQUASH_DEV || UNITY_EDITOR). Personal records: a switch (default on; off writes nothing and the card shows no line)
    // and "Borrar récords", which removes every sc.records.* key. Not remembered and not a DefaultPreset key.
    public sealed class RecordsDevSection
    {
        public VisualElement Root {get;}=new Foldout{name="records-dev-section",text="Récords personales",value=true};
        public RecordsDevSection(AppBootstrap app)
        {
            Root.style.flexShrink=0;
            Button toggle=null;
            string Text()=>"Récords: "+(PersonalRecords.Enabled?"sí":"no");
            toggle=new Button(()=>DevControlLog.Run(app,"records-toggle",()=>{PersonalRecords.Enabled=!PersonalRecords.Enabled;toggle.text=Text();})){name="records-toggle",text=Text()};
            toggle.style.minHeight=44;toggle.style.whiteSpace=WhiteSpace.Normal;Root.Add(toggle);
            var clear=new Button(()=>DevControlLog.Run(app,"records-clear",PersonalRecords.Clear)){name="records-clear",text="Borrar récords"};
            clear.style.minHeight=44;clear.style.whiteSpace=WhiteSpace.Normal;Root.Add(clear);
        }
    }
}
