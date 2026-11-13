using System.Collections.Generic;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Shot;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    public sealed class TaskSection
    {
        private readonly PracticeTask task;
        private readonly Label captured=new Label{name="task-captured"};
        private readonly Toggle enabled=new Toggle("Activar en la próxima recepción"){name="task-enabled"};
        private readonly DropdownField column,row,power,bounceColumn,depth;
        public VisualElement Root {get;}=new Foldout{name="practice-task-section",text="Tarea de observación · provisional",value=false};
        public TaskSection(AppBootstrap app)
        {
            task=app.GetComponent<PracticeTask>();Root.style.flexShrink=0;
            var help=new Label("Bandas provisionales, sin criterio de aceptación. Las selecciones se capturan al relanzar; no se guardan en presets.");
            help.style.whiteSpace=WhiteSpace.Normal;Root.Add(help);Root.Add(enabled);
            column=Choice("task-front-column","Frontis X",new[]{"[0,20;2,20)","[2,20;4,20)","[4,20;6,20]"});
            row=Choice("task-front-row","Frontis Y",new[]{"[0,60;1,60)","[1,60;2,60)","[2,60;3,60]"});
            power=Choice("task-power","Potencia P",new[]{"Suave [0;1/3)","Media [1/3;2/3)","Fuerte [2/3;1]"});
            bounceColumn=Choice("task-bounce-column","Primer bote X",new[]{"[0,20;2,20)","[2,20;4,20)","[4,20;6,20]"});
            depth=Choice("task-bounce-depth","Primer bote Z",new[]{"Corto [0,20;3,20)","Medio [3,20;6,40)","Profundo [6,40;9,55]"});
            foreach(var field in new[]{column,row,power,bounceColumn,depth})field.RegisterValueChangedCallback(_=>Stage());
            enabled.RegisterValueChangedCallback(_=>Stage());captured.style.whiteSpace=WhiteSpace.Normal;Root.Add(captured);Refresh();
        }
        private DropdownField Choice(string name,string label,string[] choices)
        {
            var field=new DropdownField(label,new List<string>(choices),0){name=name};field.style.flexShrink=0;Root.Add(field);return field;
        }
        private void Stage()
        {task?.Select(enabled.value?new PracticeTaskRequest(column.index,row.index,power.index,bounceColumn.index,depth.index):null);Refresh();}
        public void Refresh()
        {
            if(task==null)return;
            captured.text="Actual: "+PracticeTask.Describe(task.CurrentRequest)+"\nPróxima: "+PracticeTask.Describe(task.NextRequest);
            var r=task.LastRecord;
            if(r!=null)captured.text+="\nÚltimo: "+r.Stage+" · captura "+(r.CaptureFailure??"disponible")+" · frontis "+(r.HasFrontImpact?"observado":r.FrontFailure)+" · bote "+(r.HasFirstBounce?"observado":r.BounceFailure);
        }
    }
}
