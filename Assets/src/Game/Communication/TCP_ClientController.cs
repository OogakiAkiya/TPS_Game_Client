using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


public class TCP_ClientController : MonoBehaviour
{
    [SerializeField] GameController gameController;
    //ipアドレスとポート番号設定
    [SerializeField] string ipOrHost;
    [SerializeField] int port = 12345;
    private TCP_Client socket = new TCP_Client();
    private GameObject player;
    private PlayerController playerController;

    //Debug用
    System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
    bool timerFlg=false;
    [SerializeField] Text debugText=null;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<PlayerController>();


        if (PlayerPrefs.HasKey(SavedData.ServerIP))ipOrHost= PlayerPrefs.GetString(SavedData.ServerIP);
        
        //通信設定
        socket.Init(ipOrHost, port);
        socket.StartRecvThread();                     //非同期通信Recv

        //初期設定用の通信
        if(PlayerPrefs.GetString(SavedData.UserType)== "Soldier")TestSend((byte)GameHeader.ID.INIT,(byte)GameHeader.UserTypeCode.SOLDIER);
        if (PlayerPrefs.GetString(SavedData.UserType) == "Maynard") TestSend((byte)GameHeader.ID.INIT, (byte)GameHeader.UserTypeCode.MONSTER);



    }

    // Update is called once per frame
    void Update()
    {
        if (gameController && !gameController.stopFlg)
        {
            //playerのキー入力取得
            Key sendKey = playerController.InputUpdate();
            if (sendKey != 0) TestInputSend((byte)GameHeader.ID.GAME, (byte)GameHeader.GameCode.BASICDATA, sendKey);
        }

        if (!timerFlg)
        {
            //通信情報の確認
            timer.Restart();
            TestSend((byte)GameHeader.ID.DEBUG, (byte)GameHeader.GameCode.BASICDATA);
            timerFlg = true;
        }
        if (!IsInvoking("InputCheck")) Invoke("InputCheck", 2f);


        socket.Update();
        while (socket.RecvDataSize() > 0)
        {
            var recvData=socket.GetRecvData();
            GameHeader header = new GameHeader();
            header.SetHeader(recvData);

            if (header.id == GameHeader.ID.DEBUG)
            {
                int sum = BitConverter.ToInt32(recvData, GameHeader.HEADER_SIZE);
                timer.Stop();
                timerFlg = false;
                if (debugText)
                {
                    debugText.text = $"人数:"+sum+"\nTCP応答時間:"+timer.ElapsedMilliseconds+"ミリ秒";
                }
            }

            if (header.id == GameHeader.ID.ALERT)
            {
                byte[] data = new byte[100];
                FinishData finish = new FinishData();
                int index=GameHeader.HEADER_SIZE;
                while (true)
                {
                    if (index >= recvData.Length) break;
                    if (recvData.Length < GameHeader.HEADER_SIZE + FinishData.FINISHUDATALENGHT) break;
                    header.SetHeader(recvData, index);
                    finish.SetData(recvData, index + GameHeader.HEADER_SIZE);
                    FileController.GetInstance().Write("finish", header.userName +","+ finish.killAmount + "," + finish.deathAmount + "," + finish.timeMinute + "," + finish.timeSecond);
                    index += GameHeader.HEADER_SIZE + FinishData.FINISHUDATALENGHT;

                }

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;                        //エディタ(デバッグ)の時のみ動作を止める
                #else
                    Application.Quit();                                                     //コンパイル後に動作する
                #endif
                
            }

        }

    }

    public void InputCheck()
    {
        Key sendKey = playerController.CheckNowInput();
        TestInputSend((byte)GameHeader.ID.GAME, (byte)GameHeader.GameCode.CHECKDATA, sendKey);
    }


    void TestSend(byte _id, byte _code = 0x0001)
    {
        List<byte> sendData = new List<byte>();
        GameHeader header = new GameHeader();
        header.CreateNewData((GameHeader.ID)_id,playerController.userType,playerController.name,_code);
        sendData.AddRange(header.GetHeader());
        var task=socket.Send(sendData.ToArray(), sendData.Count);
    }


    void TestInputSend(byte _id, byte _code, Key _keyCode)
    {
        List<byte> sendData = new List<byte>();
        GameHeader header = new GameHeader();
        header.CreateNewData((GameHeader.ID)_id, playerController.userType, playerController.name, _code);
        sendData.AddRange(header.GetHeader());
        sendData.AddRange(BitConverter.GetBytes((short)_keyCode));
        var task =socket.Send(sendData.ToArray(), sendData.Count);
    }

}

class FinishData
{
    public static int FINISHUDATALENGHT = sizeof(int) * 4 + sizeof(bool);
    public int killAmount = 0;
    public int deathAmount = 0;
    public int timeMinute = 0;
    public int timeSecond = 0;
    public bool survivalFlg = true;

    public void SetData(int _kill, int _death, int _timeMinute, int _timeSecond, bool _survivalFlg)
    {
        killAmount = _kill;
        deathAmount = _death;
        timeMinute = _timeMinute;
        timeSecond = _timeSecond;
        survivalFlg = _survivalFlg;
    }

    public byte[] GetData()
    {
        byte[] data = new byte[FINISHUDATALENGHT];
        int index = 0;

        Buffer.BlockCopy(Convert.Conversion(killAmount), 0, data, index, sizeof(int));
        index += sizeof(int);
        Buffer.BlockCopy(Convert.Conversion(deathAmount), 0, data, index, sizeof(int));
        index += sizeof(int);
        Buffer.BlockCopy(Convert.Conversion(timeMinute), 0, data, index, sizeof(int));
        index += sizeof(int);
        Buffer.BlockCopy(Convert.Conversion(timeSecond), 0, data, index, sizeof(int));
        index += sizeof(int);
        Buffer.BlockCopy(Convert.Conversion(survivalFlg), 0, data, index, sizeof(bool));
        index += sizeof(bool);

        return data;
    }

    public void SetData(byte[] _data, int _index = 0)
    {
        int index = _index;

        killAmount=Convert.IntConversion(_data, index);
        index += sizeof(int);
        deathAmount = Convert.IntConversion(_data, index);
        index += sizeof(int);
        timeMinute = Convert.IntConversion(_data, index);
        index += sizeof(int);
        timeSecond = Convert.IntConversion(_data, index);
        index += sizeof(int);
        survivalFlg = Convert.BoolConversion(_data, index);
        index += sizeof(int);

    }


}
