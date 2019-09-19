﻿using System.Collections;
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

    private StateMachine<GameHeader.ID> state = new StateMachine<GameHeader.ID>();
    private byte[] recvData;
    private GameHeader header=new GameHeader();

    // Start is called before the first frame update
    void Start()
    {
        clientController = this.GetComponent<ClientController>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        //ソケット初期化
        socket.Init(recvPort, sendPort);


        state.AddState(GameHeader.ID.INIT, () =>
        {
            //初期化用データ送信
            SendRotation((byte)GameHeader.ID.INIT);
            state.ChangeState(GameHeader.ID.GAME);
        });
        state.AddState(GameHeader.ID.GAME, _update: GameUpdate);
        state.ChangeState(GameHeader.ID.INIT);

    }

    // Update is called once per frame
    void Update()
    {
        while (socket.server.GetRecvDataSize() > 0)
        {
            RecvRoutine();
        }

        //角度の送信(30fpsで良い)
        if (player.GetRotationSendFlg()) SendRotation((byte)GameHeader.ID.GAME);
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

        //ヘッダー所得
        header.SetHeader(recvData,sizeof(uint));

        //データ種類ごとの処理
        state.ChangeState(header.id);
        state.Update();

    }

    private void SendRotation(byte _id)
    {
        List<byte> sendData = new List<byte>();
        GameHeader header = new GameHeader();
        header.CreateNewData((GameHeader.ID)_id, player.name, (byte)GameHeader.GameCode.BASICDATA);

        byte[] rotationData = new byte[sizeof(float) * 3];
        Buffer.BlockCopy(BitConverter.GetBytes(player.transform.localEulerAngles.x), 0, rotationData, 0 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(player.transform.localEulerAngles.y), 0, rotationData, 1 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(player.transform.localEulerAngles.z), 0, rotationData, 2 * sizeof(float), sizeof(float));

        sendData.AddRange(header.GetHeader());
        sendData.AddRange(rotationData);

        socket.Send(sendData.ToArray(), serverIP, sendPort);

    }



    void GameUpdate()
    {
        //グレネード処理
        if((GameHeader.GameCode)header.gameCode == GameHeader.GameCode.GRENEDEDATA)
        {

            if (GameObject.Find(header.userName)) return;
            var pos = Convert.GetVector3(recvData, header.GetHeaderLength()+ sizeof(uint));
            var direction = Convert.GetVector3(recvData, sizeof(uint) + sizeof(float)*3 + header.GetHeaderLength());
            clientController.AddGrenade(header.userName,pos,direction);
            return;
        }

        //ユーザー処理
        if ((GameHeader.GameCode)header.gameCode == GameHeader.GameCode.BASICDATA)
        {
            bool addUserFlg = true;

            foreach (var obj in clientController.clientList)
            {
                if (obj.name.Equals(header.userName.Trim()))
                {
                    var addData = new List<byte>(recvData).GetRange(sizeof(uint), recvData.Length - sizeof(uint)).ToArray();
                    obj.AddRecvData(addData);

                    addUserFlg = false;
                    break;
                }
            }


            //user追加
            if (addUserFlg)
            {
                Vector3 pos = Convert.GetVector3(recvData, GameHeader.HEADER_SIZE + sizeof(uint));

                //ユーザーの追加
                clientController.AddUser(header.userName.Trim(), pos);
            }
        }
    }

}

