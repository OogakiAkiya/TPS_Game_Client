using System.Collections;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;


using UnityEngine;

public class UDP_ClientController : MonoBehaviour
{

    public int recvPort = 12343;
    public int sendPort = 12344;
    public string serverIP = "127.0.0.1";
    private ClientController clientController;
    private PlayerController player;
    private uint nowSequence = 0;
    private UDP_Client socket = new UDP_Client();

    private StateMachine<Header.ID> state = new StateMachine<Header.ID>();
    private byte[] recvData;

    // Start is called before the first frame update
    void Start()
    {
        clientController = this.GetComponent<ClientController>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        //ソケット初期化
        socket.Init(recvPort, sendPort);


        state.AddState(Header.ID.INIT, () =>
        {
            //初期化用データ送信
            SendRotation((byte)Header.ID.INIT);
            state.ChangeState(Header.ID.GAME);
        });
        state.AddState(Header.ID.GAME, _update: GameUpdate);
        state.ChangeState(Header.ID.INIT);

    }

    // Update is called once per frame
    void Update()
    {
        while (socket.server.GetRecvDataSize() > 0)
        {
            RecvRoutine();
        }

        //角度の送信(30fpsで良い)
        if (player.GetRotationSendFlg()) SendRotation((byte)Header.ID.GAME);
    }

    private void RecvRoutine()
    {
        recvData = socket.server.GetRecvData().Value;
        uint sequence = BitConverter.ToUInt32(recvData, 0);

        //シーケンス処理
        if (nowSequence > sequence)
        {
            if (Math.Abs(nowSequence - sequence) < 2000000000) return;
            if (nowSequence < 1000000000 && sequence > 3000000000) return;
        }
        nowSequence = sequence;

        //データ種類ごとの処理
        //state.ChangeState((Header.ID)recvData[sizeof(uint) + Header.USERID_LENGTH]);
        state.Update();

    }

    private void SendRotation(byte _id)
    {
        List<byte> sendData = new List<byte>();
        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        byte[] userName = enc.GetBytes(System.String.Format("{0, -" + Header.USERID_LENGTH + "}", player.name));              //12byteに設定する
        byte[] rotationData = new byte[sizeof(float) * 3];
        Buffer.BlockCopy(BitConverter.GetBytes(player.transform.localEulerAngles.x), 0, rotationData, 0 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(player.transform.localEulerAngles.y), 0, rotationData, 1 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(player.transform.localEulerAngles.z), 0, rotationData, 2 * sizeof(float), sizeof(float));
        sendData.AddRange(userName);
        sendData.Add(_id);
        sendData.AddRange(rotationData);
        socket.Send(sendData.ToArray(), serverIP, sendPort);

    }



    void GameUpdate()
    {
        
        //ユーザーID取得
        byte[] b_userId = new byte[Header.USERID_LENGTH];
        System.Array.Copy(recvData, sizeof(uint) + sizeof(byte), b_userId, 0, b_userId.Length);
        string userId = System.Text.Encoding.UTF8.GetString(b_userId);


        bool addUserFlg = true;

        foreach (var obj in clientController.clientList)
        {
            if (obj.name.Equals(userId.Trim()))
            {
                obj.AddRecvData(recvData);

                addUserFlg = false;
                break;
            }
        }


        //user追加
        if (addUserFlg)
        {
            Vector3 pos = Convert.GetVector3(recvData, Header.HEADER_SIZE);

            //ユーザーの追加
            this.GetComponent<ClientController>().AddUser(userId.Trim(), pos);
        }
    }


}

