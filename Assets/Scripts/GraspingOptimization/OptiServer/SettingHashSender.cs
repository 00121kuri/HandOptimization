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
        public Dictionary<string, ClientState> clientStates = new Dictionary<string, ClientState>();



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

        public void UpdateClientState(string clientId, ClientState state)
        {
            if (clientStates.ContainsKey(clientId))
            {
                if (state == ClientState.Disconnected)
                {
                    clientStates.Remove(clientId);
                }
                else
                {
                    clientStates[clientId] = state;
                }
            }
            else
            {
                clientStates.Add(clientId, state);
            }
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new UnityEngine.Rect(10, 10, Screen.width - 10, Screen.height - 10)); // 位置とサイズを指定
            GUILayout.BeginVertical();

            foreach (var clientState in clientStates)
            {
                string text = $"Client ID: {clientState.Key}, State: {clientState.Value}";
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
            SettingHashSender.Instance.UpdateClientState(ID, ClientState.Connected);
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            Debug.Log(e.Data);
            if (e.Data == "request")
            {
                SettingHash settingHash = SettingHashSender.Instance.GetSettingHash();
                if (settingHash != null)
                {
                    string json = JsonUtility.ToJson(settingHash);
                    Send(json);
                }
                SettingHashSender.Instance.UpdateClientState(ID, ClientState.Requested);
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Debug.Log("closed");
            SettingHashSender.Instance.UpdateClientState(ID, ClientState.Disconnected);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            Debug.Log("error");
            SettingHashSender.Instance.UpdateClientState(ID, ClientState.Error);
        }
    }

    public enum ClientState
    {
        Connected,
        Requested,
        Disconnected,
        Error
    }
}