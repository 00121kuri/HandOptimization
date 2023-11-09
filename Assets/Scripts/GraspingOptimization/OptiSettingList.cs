using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UniRx;
using WebSocketSharp;
using MongoDB.Driver;


namespace GraspingOptimization
{
    public class OptiSettingList : MonoBehaviour
    {
        [SerializeField] bool isClient;

        [SerializeField]
        List<string> optiSettingHasheList;
        [SerializeField]
        List<string> envSettingHasheList;
        [SerializeField]
        List<string> dtList;

        public int settingCount;

        public
        List<SettingHash> settingHashList;


        [SerializeField] string serverIp;
        [SerializeField] int serverPort;


        private WebSocket ws;

        private bool requestSent = false;

        public bool isWaiting = false;

        private float reconnectInterval = 5.0f; // 再接続の試みの間隔（秒）
        private float timeSinceLastConnectAttempt = 0.0f; // 最後に接続を試みてからの経過時間
        private bool tryReconnect = false;

        void Start()
        {
            settingHashList = new List<SettingHash>();
            if (isClient)
            {
                ws = new WebSocket($"ws://{serverIp}:{serverPort}/");
                ws.OnMessage += (sender, e) =>
                {
                    Debug.Log(e.Data);
                    SettingHash settingHash = JsonUtility.FromJson<SettingHash>(e.Data);
                    settingHashList.Add(settingHash);
                    requestSent = false;
                };
                ws.OnOpen += (sender, e) =>
                {
                    Debug.Log("WebSocket Open");
                };
                ws.OnError += (sender, e) =>
                {
                    Debug.Log("WebSocket Error Message: " + e.Message);
                };
                ws.OnClose += (sender, e) =>
                {
                    Debug.Log("WebSocket Close");
                    tryReconnect = true; // 再接続を試みるフラグを立てる
                    timeSinceLastConnectAttempt = 0.0f; // タイマーをリセット
                };
                ws.Connect();
            }
            else
            {

                foreach (string optiSettingHash in optiSettingHasheList)
                {
                    foreach (string envSettingHash in envSettingHasheList)
                    {
                        foreach (string dt in dtList)
                        {
                            settingHashList.Add(new SettingHash(optiSettingHash, envSettingHash, dt));
                        }
                    }
                }
            }
        }

        void Update()
        {
            if (isClient)
            {
                if (!ws.IsAlive && tryReconnect)
                {
                    timeSinceLastConnectAttempt += Time.deltaTime;
                    if (timeSinceLastConnectAttempt >= reconnectInterval)
                    {
                        ws.Connect(); // 再接続を試みる
                        timeSinceLastConnectAttempt = 0.0f; // タイマーをリセット
                        requestSent = false;
                    }
                }
                else if (!requestSent && isWaiting)
                {
                    // サーバー側に設定を要求する
                    ws.Send("request");
                    requestSent = true;
                }
            }
        }


        public SettingHash GetNextSettingHash()
        {
            if (settingHashList.Count == 0)
            {
                return null;
            }
            SettingHash settingHash = settingHashList[0];
            settingHashList.RemoveAt(0);
            return settingHash;
        }

        public int GetTotalSequenceCount()
        {
            settingCount = optiSettingHasheList.Count * envSettingHasheList.Count * dtList.Count;
            return settingCount;
        }


        private void OnDestroy()
        {
            if (ws != null)
            {
                ws.Close();
                ws = null;
            }
        }
    }
}
