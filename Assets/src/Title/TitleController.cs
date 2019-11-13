using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


using System.Net.Sockets;

public class TitleController : MonoBehaviour
{
    public InputField ServerIP;
    public InputField UserID;
    public Text message;
    System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();


    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.DeleteAll();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SceneChange()
    {
        //データ保存
        PlayerPrefs.SetString(SavedData.ServerIP, ServerIP.text);
        PlayerPrefs.SetString(SavedData.UserID, UserID.text);

        if (LoginCheck())
        {
            if(PlayerPrefs.GetString(SavedData.UserType) == "Soldier") SceneManager.LoadScene("Game");
            if(PlayerPrefs.GetString(SavedData.UserType) == "Maynard") SceneManager.LoadScene("Maynard");
            
        }

    }

    bool LoginCheck()
    { 
        TCP_Client socket = new TCP_Client();
        if (!socket.TryConnect(ServerIP.text, 12345))
        {
            message.text = "*Not Found Server*";
            return false;
        }

        //通信設定
        socket.Init(ServerIP.text, 12345);
        socket.StartRecvThread();                     //非同期通信Recv

        //初期設定用の通信
        List<byte> sendData = new List<byte>();
        GameHeader header = new GameHeader();
        header.CreateNewData(GameHeader.ID.TITLE, UserID.text, (byte)GameHeader.LoginCode.LOGINCHECK);
        sendData.AddRange(header.GetHeader());
        var task = socket.Send(sendData.ToArray(), sendData.Count);

        timer.Start();
        while (true)
        {
            socket.Update();
            while (socket.RecvDataSize() > 0)
            {
                GameHeader head = new GameHeader();
                head.SetHeader(socket.GetRecvData());
                if (head.gameCode == (byte)GameHeader.LoginCode.LOGINSUCCES) return true;
                if (head.gameCode == (byte)GameHeader.LoginCode.LOGINFAILURE)
                {
                    message.text = "*既に存在するユーザー名です*";
                    return false;
                }
            }
            if (timer.ElapsedMilliseconds > 30000)
            {
                message.text = "*タイムアウトしました*";
                return false;
            }
        }
    }
}
