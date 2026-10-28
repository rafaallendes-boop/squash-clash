using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using SquashClash.Core.Config;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    public sealed class PresetsSection
    {
        private readonly AppBootstrap app;
        private readonly Action changed;
        private readonly TextField nameField=new TextField("Nombre del preset") {name="preset-name"};
        private readonly DropdownField list=new DropdownField("Cargar preset") {name="preset-list"};
        private bool revealStatus;
        private readonly Label status=new Label {name="preset-status"};
        public VisualElement Root {get;}=new VisualElement {name="presets"};
        public string Status=>status.text;
        public PresetsSection(AppBootstrap app,Action changed)
        {
            this.app=app;this.changed=changed;Root.style.flexShrink=0;Root.style.minWidth=0;
            foreach(var field in new VisualElement[]{nameField,list}){field.style.flexDirection=FlexDirection.Column;field.style.flexShrink=0;field.style.minWidth=0;field.style.minHeight=70;}
            foreach(var label in new[]{nameField.labelElement,list.labelElement}){label.style.minWidth=0;label.style.width=Length.Percent(100);label.style.whiteSpace=WhiteSpace.Normal;}
            Root.Add(new Label("Presets locales"));status.style.whiteSpace=WhiteSpace.Normal;status.style.flexShrink=0;Root.Add(status);
            status.RegisterCallback<GeometryChangedEvent>(_=>{if(revealStatus){revealStatus=false;Root.GetFirstAncestorOfType<ScrollView>()?.ScrollTo(status);}});
            nameField.RegisterValueChangedCallback(_=>app.RecordDevControl("preset-name",true,"edited"));list.RegisterValueChangedCallback(_=>app.RecordDevControl("preset-list",true,"selected"));
            Root.Add(nameField);Add("preset-save","Guardar como…",()=>SaveAs(nameField.value));Root.Add(list);
            Add("preset-load","Cargar",()=>Load(list.value));Add("preset-export","Exportar",()=>{GUIUtility.systemCopyBuffer=Export();Notice("JSON visible copiado al portapapeles.");});
            Add("preset-import","Importar del portapapeles",()=>Import(GUIUtility.systemCopyBuffer));Refresh();
        }
        private void Add(string name,string text,Action action){var b=new Button(action){name=name,text=text};b.style.minHeight=44;b.style.flexShrink=0;b.style.whiteSpace=WhiteSpace.Normal;Root.Add(b);}
        private bool Notice(string message,bool success=true){revealStatus=true;status.text=message;Root.GetFirstAncestorOfType<ScrollView>()?.ScrollTo(status);changed?.Invoke();return success;}
        public void Refresh()
        {
            var choices=new List<string>{"Default"};choices.AddRange(app.Presets.List());list.choices=choices;
            if(!choices.Contains(list.value))list.SetValueWithoutNotify(choices.Contains(app.ActivePresetName)?app.ActivePresetName:"Default");
        }
        public bool SaveAs(string name)=>DevControlLog.Try(app,"preset-save",()=>SaveAsCore(name));
        private bool SaveAsCore(string name)
        {
            if(!app.Presets.TrySave(PresetCodec.FromSet(app.Tuning.Visible,name,app.Build.Label),out var error))return Notice("No se guardó: "+error,false);
            if(!app.Presets.TrySetLastActive(name,out error))return Notice("Archivo guardado; selección activa no persistida: "+error,false);
            app.SetPresetIdentity(name,Hash(PresetCodec.ToJson(PresetCodec.FromSet(app.Tuning.Visible,name,app.Build.Label))));
            Refresh();list.SetValueWithoutNotify(name);return Notice("Guardado: "+name+". Incluye valores pendientes.");
        }
        public bool Load(string name)=>DevControlLog.Try(app,"preset-load",()=>LoadCore(name));
        private bool LoadCore(string name)
        {
            try
            {
                if(name=="Default")
                {
                    var defaults=Resources.Load<TextAsset>("DefaultPreset");
                    if(!PresetCodec.TryParse(defaults.text,out var file,out var error))return Notice("No se cargó: "+error,false);
                    // Default follows the same rule: validate before changing the persisted selection.
                    PresetCodec.Overlay(app.Tuning.Defaults,file,new List<string>());
                    if(!app.Presets.TryClearLastActive(out error))return Notice(error,false);
                    app.Tuning.LoadPreset(file,new List<string>());app.SetPresetIdentity("Default",Hash(defaults.text));return Notice("Default cargado; aplica los cambios pendientes para relanzar.");
                }
                if(!app.Presets.TryLoad(name,out var f,out var loadError))return Notice("No se cargó: "+loadError,false);
                // Validate overlay before changing selection or the live editing state.
                var warnings=new List<string>();PresetCodec.Overlay(app.Tuning.Defaults,f,warnings);
                if(!app.Presets.TrySetLastActive(name,out loadError))return Notice(loadError,false);
                app.Tuning.LoadPreset(f,new List<string>());
                app.Presets.LoadStartup(app.Tuning.Defaults,new List<string>(),out _,out var fileHash);app.SetPresetIdentity(name,fileHash);
                return Notice("Cargado: "+name+". "+string.Join("\n",warnings));
            }
            catch(InvalidDataException exception){return Notice("No se cargó: "+exception.Message,false);}
        }
        public string Export(){string json=null;DevControlLog.Run(app,"preset-export",()=>json=PresetCodec.ToJson(PresetCodec.FromSet(app.Tuning.Visible,app.ActivePresetName,app.Build.Label)));return json;}
        public bool Import(string json)=>DevControlLog.Try(app,"preset-import",()=>ImportCore(json));
        private bool ImportCore(string json)
        {
            if(!PresetCodec.TryParse(json,out var f,out var error))return Notice(ImportError(json,error),false);
            var warnings=new List<string>();
            try {app.Tuning.LoadPreset(f,warnings);}
            catch(InvalidDataException exception){return Notice(ImportError(json,exception.Message),false);}
            app.SetPresetIdentity("Importado: "+(f.name??"sin nombre"),Hash(json));
            return Notice("Importado sin guardar. Usa Guardar como… para persistirlo. "+string.Join("\n",warnings));
        }
        private static string ImportError(string text,string error)
        {
            text=text??string.Empty;
            string prefix=text.Substring(0,Math.Min(80,text.Length)).Replace("\r","\\r").Replace("\n","\\n").Replace("\t","\\t");
            return $"No se importó (longitud: {text.Length}, inicio: «{prefix}»): {error}";
        }
        private static string Hash(string text){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-","").ToLowerInvariant();}
    }
}
