using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

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
            EditorSceneManager.SaveScene(scene, MainScene);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(MainScene, true) };
            AssetDatabase.SaveAssets();
            Debug.Log($"[SquashClash] setup complete unity={Application.unityVersion} bundle={bundleId} minIOS={PlayerSettings.iOS.targetOSVersionString}");
        }
    }
}
