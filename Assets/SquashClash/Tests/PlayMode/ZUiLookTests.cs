using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Shot;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Menu and stroke look (18 nov), presentation only: the player's surfaces draw with the bundled Rubik (SIL OFL 1.1),
    // the shot note takes the tone of the result it names (same words, same moment) and pops once when a stroke is judged,
    // and every text a player reads on the menu, the tutorial, the header and the pause card keeps a readable contrast.
    // Named to run last (see ZRallyHudTests).
    public sealed class ZUiLookTests : ZFirstRunFixture
    {
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        private static readonly string[] Tones={"perfect","valid","failed","info"};
        private static string FontName(VisualElement e)
        {
            var d=e.resolvedStyle.unityFontDefinition;string name=d.font!=null?d.font.name:d.fontAsset!=null?d.fontAsset.name:"";return name.EndsWith(" SDF")?name.Substring(0,name.Length-4):name;
        }
        private IEnumerator StartMatch()
        {
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);
            Click("menu-play");Refresh();yield return null;
            Assert.That(rally.Match,Is.Not.Null,"the match must start");
        }
        [UnityTest] public IEnumerator PlayerSurfaces_DrawWithTheBundledOflFont()
        {
            yield return Boot(true);yield return null;
            var expected=new Dictionary<string,string>
            {
                {"menu-title","Rubik-BoldItalic"},{"menu-play","Rubik-BoldItalic"},{"menu-howto","Rubik-Bold"},{"menu-level-easy","Rubik-Bold"},
                {"menu-build","Rubik-Regular"},{"tutorial-title","Rubik-BoldItalic"},{"tutorial-body","Rubik-Regular"},
                {"pause-title","Rubik-BoldItalic"},{"pause-continue","Rubik-Bold"},{"match-result-title","Rubik-BoldItalic"},{"match-result-score","Rubik-BoldItalic"},
                {"rally-banner-title","Rubik-BoldItalic"},{"rally-score-human","Rubik-BoldItalic"},{"rally-pause","Rubik-Bold"},{"shot-note","Rubik-BoldItalic"}
            };
            foreach(var pair in expected)Assert.That(FontName(Find(pair.Key)),Is.EqualTo(pair.Value),pair.Key);
        }
        [UnityTest] public IEnumerator ShotNote_TakesTheToneOfTheResult_AndKeepsItsWords()
        {
            yield return Boot(false);yield return null;
            var note=controller.GetComponent<ShotNoteView>();var label=Find<Label>("shot-note");
            var cases=new (ShotRecord record,string text,string tone)[]
            {
                (new ShotRecord{Result="Perfect"},"Perfecto","perfect"),
                (new ShotRecord{Result="Valid",DeltaMs=-35},"Adelantado","valid"),(new ShotRecord{Result="Valid",DeltaMs=35},"Atrasado","valid"),
                (new ShotRecord{Result="Failed",Fail="Air"},"¡Al aire!","failed"),(new ShotRecord{Result="Failed",Fail="Late"},"¡Tarde!","failed"),
                (new ShotRecord{Result="Unreachable"},"No llega","info"),(new ShotRecord{Result="NoOrder"},"Sin golpe","info"),
                (new ShotRecord{Result="NotEvaluable"},"No evaluable","info"),(new ShotRecord{Result="SystemFault"},"Fallo del sistema","info")
            };
            foreach(var c in cases)
            {
                note.Show(c.record);
                Assert.That(note.Text,Is.EqualTo(c.text),c.record.Result);
                foreach(var tone in Tones)Assert.That(label.ClassListContains("sc-shot-note--"+tone),Is.EqualTo(tone==c.tone),c.text+": "+tone);
            }
            note.ShowText("First Shot no disponible con P1");
            Assert.That(label.ClassListContains("sc-shot-note--info"),Is.True,"a plain message is informative");
            Assert.That(label.ClassListContains("sc-shot-note--perfect")||label.ClassListContains("sc-shot-note--valid")||label.ClassListContains("sc-shot-note--failed"),Is.False);
        }
        [UnityTest] public IEnumerator ShotNote_PopsOnceForAJudgedStroke_AndSettlesAtItsSize()
        {
            yield return Boot(false);yield return null;
            var note=controller.GetComponent<ShotNoteView>();var label=Find<Label>("shot-note");
            float Scale()=>label.style.scale.keyword==StyleKeyword.Undefined?label.style.scale.value.value.x:1;
            foreach(var result in new[]{"Perfect","Valid","Failed"})
            {
                note.Show(new ShotRecord{Result=result,Fail="Air"});note.Tick(0);
                Assert.That(Scale(),Is.GreaterThan(1.2f),result+" pops");
                note.Tick(ShotNoteView.PopSeconds*.5f);
                Assert.That(Scale(),Is.InRange(1.0001f,1.2f),result+" is settling");
                note.Tick(ShotNoteView.PopSeconds);
                Assert.That(Scale(),Is.EqualTo(1).Within(1e-3f),result+" settles at its size");
            }
            note.Show(new ShotRecord{Result="NoOrder"});note.Tick(0);
            Assert.That(Scale(),Is.EqualTo(1).Within(1e-3f),"an informative note does not pop");
        }
        [UnityTest] public IEnumerator ShotNote_SitsInsideTheScreen_AboveTheSafeBottom()
        {
            yield return Boot(false);yield return null;
            var note=controller.GetComponent<ShotNoteView>();var label=Find<Label>("shot-note");
            // The controller (off in this fixture) applies timing.showNotes every frame; do what it does.
            note.Show(new ShotRecord{Result="Perfect"});note.SetVisible(true);note.Tick(ShotNoteView.PopSeconds);yield return null;yield return null;yield return null;
            Assert.That(Shown(label),Is.True,"the note is on screen");
            var b=label.worldBound;var all=root.worldBound;float bottom=app.GetComponent<SquashClash.Runtime.App.SafeAreaLayout>().Bottom;
            Assert.That(b.xMin,Is.GreaterThanOrEqualTo(all.xMin));Assert.That(b.xMax,Is.LessThanOrEqualTo(all.xMax));
            Assert.That(b.yMin,Is.GreaterThanOrEqualTo(all.yMin));Assert.That(b.yMax,Is.LessThanOrEqualTo(all.yMax-bottom),"above the home indicator");
            Assert.That(b.height,Is.GreaterThanOrEqualTo(label.resolvedStyle.fontSize),"not squashed");
            Assert.That(b.yMax,Is.GreaterThan(all.center.y),"under the court, in the lower half");
        }
        // Control (it held before the redesign too): WCAG AA, 4.5:1 for text and 3:1 for large text.
        [UnityTest] public IEnumerator PlayerText_IsReadable_OnTheMenuTutorialHeaderAndPause()
        {
            yield return Boot(true);yield return null;yield return null;
            AssertReadable("menu-layer");
            Click("menu-howto");Refresh();
            for(int step=0;step<SquashClash.Runtime.Menu.FirstRunFlow.TutorialSteps;step++){yield return null;yield return null;AssertReadable("tutorial-layer");Click("tutorial-next");Refresh();}
            yield return null;
            app.DevTools=false;Refresh();
            yield return StartMatch();Drive(()=>app.Host.Current.Step>10,1);yield return null;yield return null;
            AssertReadable("boot-header");
            Click("rally-pause");Refresh();yield return null;yield return null;
            AssertReadable("pause-card");
        }
        private static float Channel(float c)=>c<=.04045f?c/12.92f:Mathf.Pow((c+.055f)/1.055f,2.4f);
        private static float Luminance(Color c)=>.2126f*Channel(c.r)+.7152f*Channel(c.g)+.0722f*Channel(c.b);
        private static Color Over(Color top,Color under)=>new Color(Mathf.Lerp(under.r,top.r,top.a),Mathf.Lerp(under.g,top.g,top.a),Mathf.Lerp(under.b,top.b,top.a),1);
        // What the text sits on: the element's own background and its ancestors', down to the first nearly opaque one.
        private static Color Backdrop(VisualElement e)
        {
            var layers=new List<Color>();
            for(var v=e;v!=null;v=v.parent){var bg=v.resolvedStyle.backgroundColor;if(bg.a<=0)continue;layers.Add(bg);if(bg.a>=.85f)break;}
            var result=layers.Count>0&&layers[layers.Count-1].a>=.85f?new Color(layers[layers.Count-1].r,layers[layers.Count-1].g,layers[layers.Count-1].b,1):Color.black;
            for(int i=layers.Count-2;i>=0;i--)result=Over(layers[i],result);
            return result;
        }
        private void AssertReadable(string layer)
        {
            int seen=0;
            foreach(var e in ShownUnder(layer).OfType<TextElement>())
            {
                if(string.IsNullOrEmpty(e.text)||!e.enabledInHierarchy)continue;seen++;
                var back=Backdrop(e);var fore=Over(e.resolvedStyle.color,back);
                float a=Luminance(fore),b=Luminance(back),ratio=(Mathf.Max(a,b)+.05f)/(Mathf.Min(a,b)+.05f);
                bool large=e.resolvedStyle.fontSize>=24||e.resolvedStyle.fontSize>=18.6f&&!FontName(e).EndsWith("Regular");
                Assert.That(ratio,Is.GreaterThanOrEqualTo(large?3f:4.5f),layer+"/"+(string.IsNullOrEmpty(e.name)?e.text:e.name)+" fore "+fore+" back "+back);
            }
            Assert.That(seen,Is.GreaterThan(0),layer+" shows text");
        }
    }
}
