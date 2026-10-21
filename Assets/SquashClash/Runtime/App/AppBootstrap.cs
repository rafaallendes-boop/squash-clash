using System;
using SquashClash.Core.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;

namespace SquashClash.Runtime.App
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class AppBootstrap : MonoBehaviour
    {
        public static AppBootstrap Instance { get; private set; }
        public BuildInfo Build { get; private set; }

        private Label fpsLabel;
        private int frames;
        private float elapsed;
        private double startedAt;
        private bool reportedDisplay;

        private void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;
            startedAt = Time.realtimeSinceStartupAsDouble;
            var metadata = Resources.Load<TextAsset>("BuildInfo");
            if (metadata != null)
                Build = BuildInfo.FromJson(metadata.text);
#if UNITY_EDITOR
            else
                Build = new BuildInfo { version = Application.version, gitHash = "editor", profile = "editor", unityVersion = Application.unityVersion };
#endif
            if (Build == null)
                throw new InvalidOperationException("Missing build metadata. Use scripts/build-ios.sh.");
            var hasPanel = Type.GetType("SquashClash.DevPanel.DevPanelBootstrap, SquashClash.DevPanel") != null;
#if SQUASH_DEV
            const bool squashDev = true;
#else
            const bool squashDev = false;
#endif
            Debug.Log($"[SquashClash] boot {Build.Label} devpanel={(hasPanel ? "present" : "absent")} development={Debug.isDebugBuild} squashDev={squashDev} unity={Application.unityVersion}");
        }

        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            root.Clear();
            root.style.flexGrow = 1;
            root.style.justifyContent = Justify.Center;
            root.style.alignItems = Align.Center;
            root.style.paddingLeft = 24;
            root.style.paddingRight = 24;
            root.style.backgroundColor = new Color(0.035f, 0.09f, 0.12f);

            var title = new Label("Squash Clash — First Ball") { name = "boot-title" };
            title.style.fontSize = 28;
            title.style.whiteSpace = WhiteSpace.Normal;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            title.style.color = Color.white;
            root.Add(title);

            var buildLabel = new Label(Build.Label) { name = "build-label" };
            buildLabel.style.fontSize = 16;
            buildLabel.style.marginTop = 16;
            buildLabel.style.color = new Color(0.77f, 0.85f, 0.88f);
            root.Add(buildLabel);

            fpsLabel = new Label("… FPS") { name = "fps-label" };
            fpsLabel.style.fontSize = 24;
            fpsLabel.style.marginTop = 24;
            fpsLabel.style.color = new Color(0.72f, 0.93f, 0.83f);
            root.Add(fpsLabel);
        }

        private void Update()
        {
            if (fpsLabel == null) return;
            elapsed += Time.unscaledDeltaTime;
            frames++;
            if (elapsed < 0.5f) return;
            var fps = frames / elapsed;
            fpsLabel.text = $"{fps:0} FPS";
            frames = 0;
            elapsed = 0;
            if (!reportedDisplay && Time.realtimeSinceStartupAsDouble - startedAt >= 3)
            {
                reportedDisplay = true;
                Debug.Log($"[SquashClash] display profile={Build.profile} fps={fps:0.0} orientation={Screen.orientation} resolution={Screen.width}x{Screen.height}");
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
