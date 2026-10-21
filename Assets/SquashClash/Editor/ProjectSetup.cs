using System;
using System.IO;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Court;
using SquashClash.Runtime.Camera;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace SquashClash.EditorTools
{
    public static class ProjectSetup
    {
        public const string MainScene = "Assets/SquashClash/Scenes/Main.unity";

        public static void Apply()
        {
            var bundleId = Environment.GetEnvironmentVariable("BUNDLE_ID");
            if (string.IsNullOrWhiteSpace(bundleId))
                throw new InvalidOperationException("BUNDLE_ID is required.");

            EditorSettings.serializationMode = SerializationMode.ForceText;
            VersionControlSettings.mode = "Visible Meta Files";
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            settings.FindProperty("activeInputHandler").intValue = 1;
            settings.ApplyModifiedPropertiesWithoutUndo();

            PlayerSettings.companyName = "Rafael Allendes";
            PlayerSettings.productName = "Squash Clash";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, bundleId);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.iOS, Il2CppCompilerConfiguration.Release);
            PlayerSettings.SetArchitecture(NamedBuildTarget.iOS, 1);
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            PlayerSettings.iOS.appleDeveloperTeamID = Environment.GetEnvironmentVariable("TEAM_ID") ?? "";
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneOnly;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.enableFrameTimingStats = true;

            // Static project baseline required by T1.2; no ball simulation in E1.
            Physics.simulationMode = SimulationMode.Script;
            Physics.bounceThreshold = 0.1f;
            Physics.defaultContactOffset = 0.001f;
            var physics = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/DynamicsManager.asset")[0]);
            physics.FindProperty("m_EnableEnhancedDeterminism").boolValue = true;
            physics.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory("Assets/SquashClash/Runtime/App");
            Directory.CreateDirectory("Assets/SquashClash/Runtime/Resources");
            Directory.CreateDirectory("Assets/SquashClash/Scenes");
            AssetDatabase.Refresh();
            const string rendererPath = "Assets/SquashClash/Runtime/App/MainRenderer.asset";
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, rendererPath);
            }
            const string pipelinePath = "Assets/SquashClash/Runtime/App/MainPipeline.asset";
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, pipelinePath);
            }
            GraphicsSettings.defaultRenderPipeline = pipeline;
            var quality = QualitySettings.GetQualityLevel();
            for (var i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }
            QualitySettings.SetQualityLevel(quality, false);

            var scene = File.Exists(MainScene)
                ? EditorSceneManager.OpenScene(MainScene, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.09f, 0.12f);
            if (!camera.TryGetComponent<UniversalAdditionalCameraData>(out _))
                camera.gameObject.AddComponent<UniversalAdditionalCameraData>();

            const string panelPath = "Assets/SquashClash/Runtime/App/AppPanelSettings.asset";
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelPath);
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(panel, panelPath);
            }
            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = new Vector2Int(390, 844);
            panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panel.match = 0.5f;
            panel.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Assets/SquashClash/Runtime/App/DefaultRuntimeTheme.tss");
            if (panel.themeStyleSheet == null)
                throw new InvalidOperationException("The default runtime theme must be imported.");
            EditorUtility.SetDirty(panel);
            var bootstrap = UnityEngine.Object.FindFirstObjectByType<AppBootstrap>();
            if (bootstrap == null) bootstrap = new GameObject("App").AddComponent<AppBootstrap>();
            bootstrap.GetComponent<UIDocument>().panelSettings = panel;
            Directory.CreateDirectory("Assets/SquashClash/Runtime/Resources/CourtMaterials");
            AssetDatabase.Refresh();
            CourtMaterial("CourtWall", new Color(0.94f, 0.95f, 0.96f));
            CourtMaterial("CourtFloor", Color.white);
            CourtMaterial("CourtLine", new Color(0.78f, 0.055f, 0.07f), false, true);
            CourtMaterial("CourtGlass", new Color(0.7f, 0.84f, 0.9f, 0.25f), true, true);
            CourtMaterial("CourtTin", new Color(0.40f, 0.45f, 0.48f));
            CourtMaterial("CourtBlue", new Color(0.04f, 0.38f, 0.95f));
            CourtMaterial("CourtRed", new Color(0.95f, 0.12f, 0.16f));
            var court = UnityEngine.Object.FindFirstObjectByType<CourtView>();
            if (court == null) court = new GameObject("Court").AddComponent<CourtView>();
            var players = UnityEngine.Object.FindFirstObjectByType<PlaceholderPlayers>();
            if (players == null) players = new GameObject("Reference Players").AddComponent<PlaceholderPlayers>();
            var rig = camera.GetComponent<CourtCameraRig>();
            if (rig == null) rig = camera.gameObject.AddComponent<CourtCameraRig>();
            var app = new SerializedObject(bootstrap);
            app.FindProperty("courtView").objectReferenceValue = court;
            app.FindProperty("placeholderPlayers").objectReferenceValue = players;
            app.FindProperty("cameraRig").objectReferenceValue = rig;
            app.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene, MainScene);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(MainScene, true) };
            AssetDatabase.SaveAssets();
            Debug.Log($"[SquashClash] setup complete unity={Application.unityVersion} bundle={bundleId} minIOS={PlayerSettings.iOS.targetOSVersionString}");
        }
        private static void CourtMaterial(string name, Color color, bool transparent = false, bool twoSided = false)
        {
            var path = "Assets/SquashClash/Runtime/Resources/CourtMaterials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Cull", twoSided ? 0 : 2);
            material.SetFloat("_Surface", transparent ? 1 : 0);
            material.SetFloat("_ZWrite", transparent ? 0 : 1);
            material.SetFloat("_SrcBlend", transparent ? (float)BlendMode.SrcAlpha : (float)BlendMode.One);
            material.SetFloat("_DstBlend", transparent ? (float)BlendMode.OneMinusSrcAlpha : (float)BlendMode.Zero);
            material.SetFloat("_SrcBlendAlpha", 1);
            material.SetFloat("_DstBlendAlpha", transparent ? (float)BlendMode.OneMinusSrcAlpha : 0);
            if (transparent) material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            else material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", transparent ? "Transparent" : "Opaque");
            material.renderQueue = transparent ? (int)RenderQueue.Transparent : (int)RenderQueue.Geometry;
            EditorUtility.SetDirty(material);
        }

    }
}
