#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using GraspingOptimization;

namespace GraspingOptimization
{
    public class CustomBuildWindow : EditorWindow
    {
        private string optiClientOutputPath = "build-app/opti-client";

        private string mlGraspingOutputPath = "build-app/ml-grasping";
        private Vector2 scrollPosition;

        [MenuItem("Build/Custom Build Window")]
        public static void ShowWindow()
        {
            GetWindow<CustomBuildWindow>("Custom Build");
        }

        void OnGUI()
        {


            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Opti Client Build Settings", EditorStyles.boldLabel);

            optiClientOutputPath = EditorGUILayout.TextField("Output Path:", optiClientOutputPath);

            if (GUILayout.Button("Browse"))
            {
                string path = EditorUtility.SaveFolderPanel("Choose Output Folder", optiClientOutputPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    optiClientOutputPath = path;
                }
            }

            if (GUILayout.Button("Build Opti Client"))
            {
                BuildOptiClient();
                GUIUtility.ExitGUI();
            }

            GUILayout.Space(20);

            GUILayout.Label("ML-Grasping Build Settings", EditorStyles.boldLabel);

            mlGraspingOutputPath = EditorGUILayout.TextField("Output Path:", mlGraspingOutputPath);

            if (GUILayout.Button("Browse"))
            {
                string path = EditorUtility.SaveFolderPanel("Choose Output Folder", mlGraspingOutputPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    mlGraspingOutputPath = path;
                }
            }

            if (GUILayout.Button("Build ML-Grasping"))
            {
                BuildMLGrasping();
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndScrollView();
        }

        private void BuildOptiClient()
        {
            if (string.IsNullOrEmpty(optiClientOutputPath))
            {
                EditorUtility.DisplayDialog("Error", "Please specify an output path.", "OK");
                return;
            }

            string tempDirectory = optiClientOutputPath;
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
            Directory.CreateDirectory(tempDirectory);
            string updateAppDirectory = LocalConfig.updateAppDirectory;

            // ビルド設定
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { "Assets/Scenes/HandOpti.unity" }; // ビルドするシーンを指定
            buildPlayerOptions.locationPathName = $"{tempDirectory}/opti-client-v{Application.version}.exe"; // アプリ名にバージョンを追加
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64; // ビルドターゲットを指定
            buildPlayerOptions.options |= BuildOptions.Development; // 開発ビルドを有効にする
            //buildPlayerOptions.options |= BuildOptions.ConnectWithProfiler; // プロファイラと接続する

            // ビルドの実行
            BuildPipeline.BuildPlayer(buildPlayerOptions);

            try
            {
                // 保存先のディレクトリを削除
                if (Directory.Exists(updateAppDirectory))
                {
                    Directory.Delete(updateAppDirectory, true);
                }
                // 保存先のディレクトリを作成
                Directory.CreateDirectory(updateAppDirectory);

                // ビルド後に指定のディレクトリにコピー
                DirectoryCopier.CopyDirectory(tempDirectory, updateAppDirectory);

                // updateInfo.jsonの作成
                UpdateInfo updateInfo = new UpdateInfo();
                updateInfo.appDirectory = LocalConfig.updateAppDirectory;
                updateInfo.appVersion = Application.version;
                string updateInfoPath = LocalConfig.updateInfoPath;
                updateInfo.ExportJson(updateInfoPath);

                Debug.Log("Build completed");
                EditorUtility.DisplayDialog("Build Complete", "The build process has completed successfully.", "OK");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Build failed: {e}");
                EditorUtility.DisplayDialog("Build Failed", $"An error occurred during the build process: {e.Message}", "OK");
            }
        }

        private void BuildMLGrasping()
        {
            if (string.IsNullOrEmpty(optiClientOutputPath))
            {
                EditorUtility.DisplayDialog("Error", "Please specify an output path.", "OK");
                return;
            }

            // ビルド設定 Linux向け
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { "Assets/Scenes/ML-Grasping.unity" };
            buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
            buildPlayerOptions.locationPathName = $"{mlGraspingOutputPath}/ml-grasping.x86_64";
            buildPlayerOptions.options |= BuildOptions.Development;

            // 削除
            if (Directory.Exists(mlGraspingOutputPath))
            {
                Directory.Delete(mlGraspingOutputPath, true);
            }

            // ビルドの実行
            BuildPipeline.BuildPlayer(buildPlayerOptions);
        }
    }
}
#endif