using System.Collections;
using System;
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

    private void Awake()
    {
        if (this.tag != "Player") return;
        cam = transform.Find("Camera").gameObject.GetComponent<Camera>();
        canvas = GameObject.Find("GameCanvas").GetComponent<Canvas>();
        imageRect = GameObject.Find("GameCanvas").transform.Find("Pointer").GetComponent<RectTransform>();

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
            pos += new Vector3(UnityEngine.Random.Range(-0.3f, 0.3f), UnityEngine.Random.Range(0.8f, 1.5f), 0.0f);
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

public class UserBodyData
{
    public enum USERDATAFLG : byte
    {
        POSITION_X = 0x001,
        POSITION_Y = 0x002,
        POSITION_Z = 0x004,
        ROTATION_X = 0x008,
        ROTATION_Y = 0x010,
        ROTATION_Z = 0x020,
    }


    public Vector3 position = Vector3.zero;
    public Vector3 rotetion = Vector3.zero;
    public int animationKey = 0;
    public int hp = 0;

    private USERDATAFLG sendDataFlg = 0x000;

    void SetData(Vector3 _position, Vector3 _rotetion, int _currentKey, int _hp)
    {
        //position
        {
            bool flg = false;
            if (Math.Abs(position.x - _position.x) > 0.001)
            {
                sendDataFlg |= USERDATAFLG.POSITION_X;
                flg = true;
            }
            if (Math.Abs(position.y - _position.y) > 0.001)
            {
                sendDataFlg |= USERDATAFLG.POSITION_Y;
                flg = true;
            }
            if (Math.Abs(position.y - _position.y) > 0.001)
            {
                sendDataFlg |= USERDATAFLG.POSITION_Z;
                flg = true;
            }
            if (flg) position = _position;

        }

        //rotation
        {
            bool flg = false;
            if (Math.Abs(rotetion.x - _rotetion.x) > 0.001)
            {
                sendDataFlg |= USERDATAFLG.ROTATION_X;
                flg = true;
            }
            if (Math.Abs(rotetion.y - _rotetion.y) > 0.001)
            {
                sendDataFlg |= USERDATAFLG.ROTATION_Y;
                flg = true;
            }
            if (Math.Abs(rotetion.y - _rotetion.y) > 0.001)
            {
                sendDataFlg |= USERDATAFLG.ROTATION_Z;
                flg = true;
            }
            if (flg) rotetion = _rotetion;

        }

        animationKey = _currentKey;
        hp = _hp;
    }

    byte[] GetData(UserBodyData _oldData = null)
    {

        List<byte> returnData = new List<byte>();

        //送信データ作成
        returnData.Add((byte)sendDataFlg);
        if (sendDataFlg.HasFlag(USERDATAFLG.POSITION_X)) returnData.AddRange(BitConverter.GetBytes(position.x));
        if (sendDataFlg.HasFlag(USERDATAFLG.POSITION_Y)) returnData.AddRange(BitConverter.GetBytes(position.y));
        if (sendDataFlg.HasFlag(USERDATAFLG.POSITION_Z)) returnData.AddRange(BitConverter.GetBytes(position.z));
        if (sendDataFlg.HasFlag(USERDATAFLG.ROTATION_X)) returnData.AddRange(BitConverter.GetBytes(rotetion.x));
        if (sendDataFlg.HasFlag(USERDATAFLG.ROTATION_Y)) returnData.AddRange(BitConverter.GetBytes(rotetion.y));
        if (sendDataFlg.HasFlag(USERDATAFLG.ROTATION_Z)) returnData.AddRange(BitConverter.GetBytes(rotetion.z));

        returnData.AddRange(BitConverter.GetBytes(animationKey));
        returnData.AddRange(BitConverter.GetBytes(hp));
        sendDataFlg = 0;


        return returnData.ToArray();
    }

    void Deserialize(byte[] _data, int _index)
    {
        int nowIndex = _index;
        USERDATAFLG nowFlg = (USERDATAFLG)_data[_index++];
        if (nowFlg.HasFlag(USERDATAFLG.POSITION_X)) position.x = GetFloat(_data, ref nowIndex);
        if (nowFlg.HasFlag(USERDATAFLG.POSITION_Y)) position.y = GetFloat(_data, ref nowIndex);
        if (nowFlg.HasFlag(USERDATAFLG.POSITION_Z)) position.z = GetFloat(_data, ref nowIndex);
        if (nowFlg.HasFlag(USERDATAFLG.ROTATION_X)) rotetion.x = GetFloat(_data, ref nowIndex);
        if (nowFlg.HasFlag(USERDATAFLG.ROTATION_Y)) rotetion.y = GetFloat(_data, ref nowIndex);
        if (nowFlg.HasFlag(USERDATAFLG.ROTATION_Z)) rotetion.z = GetFloat(_data, ref nowIndex);
        animationKey = BitConverter.ToInt32(_data, nowIndex);
        nowIndex += sizeof(int);
        hp = BitConverter.ToInt32(_data, nowIndex);
    }
    private float GetFloat(byte[] _data, ref int _startIndex)
    {
        _startIndex += 4;
        return BitConverter.ToSingle(_data, _startIndex - 4);
    }
}
