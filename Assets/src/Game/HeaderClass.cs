using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Header
{
    //====================================================
    //定数
    //====================================================
    public static readonly int USERID_LENGTH = 12;
    public static readonly int HEADER_SIZE = sizeof(ID) + sizeof(GameCode) + USERID_LENGTH;

    public enum ID : byte
    {
        DEBUG = 0x001,
        INIT = 0x002,
        GAME = 0x003
    }
    public enum GameCode : byte
    {
        BASICDATA = 0x0001,
        SCOREDATA = 0x0002

    }

    public string userName { get; private set; }
    public ID id { get; private set; } = ID.INIT;
    public GameCode gameCode { get; private set; } = GameCode.BASICDATA;

    public void CreateNewData(ID _id = ID.INIT, string _name = "", GameCode _gameCode = GameCode.BASICDATA)
    {
        if (_name != "") userName = _name;
        if (id != _id) id = _id;
        if (gameCode != _gameCode) gameCode = _gameCode;
    }

    public void SetHeader(byte[] _data, int _index = 0)
    {
        id = (ID)_data[_index];
        byte[] b_userId = new byte[USERID_LENGTH];
        System.Array.Copy(_data, _index + sizeof(byte), b_userId, 0, b_userId.Length);
        userName = System.Text.Encoding.UTF8.GetString(b_userId);
        gameCode = (GameCode)_data[_index + sizeof(byte) + USERID_LENGTH];
    }

    public byte[] GetHeader()
    {
        List<byte> returnData = new List<byte>();

        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        byte[] b_userName = enc.GetBytes(System.String.Format("{0, -" + USERID_LENGTH + "}", userName));              //12byteに設定する
        byte[] sendData = new byte[sizeof(byte) * 2 + userName.Length];
        returnData.Add((byte)id);
        returnData.AddRange(b_userName);
        returnData.Add((byte)gameCode);

        return returnData.ToArray();
    }
}
