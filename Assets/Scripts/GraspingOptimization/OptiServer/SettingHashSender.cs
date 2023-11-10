using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using System.Net.Sockets;
using System.Net;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;
using UniRx;
using MongoDB.Driver;
using OpenCvSharp;

namespace GraspingOptimization
{
    public class SettingHashSender : MonoBehaviour
    {
        public static SettingHashSender Instance { get; private set; }

        [SerializeField]
        List<string> optiSettingHasheList;
        [SerializeField]
        List<string> envSettingHasheList;
        [SerializeField]
        List<string> sequenceDtList;

        [SerializeField]
        List<SettingHash> settingHashList;

        [SerializeField] int port;

        private WebSocketServer server;

        // クライアントの接続状態を追跡するためのディクショナリ
        public Dictionary<string, OptiClientInfo> clientInfos = new Dictionary<string, OptiClientInfo>();

        private float totalStepsPerSecond = 0.0f;



        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            server = new WebSocketServer(port);
            server.AddWebSocketService<ResSettingHash>("/");


            server.Start();

            foreach (string optiSettingHash in optiSettingHasheList)
            {
                foreach (string envSettingHash in envSettingHasheList)
                {
                    foreach (string sequenceDt in sequenceDtList)
                    {
                        settingHashList.Add(new SettingHash(optiSettingHash, envSettingHash, sequenceDt));
                    }
                }
            }

        }

        public SettingHash GetSettingHash()
        {
            if (settingHashList.Count == 0)
            {
                return null;
            }
            SettingHash settingHash = settingHashList[0];
            settingHashList.RemoveAt(0);
            return settingHash;
        }

        void OnDestroy()
        {
            server.Stop();
            server = null;
        }

        public void UpdateClientState(string clientId, OptiClientInfo clientInfo)
        {
            if (clientInfos.ContainsKey(clientId))
            {
                if (clientInfo.clientState == ClientState.Disconnected)
                {
                    clientInfos.Remove(clientId);
                }
                else
                {
                    clientInfos[clientId] = clientInfo;
                    totalStepsPerSecond = 0.0f;
                    foreach (var info in clientInfos)
                    {
                        totalStepsPerSecond += info.Value.stepsPerSecond;
                    }
                }
            }
            else
            {
                clientInfos.Add(clientId, clientInfo);
                totalStepsPerSecond = 0.0f;
                foreach (var info in clientInfos)
                {
                    totalStepsPerSecond += info.Value.stepsPerSecond;
                }
            }
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new UnityEngine.Rect(10, 10, Screen.width - 10, Screen.height - 10)); // 位置とサイズを指定
            GUILayout.BeginVertical();

            GUILayout.Label($"Client Number: {clientInfos.Count}, Total steps/sec: {totalStepsPerSecond.ToString("f1")}");
            foreach (var clientInfo in clientInfos)
            {
                string text = $"Client ID: {clientInfo.Key}, State: {clientInfo.Value.clientState}, StepsPerSecond: {clientInfo.Value.stepsPerSecond.ToString("f1")}";
                GUILayout.Label(text);
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }

    public class ResSettingHash : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            Debug.Log("connected");
            OptiClientInfo clientInfo = new OptiClientInfo(ClientState.Connected);
            SettingHashSender.Instance.UpdateClientState(ID, clientInfo);
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            OptiClientInfo clientInfo = JsonUtility.FromJson<OptiClientInfo>(e.Data);
            if (clientInfo.clientState == ClientState.Waiting)
            {
                SettingHash settingHash = SettingHashSender.Instance.GetSettingHash();
                if (settingHash != null)
                {
                    string json = JsonUtility.ToJson(settingHash);
                    Send(json);
                }
                SettingHashSender.Instance.UpdateClientState(ID, clientInfo);
            }
            else if (clientInfo.clientState == ClientState.Running)
            {
                SettingHashSender.Instance.UpdateClientState(ID, clientInfo);
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Debug.Log("closed");
            OptiClientInfo clientInfo = new OptiClientInfo(ClientState.Disconnected);
            SettingHashSender.Instance.UpdateClientState(ID, clientInfo);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            Debug.Log("error");
            OptiClientInfo clientInfo = new OptiClientInfo(ClientState.Error);
            SettingHashSender.Instance.UpdateClientState(ID, clientInfo);
        }
    }
}