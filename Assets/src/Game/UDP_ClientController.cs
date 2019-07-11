using System.Collections;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

using UnityEngine;

public class UDP_ClientController : MonoBehaviour
{

    public int recvPort = 12343;
    public int sendPort = 12344;
    public string serverIP = "127.0.0.1";
    private ClientController clientController;
    private GameObject player;
    private uint nowSequence=0;
    private UDP_Client socket = new UDP_Client();

    // Start is called before the first frame update
    void Start()
    {
        clientController = this.GetComponent<ClientController>();
        player = GameObject.FindGameObjectWithTag("Player");

        //ソケット初期化
        socket.Init(recvPort, sendPort);

        //初期化用データ送信
        SendRotation(HeaderConstant.ID_INIT);
    }

    // Update is called once per frame
    void Update()
    {
        while (socket.server.GetRecvDataSize() > 0)
        {
            RecvRoutine();
        }

        //角度の送信
        SendRotation(HeaderConstant.ID_GAME);
    }

    private void RecvRoutine()
    {
        byte[] data = socket.server.GetRecvData().Value;
        byte[] b_userId = new byte[HeaderConstant.USERID_LENGTH];
        System.Array.Copy(data, sizeof(uint)+sizeof(byte), b_userId, 0, b_userId.Length);
        string userId = System.Text.Encoding.UTF8.GetString(b_userId);
        uint sequence = BitConverter.ToUInt32(data,0);

        if (nowSequence > sequence)
        {
            if(Math.Abs(nowSequence-sequence)< 2000000000)return;
            if (nowSequence < 1000000000 && sequence > 3000000000) return;
        }

        nowSequence = sequence;

        //ゲーム処理
        if (data[sizeof(uint)] == HeaderConstant.ID_GAME)
        {
            bool addUserFlg = true;
            int headerSize = sizeof(uint) + sizeof(byte) * 2 + HeaderConstant.USERID_LENGTH;

            //player用
            if (userId.Trim() == player.name)
            {

                //座標の代入
                player.transform.position = GetVector3(data, headerSize);

                //アニメーション変更
                player.GetComponent<Client>().SetAnimationState(BitConverter.ToInt32(data, headerSize + 6 * sizeof(float)));
                addUserFlg = false;
            }

            //Player以外のユーザー用
            foreach (var obj in clientController.objects)
            {
                if (obj.name.Equals(userId.Trim()))
                {
                    if (data[sizeof(uint) + sizeof(byte) + HeaderConstant.USERID_LENGTH] == HeaderConstant.CODE_GAME_BASICDATA)
                    {
                        //座標の代入
                        obj.transform.position = GetVector3(data, headerSize);

                        //アニメーション変更
                        obj.SetAnimationState(BitConverter.ToInt32(data, headerSize + 6 * sizeof(float)));

                        //hp設定
                        obj.hp = BitConverter.ToInt32(data, headerSize + 7 * sizeof(float));


                    }
                    addUserFlg = false;
                }
            }


            //user追加
            if (addUserFlg)
            {
                Vector3 pos =GetVector3(data, headerSize);

                //ユーザーの追加
                this.GetComponent<ClientController>().AddUser(userId.Trim(),pos);
            }
        }
    }

    private Vector3 GetVector3(byte[] _data, int _beginPoint = 0, bool _x = true, bool _y = true, bool _z = true)
    {
        Vector3 vect = Vector3.zero;
        if (_data.Length < sizeof(float) * 3) return vect;
        if (_x) vect.x = BitConverter.ToSingle(_data, _beginPoint + 0 * sizeof(float));
        if (_y) vect.y = BitConverter.ToSingle(_data, _beginPoint + 1 * sizeof(float));
        if (_z) vect.z = BitConverter.ToSingle(_data, _beginPoint + 2 * sizeof(float));
        return vect;
    }

    private void SendRotation(byte _id)
    {
        List<byte> sendData = new List<byte>();
        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        byte[] userName = enc.GetBytes(System.String.Format("{0, -" + HeaderConstant.USERID_LENGTH + "}", player.name));              //12byteに設定する
        byte[] rotationData = new byte[sizeof(float) * 3];
        Buffer.BlockCopy(BitConverter.GetBytes(player.transform.localEulerAngles.x), 0, rotationData, 0 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(player.transform.localEulerAngles.y), 0, rotationData, 1 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(player.transform.localEulerAngles.z), 0, rotationData, 2 * sizeof(float), sizeof(float));
        sendData.AddRange(userName);
        sendData.Add(_id);
        sendData.AddRange(rotationData);
        socket.Send(sendData.ToArray(), serverIP, sendPort);

    }
}