using System;
using SquashClash.Core.Diagnostics;
using SquashClash.Core.Config;
using SquashClash.Runtime.Court;
using SquashClash.Runtime.Camera;
using UnityEngine;
using UnityEngine.UIElements;

namespace SquashClash.Runtime.App
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class AppBootstrap : MonoBehaviour
    {
        public static AppBootstrap Instance { get; private set; }
        public BuildInfo Build { get; private set; }
        public ParamSet Params { get; private set; }
        public EffectiveConfig Config { get; private set; }

        [SerializeField] private CourtView courtView;
        [SerializeField] private PlaceholderPlayers placeholderPlayers;
        [SerializeField] private CourtCameraRig cameraRig;

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
            var defaults = Resources.Load<TextAsset>("DefaultPreset");
            if (defaults == null) throw new InvalidOperationException("Missing DefaultPreset.json.");
            Params = PresetCodec.LoadDefault(defaults.text);
            Config = EffectiveConfig.From(Params);
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
            if (courtView == null || placeholderPlayers == null || cameraRig == null)
                throw new InvalidOperationException("The main scene must reference court, camera and players.");
            courtView.Build(Config.Court);
            placeholderPlayers.Apply(Params);
            cameraRig.Apply(Params, Config.Court);

            var root = GetComponent<UIDocument>().rootVisualElement;
            root.Clear();
            root.pickingMode = PickingMode.Ignore;
            root.style.flexGrow = 1;
            root.style.alignItems = Align.Center;
            root.style.paddingTop = 60;
            root.style.backgroundColor = Color.clear;
            var header = new VisualElement { name = "boot-header", pickingMode = PickingMode.Ignore };
            header.style.alignItems = Align.Center;
            header.style.maxWidth = Length.Percent(94);
            header.style.paddingLeft = 12;
            header.style.paddingRight = 12;
            header.style.paddingTop = 8;
            header.style.paddingBottom = 8;
            header.style.backgroundColor = new Color(0.025f, 0.06f, 0.08f, 0.85f);
            root.Add(header);
            var title = new Label("Squash Clash — First Ball") { name = "boot-title", pickingMode = PickingMode.Ignore };
            title.style.fontSize = 18;
            title.style.color = Color.white;
            header.Add(title);
            var row = new VisualElement { pickingMode = PickingMode.Ignore };
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginTop = 4;
            header.Add(row);
            var buildLabel = new Label(Build.Label) { name = "build-label", pickingMode = PickingMode.Ignore };
            buildLabel.style.fontSize = 13;
            buildLabel.style.color = new Color(0.77f, 0.85f, 0.88f);
            row.Add(buildLabel);
            fpsLabel = new Label("… FPS") { name = "fps-label", pickingMode = PickingMode.Ignore };
            fpsLabel.style.fontSize = 13;
            fpsLabel.style.marginLeft = 12;
            fpsLabel.style.color = new Color(0.72f, 0.93f, 0.83f);
            row.Add(fpsLabel);
            Debug.Log($"[SquashClash] court ready lines=12 surfaces=5 colliders=0 config={Config.Hash} tin={Config.Court.TinTop}");
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
