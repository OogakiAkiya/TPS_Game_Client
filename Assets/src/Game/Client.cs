﻿using System.Collections;
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
    private UserBodyData bodyData = new UserBodyData();

    public string userID;
    [SerializeField] int hp = 100;
    List<byte[]> recvDataList = new List<byte[]>();
    private AnimationKey animationState = AnimationKey.Idle;
    private Animator animator;
    public StateMachine<AnimationKey> stateMachine { get; private set; } = new StateMachine<AnimationKey>();
    private GameObject damageEffectPref;

    private BaseWeapon weapon = null;
    [SerializeField] GameObject effect = null;
    [SerializeField] GameObject weaponAddPosition = null;
    private GameObject weaponModel;

    //UI
    [SerializeField] Text weapon_UI;
    [SerializeField] Image weapon_Image;
    //当たり判定
    private Camera cam;
    private RectTransform imageRect;
    private Canvas canvas;

    //Score
    public int deathAmount { get; private set; } = 0;          //死んだ回数
    public int killAmount { get; private set; } = 0;           //殺した回数

    //ボーン要素
    private Transform hip;
    public Vector3 hipRotation=Vector3.zero;
    private GameObject modelVisual;

    private void Awake()
    {
        hip= this.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips);
        if (this.tag != "Player") return;
        cam = transform.Find("Camera").gameObject.GetComponent<Camera>();
        canvas = GameObject.Find("GameCanvas").GetComponent<Canvas>();
        imageRect = GameObject.Find("GameCanvas").transform.Find("Pointer").GetComponent<RectTransform>();
        modelVisual = transform.Find("Geo").gameObject;

    }

    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        AddStates();
        stateMachine.ChangeState(AnimationKey.Idle);
        damageEffectPref = (GameObject)Resources.Load("blood");


        //攻撃関係
        //if (effect!=null)effect.SetActive(false);

        //weapon = new MachineGun(Shoot);
        weapon = new BaseWeapon();
        if (weapon_Image) weapon.SetTexture(weapon_Image);


    }

    // Update is called once per frame
    void Update()
    {
        //受信データの代入
        while (recvDataList.Count > 0)
        {
            var recvData = GetRecvData();
            var header = new GameHeader();
            header.SetHeader(recvData);

            if (header.gameCode == (byte)GameHeader.GameCode.BASICDATA)
            {
                if (this.tag == "Player")
                {
                    SetPlayerStatus(recvData);
                }
                else
                {
                    SetStatus(recvData);
                }
            }
            if(header.gameCode == (byte)GameHeader.GameCode.CHECKDATA)
            {
                SetStatus(recvData);
            }

            //スコア
            if (header.gameCode == (byte)GameHeader.GameCode.SCOREDATA)
            {
                SetScore(recvData);
            }

        }

        //武器関係
        if (weapon != null) weapon.state.Update();
        if (weapon_UI) weapon_UI.text = weapon.remainingBullet + "/" + weapon.magazine;


        //アニメーション変更
        if (stateMachine.currentKey != animationState) stateMachine.ChangeState(animationState);

        //hip.rotation = new Quaternion(hip.rotation.x + initRote.x, hip.rotation.y + initRote.y, hip.rotation.z + initRote.z, hip.rotation.w);
    }

    private void LateUpdate()
    {
        
        Quaternion rotation = Quaternion.Euler(hipRotation.x, hipRotation.y, hipRotation.x);
        hip.rotation *= rotation;
        
    }

    private void SetStatus(byte[] _data)
    {
        int index = bodyData.Deserialize(_data, GameHeader.HEADER_SIZE);
        this.transform.position = bodyData.position;
        this.transform.rotation = Quaternion.Euler(bodyData.rotetion);
        animationState = (AnimationKey)bodyData.animationKey;
        hp = bodyData.hp;
        if (weapon != null)
        {
            //武器の変更
            ChangeWeapon((WEAPONTYPE)System.BitConverter.ToInt32(_data, index));
            //武器ステータス設定
            weapon.SetStatus(_data, index);
        }

    }

    private void SetPlayerStatus(byte[] _data)
    {
        int index = bodyData.Deserialize(_data, GameHeader.HEADER_SIZE);
        this.transform.position = bodyData.position;
        animationState = (AnimationKey)bodyData.animationKey;
        hp = bodyData.hp;
        if (weapon != null)
        {
            //武器の変更
            ChangeWeapon((WEAPONTYPE)System.BitConverter.ToInt32(_data, index));
            //武器ステータス設定
            weapon.SetStatus(_data, index);
        }

    }

    private void SetScore(byte[] _data)
    {
        deathAmount = System.BitConverter.ToInt32(_data, GameHeader.HEADER_SIZE);
        killAmount = System.BitConverter.ToInt32(_data, GameHeader.HEADER_SIZE + sizeof(int));

    }

    private void ChangeWeapon(WEAPONTYPE _type)
    {
        //武器が変更されているかチェック
        if (weapon.type == _type) return;

        //武器の作成
        weapon = BaseWeapon.CreateInstance(_type, Shoot,ShootAnimationChengeInit,ShootAnimationChengeEnd);
        if (weapon_Image) weapon.SetTexture(weapon_Image);

        //武器のモデル変更
        if (!weaponAddPosition) return;
        if (!weapon.model) return;
        if (weaponModel) Destroy(weaponModel);

        //武器インスタンスの作成
        weaponModel = (GameObject)Instantiate(weapon.model);
        weaponModel.transform.parent = weaponAddPosition.transform;

        //武器をローカル座標への移動
        weaponModel.transform.localPosition = weapon.model.transform.position;
        weaponModel.transform.localRotation = weapon.model.transform.rotation;


        effect = weaponModel.transform.Find("Effect").gameObject;
        effect.SetActive(false);
    }

    public void CreateDamageEffect()
    {
        if (damageEffectPref)
        {
            GameObject add = Instantiate(damageEffectPref) as GameObject;
            add.transform.parent = this.transform;
            Vector3 pos = this.transform.position;
            pos += new Vector3(UnityEngine.Random.Range(-0.3f, 0.3f), UnityEngine.Random.Range(0.8f, 1.5f), 0.0f);
            add.transform.position = pos;
        }
    }

    private void ShootAnimationChengeInit()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (weapon.type == WEAPONTYPE.MACHINEGUN)
        {
            if (weapon.state.currentKey == WEAPONSTATE.ATACK)
            {
                if (stateInfo.nameHash != Animator.StringToHash("Upper.Aiming"))
                {
                    animator.CrossFadeInFixedTime("Firing Rifle", 0.1f, 1);
                    animator.SetLayerWeight(1, 1.0f);
                }
            }
            if (weapon.state.currentKey == WEAPONSTATE.RELOAD)
            {
                if((stateInfo.nameHash != Animator.StringToHash("Upper.Reloading"))){
                    animator.CrossFadeInFixedTime("Reloading", 0.1f, 1);
                    animator.SetLayerWeight(1, 1.0f);
                }
            }

        }

        if (weapon.type == WEAPONTYPE.HANDGUN)
        {
            if (weapon.state.currentKey == WEAPONSTATE.WAIT||weapon.state.currentKey == WEAPONSTATE.ATACK)
            {
                if (stateInfo.nameHash != Animator.StringToHash("Upper.Pistol Idle"))
                {
                    animator.CrossFadeInFixedTime("Pistol Idle", 0.1f, 1);
                    animator.SetLayerWeight(1, 1.0f);
                }

                
            }
            if (weapon.state.currentKey == WEAPONSTATE.RELOAD)
            {
                if ((stateInfo.nameHash != Animator.StringToHash("Upper.Reloading")))
                {
                    animator.CrossFadeInFixedTime("Pistol Reload", 0.1f, 1);
                    animator.SetLayerWeight(1, 1.0f);
                }

            }

        }

    }
    private void ShootAnimationChengeEnd()
    {
        animator.SetLayerWeight(1, 0.0f);

    }

    private void Shoot()
    {
        if (effect)
        {
            effect.SetActive(true);
        }

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
                animator.CrossFadeInFixedTime("Idle", 0.1f,0);
                if (modelVisual?.active==false)modelVisual?.SetActive(true);
                
                //animator.CrossFadeInFixedTime("Reloading", 0.1f);
            });

        //Walk
        stateMachine.AddState(AnimationKey.Walk,
            () =>
            {
                animator.CrossFadeInFixedTime("Walk", 0.1f, 0);

            });
        //WalkForward
        stateMachine.AddState(AnimationKey.WalkForward,
            () =>
            {
                animator.CrossFadeInFixedTime("WalkForward", 0.1f, 0);

            });
        //WalkBack
        stateMachine.AddState(AnimationKey.WalkBack,
            () =>
            {
                animator.CrossFadeInFixedTime("WalkBack", 0.1f, 0);

            });
        //WalkLeft
        stateMachine.AddState(AnimationKey.WalkLeft,
            () =>
            {
                animator.CrossFadeInFixedTime("WalkLeft", 0.1f, 0);

            });
        //WalkRight
        stateMachine.AddState(AnimationKey.WalkRight,
            () =>
            {
                animator.CrossFadeInFixedTime("WalkRight", 0.1f, 0);

            });

        //Run
        stateMachine.AddState(AnimationKey.Run,
            () =>
            {
                animator.CrossFadeInFixedTime("Run", 0.1f, 0);
            });
        //RunForward
        stateMachine.AddState(AnimationKey.RunForward,
            () =>
            {
                animator.CrossFadeInFixedTime("RunForward", 0.1f, 0);
            });
        //RunBack
        stateMachine.AddState(AnimationKey.RunBack,
            () =>
            {
                animator.CrossFadeInFixedTime("RunBack", 0.1f, 0);
            });
        //RunLeft
        stateMachine.AddState(AnimationKey.RunLeft,
            () =>
            {
                animator.CrossFadeInFixedTime("RunLeft", 0.1f, 0);
            });
        //RunRight
        stateMachine.AddState(AnimationKey.RunRight,
            () =>
            {
                animator.CrossFadeInFixedTime("RunRight", 0.1f, 0);
            });

        //JumpUP
        stateMachine.AddState(AnimationKey.JumpUP,
            () =>
            {
                animator.CrossFadeInFixedTime("JumpUP", 0.1f, 0);
            }
            );

        //JumpStay
        stateMachine.AddState(AnimationKey.JumpStay,
            () =>
            {
                animator.CrossFadeInFixedTime("JumpStay", 0.1f, 0);
            }
            );


        //JumpDown
        stateMachine.AddState(AnimationKey.JumpDown,
            () =>
            {
                animator.CrossFadeInFixedTime("JumpDown", 0.7f, 0);
            }
            );
        //Dying
        stateMachine.AddState(AnimationKey.Dying,
            () =>
            {
                animator.CrossFadeInFixedTime("Dying", 0.1f, 0);
                this.GetComponent<CapsuleCollider>().enabled = false;
            },
            () =>
            {

            },
            _end: () =>
            {
                if (modelVisual) modelVisual.SetActive(false);
                this.GetComponent<CapsuleCollider>().enabled = true;
            }
            );
        stateMachine.AddState(AnimationKey.Reloading,
            () =>
            {
                animator.CrossFadeInFixedTime("Reloading", 0.1f, 0);
            },
            () =>
            {
            }
            );
    }

}

