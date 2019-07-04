using System.Collections;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

using UnityEngine;

public class UDP_ClientController : MonoBehaviour
{
    //udp
    UDP_Server socket = new UDP_Server();
    public int recvPort = 12343;
    public int sendPort = 12344;

    // Start is called before the first frame update
    void Start()
    {
        //udp
        socket.Init(recvPort, sendPort);

        //temp
        ClientState client = new ClientState();
        client.socket = new UdpClient();
        byte[] data = new byte[3];
        data[0] = 0x0001;
        data[1] = 0x0002;
        data[2] = 0x0003;
        int sendData = client.socket.Send(data, data.Length, "127.0.0.1", sendPort);
        Console.WriteLine("send={0}", sendData);

        client.socket.Close();

    }

    // Update is called once per frame
    void Update()
    {
        while (socket.server.GetRecvDataSize() > 0)
        {
            RecvRoutine();
        }

    }

    private void RecvRoutine()
    {
        byte[] data = socket.server.GetRecvData().Value;
        byte[] b_userId = new byte[HeaderConstant.USERID_LENGTH];
        System.Array.Copy(data, sizeof(byte), b_userId, 0, b_userId.Length);
        string userId = System.Text.Encoding.UTF8.GetString(b_userId);
        var objects = GameObject.FindGameObjectsWithTag("users");




        //ゲーム処理
        if (data[0] == HeaderConstant.ID_GAME)
        {
            bool addUserFlg = true;
            //player用
            if (userId.Trim() == GameObject.FindGameObjectWithTag("Player").name)
            {
                UpdateUserPosition(GameObject.FindGameObjectWithTag("Player"),data);
                addUserFlg = false;
            }


            //座標の代入
            foreach (var obj in objects)
            {
                if (obj.name.Equals(userId.Trim()))
                {
                    if (data[sizeof(byte) + HeaderConstant.USERID_LENGTH] == HeaderConstant.CODE_GAME_BASICDATA)
                    {
                        UpdateUserPosition(obj,data);
                    }
                    addUserFlg = false;
                }
            }


            //user追加
            if (addUserFlg)
            {
                Vector3 vect = Vector3.zero;
                vect.x = BitConverter.ToSingle(data, sizeof(byte) * 2 + b_userId.Length + 0 * sizeof(float));
                vect.y = BitConverter.ToSingle(data, sizeof(byte) * 2 + b_userId.Length + 1 * sizeof(float));
                vect.z = BitConverter.ToSingle(data, sizeof(byte) * 2 + b_userId.Length + 2 * sizeof(float));

                //クライアント追加処理
                Array.Copy(data, sizeof(byte), b_userId, 0, b_userId.Length);
                string userName = System.Text.Encoding.UTF8.GetString(b_userId);
                //ユーザーの追加
                this.GetComponent<ClientController>().AddUser(userName.Trim());
            }
        }
    }


    private void UpdateUserPosition(GameObject _obj,byte[] _data)
    {
        Vector3 vect = Vector3.zero;
        vect.x = BitConverter.ToSingle(_data, sizeof(byte) * 2 + 12 + 0 * sizeof(float));
        vect.y = BitConverter.ToSingle(_data, sizeof(byte) * 2 + 12 + 1 * sizeof(float));
        vect.z = BitConverter.ToSingle(_data, sizeof(byte) * 2 + 12 + 2 * sizeof(float));
        _obj.transform.position = vect;

    }
}