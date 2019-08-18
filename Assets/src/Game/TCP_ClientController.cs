using System.Collections;
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
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();

        //通信設定
        socket.Init(ipOrHost, port);
        socket.StartRecvThread();                     //非同期通信Recv

        //初期設定用の通信
        TestSend((byte)Header.ID.INIT);



    }

    // Update is called once per frame
    void Update()
    {
        //playerのキー入力取得
        Key sendKey=playerController.InputUpdate();
        if (sendKey != 0) TestInputSend((byte)Header.ID.GAME, (byte)Header.GameCode.BASICDATA, sendKey);

        if (!timerFlg)
        {
            //通信情報の確認
            timer.Restart();
            TestSend((byte)Header.ID.DEBUG, (byte)Header.GameCode.BASICDATA);
            timerFlg = true;
        }


        socket.Update();
        while (socket.RecvDataSize() > 0)
        {
            var recvData=socket.GetRecvData();
            if ((Header.ID)recvData[0] == Header.ID.DEBUG)
            {
                int sum = BitConverter.ToInt32(recvData, sizeof(byte) * 2 + Header.USERID_LENGTH);
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
        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        byte[] userName = enc.GetBytes(System.String.Format("{0, -" + Header.USERID_LENGTH + "}", player.name));              //12byteに設定する
        byte[] sendData = new byte[sizeof(byte) * 2 + userName.Length];
        sendData[0] = _id;
        userName.CopyTo(sendData, sizeof(byte));
        sendData[sizeof(byte) + userName.Length] = _code;
        var task=socket.Send(sendData, sendData.Length);
    }


    void TestInputSend(byte _id, byte _code, Key _keyCode)
    {
        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        byte[] userName = enc.GetBytes(System.String.Format("{0, -" + Header.USERID_LENGTH + "}", player.name));              //12byteに設定する
        List<byte> sendData = new List<byte>();
        sendData.Add(_id);
        sendData.AddRange(userName);
        sendData.Add(_code);
        sendData.AddRange(BitConverter.GetBytes((short)_keyCode));
        var task =socket.Send(sendData.ToArray(), sendData.Count);
    }

}


