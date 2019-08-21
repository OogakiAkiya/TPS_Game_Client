using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



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
        Reloading,
        Dying
    }

    public string userID;
    public int hp = 100;
    List<byte[]> recvDataList = new List<byte[]>();
    private AnimationKey animationState = AnimationKey.Idle;
    private Animator animator;
    public StateMachine<AnimationKey> stateMachine { get; private set; } = new StateMachine<AnimationKey>();
    private GameObject damageEffectPref;

    private BaseWeapon weapon=null;
    public GameObject effect=null;
    //UI
    public Text weapon_UI;
    public Image weapon_Image;
    //当たり判定
    private Camera cam;
    private RectTransform imageRect;
    private Canvas canvas;


    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        AddStates();
        stateMachine.ChangeState(AnimationKey.Idle);
        damageEffectPref = (GameObject)Resources.Load("blood");


        //攻撃関係
        if (effect!=null)effect.SetActive(false);
        
        weapon = new MachineGun(Shoot);
        if(weapon_Image)weapon.SetTexture(weapon_Image);

        if (this.tag != "Player") return;
        cam = transform.FindChild("Camera").gameObject.GetComponent<Camera>();
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        imageRect = GameObject.Find("Canvas").transform.FindChild("Pointer").GetComponent<RectTransform>();

    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (weapon.state.currentKey == WEAPONSTATE.WAIT)
        {
            if (Input.GetMouseButton(0))
            {
                weapon.state.ChangeState(WEAPONSTATE.ATACK);
            }
        }
        */
        weapon.state.Update();
        if (weapon_UI) weapon_UI.text = weapon.remainingBullet + "/" + weapon.magazine;



        //受信データの代入
        while (recvDataList.Count > 0)
        {
            var recvData=GetRecvData();
            SetStatus(recvData);
        }

        //アニメーション変更
        if (stateMachine.currentKey != animationState)stateMachine.ChangeState(animationState);
    }

    public void SetStatus(byte[] _data)
    {
        if (this.tag == "Player")
        {
            //座標の代入
            this.transform.position = Convert.GetVector3(_data, Header.HEADER_SIZE);

            //アニメーション変更
            animationState = (AnimationKey)System.BitConverter.ToInt32(_data, Header.HEADER_SIZE + 6 * sizeof(float));

            hp = System.BitConverter.ToInt32(_data, Header.HEADER_SIZE + 7 * sizeof(float));

            if (weapon!=null)
            {
                weapon.SetStatus(_data, Header.HEADER_SIZE + 7 * sizeof(float) + sizeof(int));
            }
        }

        //座標の代入
        this.transform.position = Convert.GetVector3(_data, Header.HEADER_SIZE);

        //回転座標の代入
        this.transform.rotation = Quaternion.Euler(Convert.GetVector3(_data, Header.HEADER_SIZE + 3 * sizeof(float)));

        //アニメーション変更
        animationState=(AnimationKey)System.BitConverter.ToInt32(_data, Header.HEADER_SIZE + 6 * sizeof(float));

        //hp設定
        hp = System.BitConverter.ToInt32(_data, Header.HEADER_SIZE + 7 * sizeof(float));


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

    private void Shoot()
    {
        //発砲エフェクト
        if (effect!=null) effect.SetActive(true);

        if (this.tag != "Player") return;
        //レイの作成
        Ray ray = cam.ScreenPointToRay(GetUIScreenPos(imageRect));
        //レイの可視化
        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow,10f);

        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.tag == "users")
            {
                hit.collider.GetComponent<Client>().CreateDamageEffect();
            }
        }


    }
    Vector2 GetUIScreenPos(RectTransform rt)
    {

        //UIのCanvasに使用されるカメラは Hierarchy 上には表示されないので、
        //変換したいUIが所属しているcanvasを映しているカメラを取得し、 WorldToScreenPoint() で座標変換する
        return RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rt.position);

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
                //animator.CrossFadeInFixedTime("Reloading", 0.1f);
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
        stateMachine.AddState(AnimationKey.Reloading,
            () =>
            {
                animator.CrossFadeInFixedTime("Reloading", 0.1f);
            },
            () =>
            {
            }
            );
    }

}
