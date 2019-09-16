﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


public class TCP_ClientController : MonoBehaviour
{ 
    //ipアドレスとポート番号設定
    public string ipOrHost = "127.0.0.1";
    public int port = 12345;
    private TCP_Client socket = new TCP_Client();
    private GameObject player;
    private PlayerController playerController;

    //Debug用
    System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
    bool timerFlg=false;
    public Text debugText=null;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();


        if (PlayerPrefs.HasKey(SavedData.ServerIP))ipOrHost= PlayerPrefs.GetString(SavedData.ServerIP);
        
        //通信設定
        socket.Init(ipOrHost, port);
        socket.StartRecvThread();                     //非同期通信Recv

        //初期設定用の通信
        TestSend((byte)GameHeader.ID.INIT);



    }

    // Update is called once per frame
    void Update()
    {
        //playerのキー入力取得
        Key sendKey=playerController.InputUpdate();
        if (sendKey != 0) TestInputSend((byte)GameHeader.ID.GAME, (byte)GameHeader.GameCode.BASICDATA, sendKey);

        if (!timerFlg)
        {
            //通信情報の確認
            timer.Restart();
            TestSend((byte)GameHeader.ID.DEBUG, (byte)GameHeader.GameCode.BASICDATA);
            timerFlg = true;
        }


        socket.Update();
        while (socket.RecvDataSize() > 0)
        {
            var recvData=socket.GetRecvData();
            if ((GameHeader.ID)recvData[0] == GameHeader.ID.DEBUG)
            {
                int sum = BitConverter.ToInt32(recvData, sizeof(byte) * 2 + GameHeader.USERID_LENGTH);
                timer.Stop();
                timerFlg = false;
                if (debugText)
                {
                    debugText.text = $"人数:"+sum+"\nTCP応答時間:"+timer.ElapsedMilliseconds+"ミリ秒";
                }
            }
        }

    }


    void TestSend(byte _id, byte _code = 0x0001)
    {
        List<byte> sendData = new List<byte>();
        GameHeader header = new GameHeader();
        header.CreateNewData((GameHeader.ID)_id,playerController.name,_code);
        sendData.AddRange(header.GetHeader());
        var task=socket.Send(sendData.ToArray(), sendData.Count);
    }


    void TestInputSend(byte _id, byte _code, Key _keyCode)
    {
        List<byte> sendData = new List<byte>();
        GameHeader header = new GameHeader();
        header.CreateNewData((GameHeader.ID)_id, playerController.name, _code);
        sendData.AddRange(header.GetHeader());
        sendData.AddRange(BitConverter.GetBytes((short)_keyCode));
        var task =socket.Send(sendData.ToArray(), sendData.Count);
    }

}


