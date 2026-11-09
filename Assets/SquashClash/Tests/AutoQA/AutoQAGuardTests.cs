using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
namespace SquashClash.AutoQA
{
    public sealed class AutoQAGuardTests
    {
        internal static List<string> Violations(string source,bool editorGameView=false)
        {
            var code=Regex.Replace(source,"@\"(?:[^\"]|\"\")*\"|\"(?:\\\\.|[^\"\\\\])*\"|'(?:\\\\.|[^'\\\\])*'|//[^\\r\\n]*|/\\*[\\s\\S]*?\\*/",m=>new string(' ',m.Length));
            var errors=new List<string>();
            foreach(string name in new[]{"Enqueue","ClosedGesture","ApplyShot","ScheduleShot","InputAreaOverride","CancelSystem","TrySave","TrySetLastActive","TryClearLastActive","ContactContract","ContactPlanner","ShotSolver","HttpClient","UnityWebRequest"})
                if(Regex.IsMatch(code,@"\b"+name+@"\b"))errors.Add(name);
            if(Regex.IsMatch(code,@"\bClock\s*\.\s*(Paused|Configure)\b"))errors.Add("clock authority");
            if(Regex.IsMatch(code,@"\bUiBlocker\s*=(?!=)"))errors.Add("UI override");
            if(Regex.IsMatch(code,@"\bSquashClash\s*\.\s*DevPanel\b"))errors.Add("DevPanel");
            if(!editorGameView&&Regex.IsMatch(code,@"\b(GetField|GetProperty|GetMethod|GetType|Invoke|MakeGenericType|BindingFlags)\b"))errors.Add("reflection");
            if(Regex.IsMatch(code,@"\bnew\s+ShotRecord\b"))errors.Add("fabricated record");
            foreach(Match m in Regex.Matches(code,@"\.\s*Set\s*\("))
                if(!Regex.IsMatch(source.Substring(m.Index),"^\\.\\s*Set\\s*\\(\\s*\"practice.launch\"\\s*,"))errors.Add("non-whitelisted tuning");
            return errors;
        }
        [Test] public void AutoQASources_RespectAuthorityContract()
        {
            string dir=Path.Combine(Application.dataPath,"SquashClash/Tests/AutoQA");
            var errors=Directory.GetFiles(dir,"*.cs",SearchOption.AllDirectories).SelectMany(p=>Violations(File.ReadAllText(p),Path.GetFileName(p)=="NativeGameView.cs").Select(e=>Path.GetFileName(p)+": "+e)).ToArray();
            Assert.That(errors,Is.Empty,string.Join("\n",errors));
            string asm=File.ReadAllText(Path.Combine(dir,"SquashClash.AutoQA.asmdef"));
            Assert.That(asm,Does.Contain("TestAssemblies"));Assert.That(asm,Does.Not.Contain("DevPanel").And.Not.Contain("InputSystem.TestFramework"));
        }
        [TestCase("c.Enqueue(default);")][TestCase("h.ApplyShot(1,2,default);")][TestCase("h.ScheduleShot(1,2,null);")]
        [TestCase("app.Tuning.Set(key,1);")][TestCase("app.Tuning.Set(\"human.z\",1);")][TestCase("input.UiBlocker = p=>false;")]
        [TestCase("typeof(X).GetMethod(\"x\");")][TestCase("app.Clock.Paused=true;")]
        public void ProhibitedSource_IsRejected(string source)=>Assert.That(Violations(source),Is.Not.Empty);
        [Test] public void AllowedPreparationAndUiQuery_AreAccepted()=>Assert.That(Violations("app.Tuning.Set(\"practice.launch\",4); input.UiBlocker(anchor);"),Is.Empty);
    }
}
