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
    public string serverIP = "127.0.0.1";
    public ClientController clientController;
    public GameObject player;
    ClientState sender = new ClientState();

    public uint nowSequence=0;
    // Start is called before the first frame update
    void Start()
    {
        clientController = this.GetComponent<ClientController>();
        player = GameObject.FindGameObjectWithTag("Player");

        socket.Init(recvPort, sendPort);

        //temp
        sender.socket = new UdpClient();
        List<byte> sendData=new List<byte>();
        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        byte[] userName = enc.GetBytes(System.String.Format("{0, -" + HeaderConstant.USERID_LENGTH + "}", player.name));              //12byteに設定する
        byte[] rotationData = new byte[sizeof(float) * 3];
        Buffer.BlockCopy(BitConverter.GetBytes(player.transform.localEulerAngles.x), 0, rotationData, 0 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(player.transform.localEulerAngles.y), 0, rotationData, 1 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(player.transform.localEulerAngles.z), 0, rotationData, 2 * sizeof(float), sizeof(float));
        sendData.AddRange(userName);
        sendData.Add(HeaderConstant.ID_INIT);
        sendData.AddRange(rotationData);
        sender.socket.Send(sendData.ToArray(), sendData.ToArray().Length, serverIP, sendPort);


        /*
        ClientState client = new ClientState();
        client.socket = new UdpClient();
        byte[] data = new byte[3];
        data[0] = 0x0001;
        data[1] = 0x0002;
        data[2] = 0x0003;
        int sendData = client.socket.Send(data, data.Length, serverIP, sendPort);
        Console.WriteLine("send={0}", sendData);

        client.socket.Close();
        */


    }

    // Update is called once per frame
    void Update()
    {
        while (socket.server.GetRecvDataSize() > 0)
        {
            RecvRoutine();
        }

        List<byte> sendData = new List<byte>();
        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        byte[] userName = enc.GetBytes(System.String.Format("{0, -" + HeaderConstant.USERID_LENGTH + "}", player.name));              //12byteに設定する
        byte[] rotationData = new byte[sizeof(float) * 3];
        Buffer.BlockCopy(BitConverter.GetBytes(player.transform.localEulerAngles.x), 0, rotationData, 0 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(player.transform.localEulerAngles.y), 0, rotationData, 1 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(player.transform.localEulerAngles.z), 0, rotationData, 2 * sizeof(float), sizeof(float));
        sendData.AddRange(userName);
        sendData.Add(HeaderConstant.ID_GAME);
        sendData.AddRange(rotationData);
        sender.socket.Send(sendData.ToArray(), sendData.ToArray().Length, serverIP, sendPort);

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
            //player用
            if (userId.Trim() == player.name)
            {
                UpdateUserPosition(player,data);
                addUserFlg = false;
            }


            //座標の代入
            foreach (var obj in clientController.objects)
            {
                if (obj.name.Equals(userId.Trim()))
                {
                    if (data[sizeof(uint) + sizeof(byte) + HeaderConstant.USERID_LENGTH] == HeaderConstant.CODE_GAME_BASICDATA)
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
                vect.x = BitConverter.ToSingle(data, sizeof(uint) + sizeof(byte) * 2 + b_userId.Length + 0 * sizeof(float));
                vect.y = BitConverter.ToSingle(data, sizeof(uint) + sizeof(byte) * 2 + b_userId.Length + 1 * sizeof(float));
                vect.z = BitConverter.ToSingle(data, sizeof(uint) + sizeof(byte) * 2 + b_userId.Length + 2 * sizeof(float));

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
        vect.x = BitConverter.ToSingle(_data, sizeof(uint)+sizeof(byte) * 2 + 12 + 0 * sizeof(float));
        vect.y = BitConverter.ToSingle(_data, sizeof(uint) + sizeof(byte) * 2 + 12 + 1 * sizeof(float));
        vect.z = BitConverter.ToSingle(_data, sizeof(uint) + sizeof(byte) * 2 + 12 + 2 * sizeof(float));
        _obj.transform.position = vect;

        vect = Vector3.zero;
        vect.x = BitConverter.ToSingle(_data, sizeof(uint) + sizeof(byte) * 2 + 12 + 3 * sizeof(float));
        vect.y = BitConverter.ToSingle(_data, sizeof(uint) + sizeof(byte) * 2 + 12 + 4 * sizeof(float));
        vect.z = BitConverter.ToSingle(_data, sizeof(uint) + sizeof(byte) * 2 + 12 + 5 * sizeof(float));
        //_obj.transform.rotation = Quaternion.Euler(vect);

        _obj.GetComponent<Client>().animationState=(int)_data[sizeof(uint) + sizeof(byte) * 2 + 12 + 6 * sizeof(float)];

    }
}