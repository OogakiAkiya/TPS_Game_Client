using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Client : MonoBehaviour
{
    public enum AnimationKey : int
    {
        Idle,
        Walk,
        WalkForward,
        WalkBack,
        WalkLeft,
        WalkRight,
        Run,
        RunForward,
        RunBack,
        RunLeft,
        RunRight,
        JumpUP,
        JumpStay,
        JumpDown,
        Dying
    }

    private string userID;
    public int hp = 100;
    List<byte[]> recvDataList = new List<byte[]>();
    private AnimationKey animationState = AnimationKey.Idle;
    private Animator animator;
    public StateMachine<AnimationKey> stateMachine { get; private set; } = new StateMachine<AnimationKey>();
    private GameObject damageEffectPref;

    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        AddStates();
        stateMachine.ChangeState(AnimationKey.Idle);
        damageEffectPref = (GameObject)Resources.Load("blood");

    }

    // Update is called once per frame
    void Update()
    {
        //if (hp <= 0) this.gameObject.SetActive(false);
        if (stateMachine.currentKey != animationState)
        {
            stateMachine.ChangeState(animationState);
        }
    }

    public void SetAnimationState(int _state)
    {
        animationState = (AnimationKey)_state;
    }

    public void SetUserID(string _id)
    {
        userID = _id;
    }

    public void AddRecvData(byte[] _addData)
    {
        recvDataList.Add(_addData);
    }

    public void CreateDamageEffect()
    {
        if (damageEffectPref)
        {
            GameObject add=Instantiate(damageEffectPref) as GameObject;
            add.transform.parent = this.transform;
            Vector3 pos = this.transform.position;
            pos += new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.8f,1.5f),0.0f);
            add.transform.position=pos;
        }
    }

    private byte[] GetRecvData()
    {
        byte[] returnData;
        returnData = recvDataList[0];
        recvDataList.RemoveAt(0);
        return returnData;
    }


    //=================================================================
    //statesの情報設定
    //=================================================================
    private void AddStates()
    {
        //Idle
        stateMachine.AddState(AnimationKey.Idle,
            () =>
            {
                animator.CrossFadeInFixedTime("Idle", 0.1f);
            });

        //Walk
        stateMachine.AddState(AnimationKey.Walk,
            ()=>
            {
                animator.CrossFadeInFixedTime("Walk", 0.1f);

            });
        //WalkForward
        stateMachine.AddState(AnimationKey.WalkForward,
            () =>
            {
                animator.CrossFadeInFixedTime("WalkForward", 0.1f);

            });
        //WalkBack
        stateMachine.AddState(AnimationKey.WalkBack,
            () =>
            {
                animator.CrossFadeInFixedTime("WalkBack", 0.1f);

            });
        //WalkLeft
        stateMachine.AddState(AnimationKey.WalkLeft,
            () =>
            {
                animator.CrossFadeInFixedTime("WalkLeft", 0.1f);

            });
        //WalkRight
        stateMachine.AddState(AnimationKey.WalkRight,
            () =>
            {
                animator.CrossFadeInFixedTime("WalkRight", 0.1f);

            });

        //Run
        stateMachine.AddState(AnimationKey.Run,
            () =>
            {
                animator.CrossFadeInFixedTime("Run", 0.1f);
            });
        //RunForward
        stateMachine.AddState(AnimationKey.RunForward,
            () =>
            {
                animator.CrossFadeInFixedTime("RunForward", 0.1f);
            });
        //RunBack
        stateMachine.AddState(AnimationKey.RunBack,
            () =>
            {
                animator.CrossFadeInFixedTime("RunBack", 0.1f);
            });
        //RunLeft
        stateMachine.AddState(AnimationKey.RunLeft,
            () =>
            {
                animator.CrossFadeInFixedTime("RunLeft", 0.1f);
            });
        //RunRight
        stateMachine.AddState(AnimationKey.RunRight,
            () =>
            {
                animator.CrossFadeInFixedTime("RunRight", 0.1f);
            });

        //JumpUP
        stateMachine.AddState(AnimationKey.JumpUP,
            () =>
            {
                animator.CrossFadeInFixedTime("JumpUP", 0.1f);
            }
            );

        //JumpStay
        stateMachine.AddState(AnimationKey.JumpStay,
            () =>
            {
                animator.CrossFadeInFixedTime("JumpStay", 0.1f);
            }
            );


        //JumpDown
        stateMachine.AddState(AnimationKey.JumpDown,
            () =>
            {
                animator.CrossFadeInFixedTime("JumpDown", 0.1f);
            }
            );
        //JumpDown
        stateMachine.AddState(AnimationKey.Dying,
            () =>
            {
                animator.CrossFadeInFixedTime("Dying", 0.1f);
                this.GetComponent<CapsuleCollider>().enabled = false;
            },
            _end: () =>
            {
                this.GetComponent<CapsuleCollider>().enabled = true;
            }
            );

    }

}
