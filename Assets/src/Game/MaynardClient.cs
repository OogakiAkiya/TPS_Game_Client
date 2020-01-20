using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MaynardClient : BaseClient
{
    //自身のモデル
    private GameObject modelVisual;
    [SerializeField] Vector3 attackRange = new Vector3(0.55f, 0.3f, 0.55f);
    [SerializeField] public Slider slider = null;
    private string parentTag;
    private MonsterType type = MonsterType.MAYNARD;

    private GameObject eggPref;
    private GameObject egg;
    private Vector3 scale = new Vector3(0, 0, 0);

    [SerializeField] AudioClip modelChangeAudio;
    [SerializeField] AudioClip cryAudio;
    [SerializeField] ClientParent parent;

    private System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

    // Start is called before the first frame update
    void Start()
    {
        base.Init();
        //stateMachine.ChangeState(AnimationKey.Idle);

        modelVisual = transform.Find("Ch30").gameObject;
        parentTag = this.transform.parent.tag;

        eggPref = (GameObject)Resources.Load("egg");
    }

    // Update is called once per frame
    void Update()
    {
        base.update();
        if (timer.Elapsed.Seconds > 5)
        {
            parent.audioSource.pitch = 1.0f;
            parent.audioSource.loop = false;
            parent.audioSource.PlayOneShot(cryAudio);
            timer.Restart();
        }

    }

    protected override void SetStatus(byte[] _data,int _index=0)
    {
        if (type != (MonsterType)_data[GameHeader.HEADER_SIZE])
        {
            ChangeModel((MonsterType)_data[GameHeader.HEADER_SIZE], _data);
            return;
        }

        if (parentTag != Tags.PLAYER) base.SetStatus(_data,sizeof(byte));
        int index = bodyData.Deserialize(_data, GameHeader.HEADER_SIZE+sizeof(byte));
        this.transform.position = bodyData.position;
        animationState = (AnimationKey)bodyData.animationKey;
        hp = bodyData.hp;


        //changeConter
        if (slider) slider.value = Convert.IntConversion(_data, index);
        index += 4;
        
        if (weapon == null) return;
        //武器の変更
        ChangeWeapon((WEAPONTYPE)System.BitConverter.ToInt32(_data, index), Atack);
        //武器ステータス設定
        weapon.SetStatus(_data, index);
    }

    protected void ChangeModel(MonsterType _type, byte[] _data)
    {
        this.animationState = AnimationKey.Idle;
        this.stateMachine.ChangeState(animationState);
        this.gameObject.SetActive(false);
        int index = bodyData.Deserialize(_data, GameHeader.HEADER_SIZE + sizeof(byte));
        BaseClient obj = this.transform.parent.GetComponent<ClientParent>().ChangeModel(_type).GetComponent<BaseClient>();

        //座標を直接代入するのはオブジェクトが変更されたとき違和感をなくす為
        obj.transform.position = bodyData.position;
        obj.transform.rotation = Quaternion.Euler(bodyData.rotetion);

        
        obj.Init(bodyData,animationState);                         //オブジェクトのボディーデータを引き継がせる
        obj.AddRecvData(_data);                     //現在のレシーブデータを変更したモデルデータに渡す
        obj.userID = this.userID;
        if(parentTag == Tags.PLAYER) GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().current = obj;
        GameObject.FindGameObjectWithTag("Server").GetComponent<ClientController>().UpdateUserArray();
    }

    protected override void SetCheckStatus(byte[] _data, int _index=0)
    {
        if (parentTag != Tags.PLAYER)
        {
            base.SetStatus(_data, sizeof(byte));
        }
    }
    public override void CreateDamageEffect()
    {
        if (!damageEffectPref) return;
            GameObject add = Instantiate(damageEffectPref) as GameObject;
            add.transform.parent = this.transform;
            Vector3 pos = this.transform.position;
            pos += new Vector3(UnityEngine.Random.Range(-0.3f, 0.3f), UnityEngine.Random.Range(0.8f, 1.5f), 0.0f);
            add.transform.position = pos;
    }

    protected override void Atack()
    {
        if (effect) effect.SetActive(true);

        Vector3 vector = this.transform.position + this.transform.forward * 0.3f + this.transform.up;
        Collider[] colliders = Physics.OverlapBox(vector, attackRange, this.transform.localRotation, 1 << 9);

        for(int i = 0; i < colliders.Length; i++)
        {
            //Layer管理
            colliders[i].GetComponent<BaseClient>().CreateDamageEffect();
        }

    }

    protected override void AddStates()
    {
        //Idle
        stateMachine.AddState(AnimationKey.Idle,
            () =>
            {
                animator.CrossFadeInFixedTime("Idle", 0.1f, 0);
                if (modelVisual?.active == false) modelVisual?.SetActive(true);
                //animator.CrossFadeInFixedTime("Reloading", 0.1f);
            });

        //Walk
        stateMachine.AddState(AnimationKey.Walk,
            () =>
            {
                animator.CrossFadeInFixedTime("Walk", 0.1f, 0);
                audioSource.loop = true;
                audioSource.pitch = 1.3f;
                audioSource.PlayOneShot(walkAudio);
            },
            _end: () => {
                audioSource.loop = false;
                audioSource.Stop();
            });
        //WalkForward
        stateMachine.AddState(AnimationKey.WalkForward,
            () =>
            {
                animator.CrossFadeInFixedTime("WalkForward", 0.1f, 0);
                audioSource.loop = true;
                audioSource.pitch = 1.3f;
                audioSource.PlayOneShot(walkAudio);
            },
            _end: () => {
                audioSource.loop = false;
                audioSource.Stop();
            });
        //Run
        stateMachine.AddState(AnimationKey.Run,
            () =>
            {
                animator.CrossFadeInFixedTime("Run", 0.1f, 0);
                audioSource.loop = true;
                audioSource.pitch = 1.9f;
                audioSource.PlayOneShot(walkAudio);
            },
            _end: () => {
                audioSource.loop = false;
                audioSource.Stop();
            });
        //RunForward
        stateMachine.AddState(AnimationKey.RunForward,
            () =>
            {
                animator.CrossFadeInFixedTime("RunForward", 0.1f, 0);
                animator.speed = 1.5f;
                audioSource.loop = true;
                audioSource.pitch = 1.9f;
                audioSource.PlayOneShot(walkAudio);
            },
            _end: () => {
                animator.speed = 1.0f;
                audioSource.loop = false;
                audioSource.Stop();
            });
        //JumpUP
        stateMachine.AddState(AnimationKey.JumpUP,
            () =>
            {
                animator.CrossFadeInFixedTime("JumpUP", 0.05f, 0);
                audioSource.pitch = 1.0f;
                audioSource.loop = false;
                audioSource.Stop();
            }
            );

        //JumpStay
        stateMachine.AddState(AnimationKey.JumpStay,
            () =>
            {
                animator.CrossFadeInFixedTime("JumpStay", 0, 0);
            }
            );


        //JumpDown
        stateMachine.AddState(AnimationKey.JumpDown,
            () =>
            {
                animator.CrossFadeInFixedTime("JumpDown", 0.7f, 0);
                audioSource.pitch = 1.0f;
                audioSource.Stop();
                audioSource.PlayOneShot(jumpDownAudio);
            },
            _end: () => {
                animator.speed = 1.0f;
                audioSource.loop = false;
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
        stateMachine.AddState(AnimationKey.Attack, () =>
        {
            animator.CrossFadeInFixedTime("Attack", 0.1f, 0);

        });
        stateMachine.AddState(AnimationKey.Reloading,
            () =>
            {
                animator.CrossFadeInFixedTime("Reloading", 0.1f, 0);
            },
            () =>
            {
            }
            );
        stateMachine.AddState(AnimationKey.ModelChange, () =>
        {
            Vector3 pos = this.transform.position;
            pos.y += 0.8f;
            egg =Instantiate(eggPref,pos,this.transform.rotation)as GameObject;
            animator.CrossFadeInFixedTime("Agony", 0.1f, 0);
            egg.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            audioSource.Stop();
            audioSource.PlayOneShot(modelChangeAudio);
            audioSource.pitch = 1.0f;
        },
        () =>
        {
            egg.transform.localScale = scale;
            if (scale.x < 1.85f) scale += new Vector3(0.015f, 0.015f, 0.015f);

        },
        () =>
        {
            audioSource.Stop();
            Destroy(egg);
        }
        );
    }

}
