using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SquashClash.Core.Config;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    public sealed class ParamSection
    {
        private readonly AppBootstrap app;
        private readonly Action changed;
        private readonly Dictionary<string,VisualElement> controls=new Dictionary<string,VisualElement>();
        private readonly Dictionary<string,Label> captions=new Dictionary<string,Label>();
        private readonly Label errorLabel=new Label {name="param-error"};
        public VisualElement Root {get;}=new VisualElement {name="parameters"};
        public ParamSection(AppBootstrap app,Action changed)
        {
            this.app=app;this.changed=changed;Root.style.flexShrink=0;Root.style.minWidth=0;
            errorLabel.style.whiteSpace=WhiteSpace.Normal;Root.Add(errorLabel);
            foreach(var group in ParamCatalog.All.GroupBy(d=>d.Group))
            {
                var fold=new Foldout {name="params-"+group.Key,text=GroupLabel(group.Key),value=false};fold.RegisterValueChangedCallback(e=>{if(e.target==fold)app.RecordDevControl(fold.name,true,e.newValue?"expanded":"collapsed");});fold.style.flexShrink=0;Root.Add(fold);
                foreach(var def in group)
                {
                    var row=new VisualElement();row.style.flexShrink=0;row.style.minWidth=0;row.style.marginBottom=8;fold.Add(row);
                    var caption=new Label();caption.style.whiteSpace=WhiteSpace.Normal;row.Add(caption);captions.Add(def.Key,caption);
                    VisualElement control;
                    if(def.Kind==ParamKind.Choice)
                    {
                        var options=def.Choices;var dropdown=new DropdownField(options.Select(o=>o.Label).ToList(),0);
                        dropdown.RegisterValueChangedCallback(e=>{var i=Array.FindIndex(options,o=>o.Label==e.newValue);if(i>=0)Edit(def.Key,options[i].Value);});control=dropdown;
                    }
                    else if(def.Kind==ParamKind.Bool)
                    {
                        var toggle=new Toggle();toggle.RegisterValueChangedCallback(e=>Edit(def.Key,e.newValue?1:0));control=toggle;
                    }
                    else if(def.Kind==ParamKind.Int)
                    {
                        var slider=new SliderInt((int)def.Min,(int)def.Max) {showInputField=true};slider.RegisterValueChangedCallback(e=>Edit(def.Key,e.newValue));control=slider;
                    }
                    else
                    {
                        var slider=new Slider((float)def.Min,(float)def.Max) {showInputField=true};
                        slider.RegisterValueChangedCallback(e=>Edit(def.Key,Quantize(e.newValue,def)));control=slider;
                    }
                    // Keep incomplete decimals/integers local until Enter or focus leaves the field.
                    if(control is Slider||control is SliderInt)
                    {
                        var text=control.Q<TextField>();if(text!=null)text.isDelayed=true;
                        var integer=control.Q<IntegerField>();if(integer!=null)integer.isDelayed=true;
                    }
                    control.name=def.Key;control.style.minHeight=44;control.style.flexShrink=0;control.style.minWidth=0;row.Add(control);controls.Add(def.Key,control);
                }
            }
            Refresh();
        }
        private static string GroupLabel(string group)=>group switch {"Pace"=>"Ritmo","Physics"=>"Física","Materials"=>"Materiales","Launcher"=>"Lanzador","Court"=>"Cancha","Camera"=>"Cámara","Players"=>"Referencias","Debug"=>"Depuración",_=>group};
        private static double Quantize(float value,ParamDef d)=>float.IsNaN(value)||float.IsInfinity(value)?value:Math.Max(d.Min,Math.Min(d.Max,Math.Round(Math.Round(value/d.Step)*d.Step,8)));
        private void Edit(string key,double value){TrySet(key,value,out _);Refresh();changed?.Invoke();}
        public bool TrySet(string key,double value,out string error)
        {
            app.RecordDevControl(key,true,"invoked","activation");
            try {app.Tuning.Set(key,value);error=null;errorLabel.text="";app.RecordDevControl(key,true,"completed");return true;}
            catch(ArgumentException e){error=e.Message;errorLabel.text="Valor inválido: "+error;app.RecordDevControl(key,false,"invalid-value");return false;}
        }
        public void Refresh()
        {
            var backend=app.Host.ActiveConfig?.Backend??app.Config.Backend;
            foreach(var def in ParamCatalog.All)
            {
                var value=app.Tuning.Visible.Get(def.Key);var c=controls[def.Key];bool operative=def.OperativeFor(backend);c.SetEnabled(operative);
                captions[def.Key].text=def.Label+" · "+value.ToString("0.####",CultureInfo.InvariantCulture)+" "+def.Unit+
                    (app.Tuning.PendingKeys.Contains(def.Key)?" · pendiente":"")+(operative?"":" · no operativo con P1");
                var focused=c.panel?.focusController?.focusedElement as VisualElement;
                if(focused!=null&&(focused==c||c.Contains(focused)))continue;
                if(c is Slider s)s.SetValueWithoutNotify((float)value);
                else if(c is SliderInt i)i.SetValueWithoutNotify((int)value);
                else if(c is Toggle t)t.SetValueWithoutNotify(value!=0);
                else if(c is DropdownField d)d.SetValueWithoutNotify(def.Choices.Single(o=>o.Value==value).Label);
            }
        }
    }
}
