using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Client : MonoBehaviour
{
    public enum AnimationKey : int
    {
        Idle,
        Walk,
        Run,
        Jump
    }

    string userID;
    List<byte[]> recvDataList = new List<byte[]>();
    public int animationState = 0;
    private Animator animator;
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
        animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        //アニメーション遷移
        if (stateInfo.nameHash != Animator.StringToHash("Base Layer.Idle")){
            if (animationState == (int)AnimationKey.Idle) animator.CrossFadeInFixedTime("Idle", 0.0f);
        }
        if (stateInfo.nameHash != Animator.StringToHash("Base Layer.Walk")){
            if (animationState == (int)AnimationKey.Walk) animator.CrossFadeInFixedTime("Walk", 0.0f);
        }
        if (stateInfo.nameHash != Animator.StringToHash("Base Layer.Run")){
            if (animationState == (int)AnimationKey.Run) animator.CrossFadeInFixedTime("Run", 0.0f);
        }


    }
}
