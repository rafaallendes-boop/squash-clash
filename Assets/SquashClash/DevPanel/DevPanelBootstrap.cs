using UnityEngine;
using UnityEngine.SceneManagement;
namespace SquashClash.DevPanel
{
    public static class DevPanelBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            CreateBar();
        }
        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => CreateBar();
        private static void CreateBar()
        {
            if (Object.FindFirstObjectByType<DevBar>() == null) new GameObject("First Ball Dev Bar").AddComponent<DevBar>();
        }
    }
}
