using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour
{
    string userID;

    List<byte[]> recvDataList = new List<byte[]>();
    public void SetUserID(string _id)
    {
        userID = _id;
    }

    public void AddRecvData(byte[] _addData)
    {
        recvDataList.Add(_addData);
    }

    private byte[] GetRecvData()
    {
        byte[] returnData;
        returnData = recvDataList[0];
        recvDataList.RemoveAt(0);
        return returnData;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
