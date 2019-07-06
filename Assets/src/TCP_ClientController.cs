using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TCP_ClientController : MonoBehaviour
{
    enum Key : short
    {
        W = 0x001,
        S = 0x002,
        A = 0x004,
        D = 0x008,
        G = 0x010,
        R = 0x020,
        SHIFT = 0x040,
        LEFT_BUTTON = 0x080,
        RIGHT_BUTTON = 0x100
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
        if (Input.anyKey)
        {
            InputUpdata();
        }

        socket.Update();
        while (socket.RecvDataSize() > 0)
        {
            socket.GetRecvData();
        }

    }

    private void InputUpdata()
    {
        Key sendKey=0;
        
        sendKey|=InputTemple(KeyCode.W, Key.W);
        sendKey |= InputTemple(KeyCode.S, Key.S);
        sendKey |= InputTemple(KeyCode.A, Key.A);
        sendKey |= InputTemple(KeyCode.D, Key.D);

        if(sendKey!=0) TestInputSend(HeaderConstant.ID_GAME, HeaderConstant.CODE_GAME_BASICDATA, sendKey);
    }

    private Key InputTemple(KeyCode _key,Key _keyCode)
    {
        if (Input.GetKeyUp(_key) || Input.GetKeyDown(_key))
        {
            return _keyCode;
        }
        return 0;
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
        byte[] sendData = new byte[sizeof(byte) * 2 + userName.Length+sizeof(Key)];
        sendData[0] = _id;
        userName.CopyTo(sendData, sizeof(byte));
        sendData[sizeof(byte) + userName.Length] = _code;
        sendData[sizeof(byte) * 2 + userName.Length] = (byte)_keyCode;
        var task=socket.Send(sendData, sendData.Length);
    }

}


