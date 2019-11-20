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

    [SerializeField] int sendPort = 12344;
    [SerializeField] string serverIP = "127.0.0.1";
    [SerializeField] GameObject gameCanvas;
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
        if (PlayerPrefs.HasKey(SavedData.ServerIP)) serverIP = PlayerPrefs.GetString(SavedData.ServerIP);


        clientController = this.GetComponent<ClientController>();
        player = GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<PlayerController>();

        //ソケット初期化
        socket.Init(sendPort);


        state.AddState(GameHeader.ID.INIT, () =>
        {
            //初期化用データ送信
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
        if (!IsInvoking("Second30Invoke")) Invoke("Second30Invoke", 1f / 30);
    }

    private void Second30Invoke()
    {
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
        if (header.type==0)
        {
            var temp = header.type;
            temp = GameHeader.UserTypeCode.SOLDIER;
            var n = temp;
        }
        //データ種類ごとの処理
        state.ChangeState(header.id);
        state.Update();

    }

    private void SendRotation(byte _id)
    {
        List<byte> sendData = new List<byte>();
        GameHeader header = new GameHeader();
        header.CreateNewData((GameHeader.ID)_id, player.userType,player.name, (byte)GameHeader.GameCode.BASICDATA);
        sendData.AddRange(header.GetHeader());
        sendData.AddRange(Convert.GetByteVector2(player.transform.localEulerAngles));

        socket.Send(sendData.ToArray(), serverIP, sendPort);

    }



    void GameUpdate()
    {
        //グレネード処理
        if((GameHeader.GameCode)header.gameCode == GameHeader.GameCode.GRENEDEDATA)
        {

            if (GameObject.Find(header.userName)) return;
            var pos = Convert.GetVector3(recvData, GameHeader.HEADER_SIZE+ sizeof(uint));
            var direction = Convert.GetVector3(recvData, sizeof(uint) + sizeof(float)*3 + GameHeader.HEADER_SIZE);
            clientController.AddGrenade(header.userName,pos,direction);
            return;
        }

        //ユーザー処理
        if ((GameHeader.GameCode)header.gameCode == GameHeader.GameCode.BASICDATA || (GameHeader.GameCode)header.gameCode == GameHeader.GameCode.SCOREDATA)
        {

            foreach (var obj in clientController.clientArray)
            {
                if (obj.name.Equals(header.userName.Trim()))
                {
                    byte[] addData = new List<byte>(recvData).GetRange(sizeof(uint), recvData.Length - sizeof(uint)).ToArray();
                    obj.AddRecvData(addData);

                    break;
                }
            }
        }

        if ((GameHeader.GameCode)header.gameCode == GameHeader.GameCode.CHECKDATA)
        {
            bool addUserFlg = true;
            for (int i=0;i< clientController.clientArray.Length; i++)
            {
                BaseClient client = clientController.clientArray[i];
                if (client.name.Equals(header.userName.Trim()))
                {
                    byte[] addData = new List<byte>(recvData).GetRange(sizeof(uint), recvData.Length - sizeof(uint)).ToArray();
                    client.AddRecvData(addData);
                    addUserFlg = false;
                    break;
                }

            }

            //user追加
            if (!gameCanvas.activeSelf) return;
            if (addUserFlg)
            {
                Vector3 pos = Convert.GetVector3(recvData, GameHeader.HEADER_SIZE + sizeof(uint)+sizeof(byte));
                byte[] addData = new List<byte>(recvData).GetRange(sizeof(uint), recvData.Length - sizeof(uint)).ToArray();

                //ユーザーの追加
                if(header.type==GameHeader.UserTypeCode.SOLDIER)clientController.AddSoldierUser(header.userName.Trim(), pos).AddRecvData(addData);
                if (header.type == GameHeader.UserTypeCode.MAYNARD) clientController.AddMaynardUser(header.userName.Trim(), pos).AddRecvData(addData);

            }

        }
    }

}

