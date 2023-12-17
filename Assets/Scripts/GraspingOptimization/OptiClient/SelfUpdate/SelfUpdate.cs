using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using Unity.VisualScripting;
using System.IO;

namespace GraspingOptimization
{
    public class SelfUpdate : MonoBehaviour
    {
        public static SelfUpdate instance;

        [SerializeField]
        private GameObject updateButton;

        private void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(this);
        }

        public bool IsUpdateRequired()
        {
            UpdateInfo updateInfo = GetUpdateInfo();
            if (updateInfo == null)
            {
                // ネットワークフォルダが見つからない場合など
                Debug.Log("updateInfo is null");
                updateButton.SetActive(false);
                return false;
            }

            bool isUpdateRequired;
            if (updateInfo.appVersion == Application.version)
            {
                updateButton.SetActive(false);
                isUpdateRequired = false;
            }
            else
            {
                updateButton.SetActive(true);
                isUpdateRequired = true;
            }
#if UNITY_EDITOR
            updateButton.SetActive(false);
#endif
            return isUpdateRequired;
        }

        public UpdateInfo GetUpdateInfo()
        {
            UpdateInfo updateInfo = null;
            string updateInfoPath = LocalConfig.updateInfoPath;
            try
            {
                string updateInfoJson = System.IO.File.ReadAllText(updateInfoPath);
                updateInfo = JsonUtility.FromJson<UpdateInfo>(updateInfoJson);
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
                return null; // ネットワークフォルダが見つからない場合など
            }
            return updateInfo;
        }

        public void UpdateApp(string updateAppDirectory)
        {
            // 現在のアプリケーションがあるディレクトリを取得
            string cureentDirectory = Directory.GetCurrentDirectory();
            Debug.Log($"current directory: {cureentDirectory}");
            // アプリケーションをコピー
            try
            {
                DirectoryCopier.CopyDirectory(updateAppDirectory, cureentDirectory);
                Debug.Log($"copy {updateAppDirectory} to {cureentDirectory}");
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
        }

        public void OnUpdateButtonClicked()
        {
            UpdateInfo updateInfo = GetUpdateInfo();
            Debug.Log($"updateInfo: {updateInfo.appVersion}, {updateInfo.appDirectory}");
            if (IsUpdateRequired())
            {
                UpdateApp(updateInfo.appDirectory);
                Debug.Log("update completed");
                Application.Quit();
            }
            else
            {
                string currentVersion = Application.version;
                Debug.Log($"no update required, current version: {currentVersion}");
            }
        }

        public void OnClickExportUpdateInfo()
        {
            UpdateInfo updateInfo = new UpdateInfo();
            updateInfo.appDirectory = LocalConfig.updateAppDirectory;
            updateInfo.appVersion = Application.version;
            string updateInfoPath = LocalConfig.updateInfoPath;
            updateInfo.ExportJson(updateInfoPath);
        }
    }
}