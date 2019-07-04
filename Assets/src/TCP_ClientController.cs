using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TCP_ClientController : MonoBehaviour
{

    //Key
    enum Key : byte
    {
        W_UP = 0x0001,
        S_UP = 0x0002,
        A_UP = 0x0003,
        D_UP = 0x0004,
        W_DOWN = 0x0005,
        S_DOWN = 0x0006,
        A_DOWN = 0x0007,
        D_DOWN = 0x0008
    }

    //ipアドレスとポート番号設定
    public string ipOrHost = "127.0.0.1";
    public int port = 12345;
    private string userID;
    TCP_Client socket = new TCP_Client();

    // Start is called before the first frame update
    void Start()
    {
        userID = GameObject.FindGameObjectWithTag("Player").name;


        //通信設定
        socket.Init(ipOrHost, port);
        socket.StartRecvThread();                     //非同期通信Recv

        //初期設定用の通信
        TestSend(HeaderConstant.ID_INIT);

    }

    // Update is called once per frame
    void Update()
    {
        InputUpdata();

        socket.Update();
        while (socket.RecvDataSize() > 0)
        {

        }

    }

    private void InputUpdata()
    {
        if (Input.GetKeyUp(KeyCode.W))
        {
            TestInputSend(HeaderConstant.ID_GAME, HeaderConstant.CODE_GAME_BASICDATA, Key.W_UP);
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            TestInputSend(HeaderConstant.ID_GAME, HeaderConstant.CODE_GAME_BASICDATA, Key.S_UP);

        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            TestInputSend(HeaderConstant.ID_GAME, HeaderConstant.CODE_GAME_BASICDATA, Key.A_UP);
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            TestInputSend(HeaderConstant.ID_GAME, HeaderConstant.CODE_GAME_BASICDATA, Key.D_UP);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            TestInputSend(HeaderConstant.ID_GAME, HeaderConstant.CODE_GAME_BASICDATA, Key.W_DOWN);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            TestInputSend(HeaderConstant.ID_GAME, HeaderConstant.CODE_GAME_BASICDATA, Key.S_DOWN);

        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            TestInputSend(HeaderConstant.ID_GAME, HeaderConstant.CODE_GAME_BASICDATA, Key.A_DOWN);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            TestInputSend(HeaderConstant.ID_GAME, HeaderConstant.CODE_GAME_BASICDATA, Key.D_DOWN);
        }


    }


    void TestSend(byte _id, byte _code = 0x0001, byte _keyCode = 0x0001)
    {
        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        byte[] userName = enc.GetBytes(System.String.Format("{0, -" + HeaderConstant.USERID_LENGTH + "}", userID));              //12byteに設定する
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
        byte[] userName = enc.GetBytes(System.String.Format("{0, -" + HeaderConstant.USERID_LENGTH + "}", userID));              //12byteに設定する
        byte[] sendData = new byte[sizeof(byte) * 3 + userName.Length];
        sendData[0] = _id;
        userName.CopyTo(sendData, sizeof(byte));
        sendData[sizeof(byte) + userName.Length] = _code;
        sendData[sizeof(byte) * 2 + userName.Length] = (byte)_keyCode;
        var task=socket.Send(sendData, sendData.Length);
    }

}


