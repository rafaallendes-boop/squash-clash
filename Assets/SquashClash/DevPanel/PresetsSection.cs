using System;
using System.Collections.Generic;
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
        private readonly TextField nameField=new TextField("Guardar como…") {name="preset-name"};
        private readonly DropdownField list=new DropdownField("Cargar preset") {name="preset-list"};
        private readonly Label status=new Label {name="preset-status"};
        public VisualElement Root {get;}=new VisualElement {name="presets"};
        public string Status=>status.text;
        public PresetsSection(AppBootstrap app,Action changed)
        {
            this.app=app;this.changed=changed;nameField.style.minHeight=44;list.style.minHeight=44;
            Root.Add(nameField);Add("preset-save","Guardar",()=>SaveAs(nameField.value));Root.Add(list);
            Add("preset-load","Cargar",()=>Load(list.value));Add("preset-export","Exportar",()=>{GUIUtility.systemCopyBuffer=Export();Notice("JSON visible copiado al portapapeles.");});
            Add("preset-import","Importar del portapapeles",()=>Import(GUIUtility.systemCopyBuffer));status.style.whiteSpace=WhiteSpace.Normal;Root.Add(status);Refresh();
        }
        private void Add(string name,string text,Action action){var b=new Button(action){name=name,text=text};b.style.minHeight=44;Root.Add(b);}
        private bool Notice(string message,bool success=true){status.text=message;changed?.Invoke();return success;}
        public void Refresh()
        {
            var choices=new List<string>{"Default"};choices.AddRange(app.Presets.List());list.choices=choices;
            if(!choices.Contains(list.value))list.SetValueWithoutNotify(choices.Contains(app.ActivePresetName)?app.ActivePresetName:"Default");
        }
        public bool SaveAs(string name)
        {
            if(!app.Presets.TrySave(PresetCodec.FromSet(app.Tuning.Visible,name,app.Build.Label),out var error))return Notice("No se guardó: "+error,false);
            if(!app.Presets.TrySetLastActive(name,out error))return Notice("Archivo guardado; selección activa no persistida: "+error,false);
            app.SetPresetIdentity(name,Hash(PresetCodec.ToJson(PresetCodec.FromSet(app.Tuning.Visible,name,app.Build.Label))));
            Refresh();list.SetValueWithoutNotify(name);return Notice("Guardado: "+name+". Incluye valores pendientes.");
        }
        public bool Load(string name)
        {
            if(name=="Default")
            {
                if(!app.Presets.TryClearLastActive(out var error))return Notice(error,false);
                var defaults=Resources.Load<TextAsset>("DefaultPreset");PresetCodec.TryParse(defaults.text,out var file,out _);
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
        public string Export()=>PresetCodec.ToJson(PresetCodec.FromSet(app.Tuning.Visible,app.ActivePresetName,app.Build.Label));
        public bool Import(string json)
        {
            if(!PresetCodec.TryParse(json,out var f,out var error))return Notice("No se importó: "+error,false);
            var warnings=new List<string>();app.Tuning.LoadPreset(f,warnings);
            app.SetPresetIdentity("Importado: "+(f.name??"sin nombre"),Hash(json));
            return Notice("Importado sin guardar. Usa Guardar como… para persistirlo. "+string.Join("\n",warnings));
        }
        private static string Hash(string text){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-","").ToLowerInvariant();}
    }
}
