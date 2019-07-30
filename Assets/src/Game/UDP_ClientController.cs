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
    private GameObject player;
    private uint nowSequence = 0;
    private UDP_Client socket = new UDP_Client();
    // Start is called before the first frame update
    void Start()
    {

        clientController = this.GetComponent<ClientController>();
        player = GameObject.FindGameObjectWithTag("Player");

        //ソケット初期化
        socket.Init(recvPort, sendPort);

        //初期化用データ送信
        SendRotation(HeaderConstant.ID_INIT);
    }

    // Update is called once per frame
    void Update()
    {
        while (socket.server.GetRecvDataSize() > 0)
        {
            RecvRoutine();
        }

        //角度の送信
        SendRotation(HeaderConstant.ID_GAME);
    }

    private void RecvRoutine()
    {
        byte[] data = socket.server.GetRecvData().Value;
        byte[] b_userId = new byte[HeaderConstant.USERID_LENGTH];
        System.Array.Copy(data, sizeof(uint) + sizeof(byte), b_userId, 0, b_userId.Length);
        string userId = System.Text.Encoding.UTF8.GetString(b_userId);
        uint sequence = BitConverter.ToUInt32(data, 0);

        if (nowSequence > sequence)
        {
            if (Math.Abs(nowSequence - sequence) < 2000000000) return;
            if (nowSequence < 1000000000 && sequence > 3000000000) return;
        }

        nowSequence = sequence;

        //ゲーム処理
        if (data[sizeof(uint)] == HeaderConstant.ID_GAME)
        {
            bool addUserFlg = true;
            int headerSize = sizeof(uint) + sizeof(byte) * 2 + HeaderConstant.USERID_LENGTH;

            //player用
            if (userId.Trim() == player.name)
            {

                //座標の代入
                player.transform.position = GetVector3(data, headerSize);

                //アニメーション変更
                player.GetComponent<Client>().SetAnimationState(BitConverter.ToInt32(data, headerSize + 6 * sizeof(float)));
                addUserFlg = false;
            }

            //Player以外のユーザー用
            foreach (var obj in clientController.objects)
            {
                if (obj.name.Equals(userId.Trim()))
                {
                    if (data[sizeof(uint) + sizeof(byte) + HeaderConstant.USERID_LENGTH] == HeaderConstant.CODE_GAME_BASICDATA)
                    {
                        //座標の代入
                        obj.transform.position = GetVector3(data, headerSize);

                        //アニメーション変更
                        obj.SetAnimationState(BitConverter.ToInt32(data, headerSize + 6 * sizeof(float)));

                        //hp設定
                        obj.hp = BitConverter.ToInt32(data, headerSize + 7 * sizeof(float));


                    }
                    addUserFlg = false;
                }
            }


            //user追加
            if (addUserFlg)
            {
                Vector3 pos = GetVector3(data, headerSize);

                //ユーザーの追加
                this.GetComponent<ClientController>().AddUser(userId.Trim(), pos);
            }
        }
    }

    private Vector3 GetVector3(byte[] _data, int _beginPoint = 0, bool _x = true, bool _y = true, bool _z = true)
    {
        Vector3 vect = Vector3.zero;
        if (_data.Length < sizeof(float) * 3) return vect;
        if (_x) vect.x = BitConverter.ToSingle(_data, _beginPoint + 0 * sizeof(float));
        if (_y) vect.y = BitConverter.ToSingle(_data, _beginPoint + 1 * sizeof(float));
        if (_z) vect.z = BitConverter.ToSingle(_data, _beginPoint + 2 * sizeof(float));
        return vect;
    }

    private void SendRotation(byte _id)
    {
        List<byte> sendData = new List<byte>();
        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        byte[] userName = enc.GetBytes(System.String.Format("{0, -" + HeaderConstant.USERID_LENGTH + "}", player.name));              //12byteに設定する
        byte[] rotationData = new byte[sizeof(float) * 3];
        Buffer.BlockCopy(BitConverter.GetBytes(player.transform.localEulerAngles.x), 0, rotationData, 0 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(player.transform.localEulerAngles.y), 0, rotationData, 1 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(player.transform.localEulerAngles.z), 0, rotationData, 2 * sizeof(float), sizeof(float));
        sendData.AddRange(userName);
        sendData.Add(_id);
        sendData.AddRange(rotationData);
        socket.Send(sendData.ToArray(), serverIP, sendPort);

    }

}

/*
class UPnPController
{
    ~UPnPController()
    {
        DeletePort();
    }

    public void AddPort()
    {
        UPnPTest();
    }

    public void DeletePort()
    {
        int port = 0;
        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1500);

        EndPoint endPoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);
        string requestString = "M-SEARCH * HTTP/1.1\r\n" +
                               "HOST: 239.255.255.250:1900\r\n" +
                               "ST: upnp:rootdevice\r\n" +
                               "MAN: \"ssdp:discover\"\r\n" +
                               "MX: 3\r\n" +
                               "\r\n";
        byte[] requestByte = Encoding.ASCII.GetBytes(requestString);
        client.SendTo(requestByte, requestByte.Length, SocketFlags.None, endPoint);

        EndPoint endPoint2 = new IPEndPoint(IPAddress.Any, 0);
        byte[] responseByte = new byte[1024];
        client.ReceiveFrom(responseByte, ref endPoint2);
        var responseString = Encoding.ASCII.GetString(responseByte);
        Debug.Log(responseString);
        string location = "";
        string[] parts = responseString.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string part in parts)
        {
            if (part.ToLower().StartsWith("location"))
            {
                location = part.Substring(part.IndexOf(':') + 1);
                string port_s = location.Substring(location.LastIndexOf(':') + 1);
                port = int.Parse(port_s.Substring(0, port_s.IndexOf('/')));
                location = location.Trim();

                //IPの取得
                break;
            }
        }
        Debug.Log(location);
        string controlUrl;
        string urn = "urn:schemas-upnp-org:service:WANPPPConnection:1";
        using (WebClient webClient = new WebClient())
        {
            var st = webClient.DownloadString(location);
            Debug.Log(st);
            int serviceIndex = st.IndexOf(urn);
            if (serviceIndex == -1)
            {
                urn = "urn:schemas-upnp-org:service:WANIPConnection:1";
                serviceIndex = st.IndexOf(urn);
            }
            controlUrl = st.Substring(serviceIndex);
            string tag1 = "<controlURL>";
            string tag2 = "</controlURL>";
            controlUrl = controlUrl.Substring(controlUrl.IndexOf(tag1) + tag1.Length);
            controlUrl = controlUrl.Substring(0, controlUrl.IndexOf(tag2));
        }


        string bodyString =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
        "<s:Envelope " + "xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" " + "s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
        " <s:Body>" +
        "  <u:DeletePortMapping xmlns:u=\"" + urn + "\">" +
        "   <NewRemoteHost></NewRemoteHost>" +
        "   <NewExternalPort>" + 12343 + "</NewExternalPort>" +
        "   <NewProtocol>" + "UDP" + "</NewProtocol>" +
        "  </u:DeletePortMapping>" +
        " </s:Body>" +
        "</s:Envelope>";


        byte[] bodyByte = UTF8Encoding.ASCII.GetBytes(bodyString);
        string headString = "POST " + controlUrl + " HTTP/1.1\r\n" +
                            "HOST: " + "192.168.179.1" + ":" + port + "\r\n" +
                            "CONTENT-LENGTH: " + bodyByte.Length + "\r\n" +
                            "CONTENT-TYPE: text/xml; charset=\"utf-8\"" + "\r\n" +
                            "SOAPACTION: \"" + urn + "#" + "DeletePortMapping" + "\"\r\n" +
                            "\r\n";
        byte[] headByte = Encoding.ASCII.GetBytes(headString);
        byte[] tempByte = new byte[headByte.Length + bodyByte.Length];
        headByte.CopyTo(tempByte, 0);
        bodyByte.CopyTo(tempByte, headByte.Length);

        SOAPSend(tempByte, port);


    }



    private void UPnPTest()
    {
        int port = 0;
        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1500);

        EndPoint endPoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);
        string requestString = "M-SEARCH * HTTP/1.1\r\n" +
                               "HOST: 239.255.255.250:1900\r\n" +
                               "ST: upnp:rootdevice\r\n" +
                               "MAN: \"ssdp:discover\"\r\n" +
                               "MX: 3\r\n" +
                               "\r\n";
        byte[] requestByte = Encoding.ASCII.GetBytes(requestString);
        client.SendTo(requestByte, requestByte.Length, SocketFlags.None, endPoint);

        EndPoint endPoint2 = new IPEndPoint(IPAddress.Any, 0);
        byte[] responseByte = new byte[1024];
        client.ReceiveFrom(responseByte, ref endPoint2);
        var responseString = Encoding.ASCII.GetString(responseByte);
        Debug.Log(responseString);
        string location = "";
        string[] parts = responseString.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string part in parts)
        {
            if (part.ToLower().StartsWith("location"))
            {
                location = part.Substring(part.IndexOf(':') + 1);
                string port_s = location.Substring(location.LastIndexOf(':') + 1);
                port = int.Parse(port_s.Substring(0, port_s.IndexOf('/')));
                location = location.Trim();

                //IPの取得
                break;
            }
        }
        Debug.Log(location);
        string controlUrl;
        string urn = "urn:schemas-upnp-org:service:WANPPPConnection:1";
        using (WebClient webClient = new WebClient())
        {
            var st = webClient.DownloadString(location);
            Debug.Log(st);
            int serviceIndex = st.IndexOf(urn);
            if (serviceIndex == -1)
            {
                urn = "urn:schemas-upnp-org:service:WANIPConnection:1";
                serviceIndex = st.IndexOf(urn);
            }
            controlUrl = st.Substring(serviceIndex);
            string tag1 = "<controlURL>";
            string tag2 = "</controlURL>";
            controlUrl = controlUrl.Substring(controlUrl.IndexOf(tag1) + tag1.Length);
            controlUrl = controlUrl.Substring(0, controlUrl.IndexOf(tag2));
        }

        string bodyString =
    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
    "<s:Envelope " + "xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" " + "s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
    " <s:Body>" +
    "  <u:AddPortMapping xmlns:u=\"" + urn + "\">" +
    "   <NewRemoteHost></NewRemoteHost>" +
    "   <NewExternalPort>" + 12343 + "</NewExternalPort>" +
    "   <NewProtocol>" + "UDP" + "</NewProtocol>" +
    "   <NewInternalPort>" + 12343 + "</NewInternalPort>" +
    "   <NewInternalClient>" + "192.168.179.2" + "</NewInternalClient>" +
    "   <NewEnabled>1</NewEnabled>" +
    "   <NewPortMappingDescription>" + "GAME" + "</NewPortMappingDescription>" +
    "   <NewLeaseDuration>0</NewLeaseDuration>" +
    "  </u:AddPortMapping>" +
    " </s:Body>" +
    "</s:Envelope>";


        byte[] bodyByte = UTF8Encoding.ASCII.GetBytes(bodyString);
        string headString = "POST " + controlUrl + " HTTP/1.1\r\n" +
                            "HOST: " + "192.168.179.1" + ":" + port + "\r\n" +
                            "CONTENT-LENGTH: " + bodyByte.Length + "\r\n" +
                            "CONTENT-TYPE: text/xml; charset=\"utf-8\"" + "\r\n" +
                            "SOAPACTION: \"" + urn + "#" + "AddPortMapping" + "\"\r\n" +
                            "\r\n";
        byte[] headByte = Encoding.ASCII.GetBytes(headString);
        byte[] tempByte = new byte[headByte.Length + bodyByte.Length];
        headByte.CopyTo(tempByte, 0);
        bodyByte.CopyTo(tempByte, headByte.Length);

        SOAPSend(tempByte, port);
    }


    private void SOAPSend(byte[] _data, int _port)
    {
        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        EndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.179.1"), _port);                        //IPはルーター
        client.Connect(endPoint);
        client.Send(_data, _data.Length, SocketFlags.None);

        byte[] responseByte = new byte[1024];
        MemoryStream memoryStream = new MemoryStream();
        while (true)
        {
            int responseSize = client.Receive(responseByte, responseByte.Length, SocketFlags.None);
            if (responseSize == 0) break;
            memoryStream.Write(responseByte, 0, responseSize);
            break;
        }

        string responseString = Encoding.ASCII.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        memoryStream.Close();

        client.Shutdown(SocketShutdown.Both);
        client.Close();


    }

}
*/
