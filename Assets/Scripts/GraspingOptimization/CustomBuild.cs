#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using GraspingOptimization;

namespace GraspingOptimization
{
    public class CustomBuildScript
    {
        [MenuItem("Build/Build Client")]
        public static void BuildGameWithVersion()
        {
            string tempDirectory = "build-app/temp";
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
            Directory.CreateDirectory(tempDirectory);
            string updateAppDirectory = LocalConfig.updateAppDirectory;
            // ビルド設定
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { "Assets/Scenes/HandOpti.unity" }; // ビルドするシーンを指定
            buildPlayerOptions.locationPathName = $"{tempDirectory}/opti-client-v" + Application.version + ".exe"; // アプリ名にバージョンを追加
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64; // ビルドターゲットを指定
            buildPlayerOptions.options |= BuildOptions.Development; // 開発ビルドを有効にする
            buildPlayerOptions.options |= BuildOptions.ConnectWithProfiler; // プロファイラと接続する

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
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
        }
    }
}
#endif
