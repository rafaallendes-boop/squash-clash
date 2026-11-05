using System.Collections;
using NUnit.Framework;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Input;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    public sealed class S2DefaultStartupTests
    {
        [UnityTest] public IEnumerator MainDefault_EntersM24_WithoutExplicitControlOverride()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            var app=AppBootstrap.Instance;app.Host.enabled=false;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            var input=app.GetComponent<TouchInputAdapter>();input.UiBlocker=_=>false;
            var screen=InputSystem.AddDevice<Touchscreen>();
            try
            {
                var point=(Vector2)UnityEngine.Camera.main.WorldToScreenPoint(new Vector3(3.2f,2.2f,0));
                double now=System.Math.Ceiling(Time.realtimeSinceStartupAsDouble)+1;input.ReadRealTime=()=>now;
                InputSystem.QueueStateEvent(screen,new TouchState{touchId=321,phase=UnityEngine.InputSystem.TouchPhase.Began,position=point},now);InputSystem.Update();
                Assert.That(app.GetComponent<GesturePreview>().M24,Is.Not.Null,"Main must use control2 from defaults, no fixture override");
            }
            finally {InputSystem.RemoveDevice(screen);}
            yield return PlayModeTestWorld.ClearApp();
        }
    }
}
