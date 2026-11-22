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
        [UnityTest] public IEnumerator ServeGuideUsesTheShotNoteVoiceWithoutABox()
        {
            yield return Boot(false);
            var guide = Find("serve-guide");
            Assert.That(guide.pickingMode, Is.EqualTo(PickingMode.Ignore));
            Assert.That(Find("serve-guide-line").pickingMode, Is.EqualTo(PickingMode.Ignore));
            // IResolvedStyle omits text-shadow in this Unity version; read the same computed value.
            const System.Reflection.BindingFlags styleFlags = System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            var styleField = typeof(VisualElement).GetField("m_Style", styleFlags);
            var shadowProperty = styleField.FieldType.GetProperty("textShadow", styleFlags);
            TextShadow Shadow(VisualElement element) => (TextShadow)shadowProperty.GetValue(styleField.GetValue(element));
            foreach (var expected in new[] { (name: "serve-guide-lob", text: "Toca arriba: globo"), (name: "serve-guide-drive", text: "Toca abajo: drive") })
            {
                var label = Find<Label>(expected.name);
                Assert.That(label.text, Is.EqualTo(expected.text));
                Assert.That(label.pickingMode, Is.EqualTo(PickingMode.Ignore));
                Assert.That(FontName(label), Is.EqualTo("Rubik-BoldItalic"));
                Assert.That(label.resolvedStyle.fontSize, Is.EqualTo(20));
                Assert.That(label.resolvedStyle.backgroundColor.a, Is.Zero);
                Assert.That(Shadow(label), Is.EqualTo(Shadow(Find("shot-note"))));
            }
        }

        [UnityTest] public IEnumerator ShotNote_TakesTheToneOfTheResult_AndKeepsItsWords()
        {
            yield return Boot(false);yield return null;
            var note=controller.GetComponent<ShotNoteView>();var label=Find<Label>("shot-note");
            var cases=new (ShotRecord record,string text,string tone)[]
            {
                (new ShotRecord{Result="Perfect"},"Perfecto","perfect"),
                (new ShotRecord{Result="Valid",DeltaMs=-35},"Adelantado","valid"),(new ShotRecord{Result="Valid",DeltaMs=35},"Atrasado","valid"),
                (new ShotRecord{Result="Failed",Fail="Air"},"¡Temprano!","failed"),(new ShotRecord{Result="Failed",Fail="Late"},"¡Tarde!","failed"),
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
        // Shot note in the band (21 nov, PO): the note sits centred in the empty band between the court's bottom edge and the
        // safe bottom, never over the court. The court is pulled up (camera rect) so a band exists whatever the test screen is.
        private IEnumerator NoteInBandSetup()
        {
            yield return Boot(false);yield return null;
            UnityEngine.Camera.main.rect=new Rect(0,.3f,1,.7f);yield return null;
            var note=controller.GetComponent<ShotNoteView>();
            note.Show(new ShotRecord{Result="Perfect"});note.SetVisible(true);note.Tick(ShotNoteView.PopSeconds);yield return null;yield return null;yield return null;
        }
        [UnityTest] public IEnumerator ShotNote_PanelClosed_SitsBelowTheCourt_InTheBand()
        {
            yield return NoteInBandSetup();
            var note=controller.GetComponent<ShotNoteView>();var label=Find<Label>("shot-note");
            var view=Object.FindFirstObjectByType<SquashClash.DevPanel.DevPanelView>();if(view!=null&&view.IsOpen)view.Toggle();yield return null;yield return null;
            var all=root.worldBound;float safeBottom=app.GetComponent<SquashClash.Runtime.App.SafeAreaLayout>().Bottom;
            float courtBottomY=all.yMax-note.CourtBottomEdge();var b=label.worldBound;
            Assert.That(note.CourtBottomEdge(),Is.GreaterThan(20f),"the court leaves a band");
            Assert.That(b.yMin,Is.GreaterThanOrEqualTo(courtBottomY-.5f),"entirely under the court's bottom edge");
            Assert.That(b.yMax,Is.LessThanOrEqualTo(all.yMax-safeBottom+.5f),"above the safe bottom");
            Assert.That((b.yMin+b.yMax)/2,Is.EqualTo((courtBottomY+all.yMax-safeBottom)/2).Within(2f),"centred in the band");
        }
        [UnityTest] public IEnumerator ShotNote_PanelOpen_StaysAboveTheVisibleBar()
        {
            yield return NoteInBandSetup();
            var label=Find<Label>("shot-note");
            var view=Object.FindFirstObjectByType<SquashClash.DevPanel.DevPanelView>();Assert.That(view,Is.Not.Null);if(!view.IsOpen)view.Toggle();yield return null;yield return null;yield return null;
            var bar=root.Q("dev-bar");Assert.That(bar.resolvedStyle.visibility,Is.EqualTo(Visibility.Visible),"bar shown while open");
            Assert.That(label.worldBound.yMax,Is.LessThanOrEqualTo(bar.worldBound.yMin+.5f),"above the bar");
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
        // Menu redesign (19 nov): one loud action and one loud word. Jugar is clearly the biggest thing a finger can hit
        // (at least 1.4 times the area of any other menu button) and the wordmark is the biggest type on the menu, at display size.
        [UnityTest] public IEnumerator Menu_PlayAndTheWordmarkLeadTheHierarchy()
        {
            yield return Boot(true);app.DevTools=false;Refresh();yield return null;yield return null;
            var play=Find<Button>("menu-play").worldBound;float playArea=play.width*play.height;
            foreach(var b in ShownUnder("menu-layer").OfType<Button>())
            {
                if(b.name=="menu-play")continue;
                Assert.That(playArea,Is.GreaterThanOrEqualTo(1.4f*b.worldBound.width*b.worldBound.height),"Jugar against "+b.name);
            }
            float title=Find("menu-title").resolvedStyle.fontSize;
            Assert.That(title,Is.GreaterThanOrEqualTo(56),"the wordmark is set at display size");
            foreach(var t in ShownUnder("menu-layer").OfType<TextElement>())
                if(t.name!="menu-title")Assert.That(t.resolvedStyle.fontSize,Is.LessThan(title),t.name+" is quieter than the wordmark");
        }
        // The sound, music and hand switches show their state by look as well as by word: an off switch is marked.
        [UnityTest] public IEnumerator Menu_OffSwitchesAreMarked()
        {
            SquashClash.Runtime.Audio.AudioScape.SoundOn=true;SquashClash.Runtime.Audio.MusicPlayer.MusicOn=true;
            yield return Boot(true);yield return null;
            var sound=Find<Button>("menu-sound");
            Assert.That(sound.ClassListContains("sc-toggle--off"),Is.False,"sound starts on");
            flow.ToggleSound();
            Assert.That(sound.ClassListContains("sc-toggle--off"),Is.True,"sound off is marked");
            Assert.That(Find<Button>("pause-sound").ClassListContains("sc-toggle--off"),Is.True,"in the pause card too");
            flow.ToggleSound();
            Assert.That(sound.ClassListContains("sc-toggle--off"),Is.False);
            var music=Find<Button>("menu-music");flow.ToggleMusic();
            Assert.That(music.ClassListContains("sc-toggle--off"),Is.True,"music off is marked");flow.ToggleMusic();
        }
        [UnityTest] public IEnumerator Credits_DrawWithRubik_AndAreReadable()
        {
            yield return Boot(true);yield return null;
            Click("menu-credits");Refresh();yield return null;yield return null;
            Assert.That(FontName(Find("credits-title")),Is.EqualTo("Rubik-BoldItalic"));
            Assert.That(FontName(Find("credits-font-copyright")),Is.EqualTo("Rubik-Regular"));
            Assert.That(FontName(Find("credits-back")),Is.EqualTo("Rubik-Bold"));
            AssertReadable("credits-layer");
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
