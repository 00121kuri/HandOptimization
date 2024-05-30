using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System;
using System.Text;
using UniRx;
using UnityEngine.UI;


namespace GraspingOptimization
{
    public class ObjectPoseRecever : MonoBehaviour
    {
        private UdpClient udpClient;
        private Subject<string> subject = new Subject<string>();
        [SerializeField] Text message;

        [SerializeField] int port;

        [SerializeField] GameObject realObject;

        [System.Serializable]
        public class ObjectRecevedData
        {
            public List<float> position;
            public List<float> rotation;
        }


        void Start()
        {
            udpClient = new UdpClient(port);
            udpClient.BeginReceive(OnReceived, udpClient);

            subject
                .ObserveOnMainThread()
                .Subscribe(json =>
                {
                    ObjectRecevedData objectRecevedData = JsonUtility.FromJson<ObjectRecevedData>(json);
                    SetObjectPose(objectRecevedData);
                }).AddTo(this);
        }

        private void OnReceived(System.IAsyncResult result)
        {
            UdpClient getUdp = (UdpClient)result.AsyncState;
            IPEndPoint ipEnd = null;

            byte[] getByte = getUdp.EndReceive(result, ref ipEnd);

            string json = Encoding.UTF8.GetString(getByte);

            Debug.Log(json);
            subject.OnNext(json);

            getUdp.BeginReceive(OnReceived, getUdp);
        }

        private void OnDestroy()
        {
            udpClient.Close();
        }

        public void SetObjectPose(ObjectRecevedData objectRecevedData)
        {
            // realObjectのlocalの位置・姿勢を変更する
            Vector3 position = new Vector3(objectRecevedData.position[0], objectRecevedData.position[1], objectRecevedData.position[2]);
            //Quaternion rotation = Quaternion.Euler(objectRecevedData.rotation[0], objectRecevedData.rotation[1], objectRecevedData.rotation[2]);
            Quaternion rotation = new Quaternion(objectRecevedData.rotation[0], objectRecevedData.rotation[1], objectRecevedData.rotation[2], objectRecevedData.rotation[3]);

            realObject.transform.localPosition = position;
            realObject.transform.localRotation = rotation;
        }
    }
}
