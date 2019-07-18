using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class TCP_ClientController : MonoBehaviour
{ 
    //ipアドレスとポート番号設定
    public string ipOrHost = "127.0.0.1";
    public int port = 12345;
    private TCP_Client socket = new TCP_Client();
    private GameObject player;
    private PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();

        //通信設定
        socket.Init(ipOrHost, port);
        socket.StartRecvThread();                     //非同期通信Recv

        //初期設定用の通信
        TestSend(HeaderConstant.ID_INIT);

    }

    // Update is called once per frame
    void Update()
    {
        //playerのキー入力取得
        Key sendKey=playerController.InputUpdate();
        if (sendKey != 0) TestInputSend(HeaderConstant.ID_GAME, HeaderConstant.CODE_GAME_BASICDATA, sendKey);


        socket.Update();
        while (socket.RecvDataSize() > 0)
        {
            var da=socket.GetRecvData();
        }

    }


    void TestSend(byte _id, byte _code = 0x0001, byte _keyCode = 0x0001)
    {
        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        byte[] userName = enc.GetBytes(System.String.Format("{0, -" + HeaderConstant.USERID_LENGTH + "}", player.name));              //12byteに設定する
        byte[] sendData = new byte[sizeof(byte) * 3 + userName.Length];
        sendData[0] = _id;
        userName.CopyTo(sendData, sizeof(byte));
        sendData[sizeof(byte) + userName.Length] = _code;
        sendData[sizeof(byte) * 2 + userName.Length] = (byte)_keyCode;
        var task=socket.Send(sendData, sendData.Length);
    }


    void TestInputSend(byte _id, byte _code, Key _keyCode)
    {
        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        byte[] userName = enc.GetBytes(System.String.Format("{0, -" + HeaderConstant.USERID_LENGTH + "}", player.name));              //12byteに設定する
        List<byte> sendData = new List<byte>();
        sendData.Add(_id);
        sendData.AddRange(userName);
        sendData.Add(_code);
        sendData.AddRange(BitConverter.GetBytes((short)_keyCode));
        var task =socket.Send(sendData.ToArray(), sendData.Count);
    }

}


