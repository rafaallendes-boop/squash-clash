using System.Collections;
using System.Linq;
using NUnit.Framework;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace SquashClash.Runtime.Tests {
    internal static class PlayModeTestWorld {
        internal static IEnumerator ClearApp() {
            if(AppBootstrap.Instance!=null) Object.Destroy(AppBootstrap.Instance.gameObject);
            yield return null;
            for(int i=0;i<120 && HasPhysicsScenes();i++) yield return null;
            Assert.That(HasPhysicsScenes(),Is.False,"Previous app-owned physics scenes must unload.");
        }
        private static bool HasPhysicsScenes()=>Enumerable.Range(0,SceneManager.sceneCount).Select(SceneManager.GetSceneAt).Any(s=>s.name.StartsWith("P1-PhysX-"));
    }
}
