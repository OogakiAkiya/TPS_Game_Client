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

    private BaseWeapon weapon = null;
    public GameObject effect = null;
    public GameObject weaponAddPosition = null;
    private GameObject weaponModel;

    //UI
    public Text weapon_UI;
    public Image weapon_Image;
    //当たり判定
    private Camera cam;
    private RectTransform imageRect;
    private Canvas canvas;

    //Score
    public int deathAmount = 0;          //死んだ回数
    public int killAmount = 0;           //殺した回数

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

        if (this.tag != "Player") return;
        cam = transform.Find("Camera").gameObject.GetComponent<Camera>();
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        imageRect = GameObject.Find("Canvas").transform.Find("Pointer").GetComponent<RectTransform>();

    }

    // Update is called once per frame
    void Update()
    {
        //受信データの代入
        while (recvDataList.Count > 0)
        {
            var recvData = GetRecvData();
            /*
            if (recvData[sizeof(uint) + sizeof(byte) + Header.USERID_LENGTH] == (byte)Header.GameCode.BASICDATA)
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
            
            //スコア
            if (recvData[sizeof(uint) + sizeof(byte) + Header.USERID_LENGTH] == (byte)Header.GameCode.SCOREDATA) SetScore(recvData);
            */

            if (recvData[sizeof(byte) + GameHeader.USERID_LENGTH] == (byte)GameHeader.GameCode.BASICDATA)
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

            //スコア
            if (recvData[sizeof(byte) + GameHeader.USERID_LENGTH] == (byte)GameHeader.GameCode.SCOREDATA) SetScore(recvData);

        }

        //武器関係
        weapon.state.Update();
        if (weapon_UI) weapon_UI.text = weapon.remainingBullet + "/" + weapon.magazine;


        //アニメーション変更
        if (stateMachine.currentKey != animationState) stateMachine.ChangeState(animationState);
    }

    private void SetStatus(byte[] _data)
    {
        //座標の代入
        this.transform.position = Convert.GetVector3(_data, GameHeader.HEADER_SIZE);

        //アニメーション変更
        animationState = (AnimationKey)System.BitConverter.ToInt32(_data, GameHeader.HEADER_SIZE + 6 * sizeof(float));

        //hpの設定
        hp = System.BitConverter.ToInt32(_data, GameHeader.HEADER_SIZE + 7 * sizeof(float));

        //回転座標の代入
        this.transform.rotation = Quaternion.Euler(Convert.GetVector3(_data, GameHeader.HEADER_SIZE + 3 * sizeof(float)));

    }

    private void SetPlayerStatus(byte[] _data)
    {
        //どこかおかしい？
        //座標の代入
        this.transform.position = Convert.GetVector3(_data, GameHeader.HEADER_SIZE);

        //アニメーション変更
        animationState = (AnimationKey)System.BitConverter.ToInt32(_data, GameHeader.HEADER_SIZE + 6 * sizeof(float));

        //hpの設定
        hp = System.BitConverter.ToInt32(_data, GameHeader.HEADER_SIZE + 7 * sizeof(float));

        if (weapon != null)
        {
            //武器の変更
            ChangeWeapon((WEAPONTYPE)System.BitConverter.ToInt32(_data, GameHeader.HEADER_SIZE + 7 * sizeof(float) + sizeof(int)));
            //武器ステータス設定
            weapon.SetStatus(_data, GameHeader.HEADER_SIZE + 7 * sizeof(float) + sizeof(int));
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
        weapon = BaseWeapon.CreateInstance(_type, Shoot);
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
            pos += new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.8f, 1.5f), 0.0f);
            add.transform.position = pos;
        }
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
                animator.CrossFadeInFixedTime("Idle", 0.1f);
                //animator.CrossFadeInFixedTime("Reloading", 0.1f);
            });

        //Walk
        stateMachine.AddState(AnimationKey.Walk,
            () =>
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
