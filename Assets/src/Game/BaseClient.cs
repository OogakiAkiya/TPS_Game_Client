using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class BaseClient : MonoBehaviour
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
        Attack,
        ModelChange,
        Dying
    }

    protected UserBodyData bodyData = new UserBodyData();

    public string userID;
    [SerializeField] public int hp { get;protected set; } = 100;
    List<byte[]> recvDataList = new List<byte[]>();
    protected AnimationKey animationState = AnimationKey.Idle;
    protected Animator animator;
    public StateMachine<AnimationKey> stateMachine { get; private set; } = new StateMachine<AnimationKey>();
    protected GameObject damageEffectPref;

    protected BaseWeapon weapon = null;
    [SerializeField] protected GameObject effect = null;
    [SerializeField] protected GameObject weaponAddPosition = null;
    protected GameObject weaponModel;

    //Score
    public int deathAmount { get; private set; } = 0;          //死んだ回数
    public int killAmount { get; private set; } = 0;           //殺した回数
                                                               //UI
    [SerializeField]protected Text weapon_UI;
    [SerializeField]protected Image weapon_Image;

    public AnimationKey current;


    protected void Init()
    {
        animator = this.GetComponent<Animator>();
        damageEffectPref = (GameObject)Resources.Load("blood");
        AddStates();

        weapon = new BaseWeapon();
        if (weapon_Image) weapon.SetTexture(weapon_Image);


    }
    protected void update()
    {
        current = stateMachine.currentKey;
        //受信データの代入
        while (recvDataList.Count > 0)
        {
            var recvData = GetRecvData();
            var header = new GameHeader();
            header.SetHeader(recvData);

            if (header.gameCode == (byte)GameHeader.GameCode.BASICDATA)
            {
                SetStatus(recvData);
            }
            if (header.gameCode == (byte)GameHeader.GameCode.CHECKDATA)
            {
                SetCheckStatus(recvData);
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
        //モデルが変わったときにここが通らない
        stateMachine.Update();
        if (stateMachine.currentKey != animationState) stateMachine.ChangeState(animationState);

        //hip.rotation = new Quaternion(hip.rotation.x + initRote.x, hip.rotation.y + initRote.y, hip.rotation.z + initRote.z, hip.rotation.w);

    }

    protected virtual void SetStatus(byte[] _data,int _index=0)
    {
        int index = bodyData.Deserialize(_data, GameHeader.HEADER_SIZE+_index);
        this.transform.position = bodyData.position;
        this.transform.rotation = Quaternion.Euler(bodyData.rotetion);
        animationState = (AnimationKey)bodyData.animationKey;
        hp = bodyData.hp;
        if (weapon != null)
        {
            //武器の変更
            ChangeWeapon((WEAPONTYPE)System.BitConverter.ToInt32(_data, index), Atack);
            //武器ステータス設定
            weapon.SetStatus(_data, index);
        }

    }

    protected virtual void SetCheckStatus(byte[] _data,int _index=0)
    {
        int index = bodyData.Deserialize(_data, GameHeader.HEADER_SIZE+_index);
        this.transform.position = bodyData.position;
        this.transform.rotation = Quaternion.Euler(bodyData.rotetion);
        animationState = (AnimationKey)bodyData.animationKey;
        hp = bodyData.hp;
        if (weapon != null)
        {
            //武器の変更
            ChangeWeapon((WEAPONTYPE)System.BitConverter.ToInt32(_data, index), Atack);
            //武器ステータス設定
            weapon.SetStatus(_data, index);
        }

    }

    private void SetScore(byte[] _data)
    {
        deathAmount = System.BitConverter.ToInt32(_data, GameHeader.HEADER_SIZE);
        killAmount = System.BitConverter.ToInt32(_data, GameHeader.HEADER_SIZE + sizeof(int));

    }

    protected void ChangeWeapon(WEAPONTYPE _type, Action _atack, Action _init = null, Action _finish = null)
    {
        //武器が変更されているかチェック
        if (weapon.type == _type) return;

        //武器の作成
        weapon = BaseWeapon.CreateInstance(_type, _atack, _init, _finish);
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

    /*
    public void Init(UserBodyData _data)
    {
        bodyData = _data;
    }
    */

    public void Init(UserBodyData _data, AnimationKey _key)
    {
        bodyData = _data;
        animationState = _key;
    }
    
    public virtual void CreateDamageEffect()
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

    protected virtual void Atack()
    {
        //攻撃用エフェクトの表示
        if (effect) effect.SetActive(true);

        /*
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
        */
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

    protected virtual void AddStates()
    {

    }



}

