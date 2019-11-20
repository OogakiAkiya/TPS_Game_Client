using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaynardClient : BaseClient
{
    //自身のモデル
    private GameObject modelVisual;
    [SerializeField] Vector3 attackRange = new Vector3(0.55f, 0.3f, 0.55f);

    // Start is called before the first frame update
    void Start()
    {
        base.Init();
        stateMachine.ChangeState(AnimationKey.Idle);

        modelVisual = transform.Find("Ch30").gameObject;

    }

    // Update is called once per frame
    void Update()
    {
        base.update();   
    }

    protected override void SetStatus(byte[] _data)
    {
        int index = bodyData.Deserialize(_data, GameHeader.HEADER_SIZE);
        this.transform.position = bodyData.position;
        animationState = (AnimationKey)bodyData.animationKey;
        hp = bodyData.hp;

        if (weapon == null) return;
        //武器の変更
        ChangeWeapon((WEAPONTYPE)System.BitConverter.ToInt32(_data, index), Atack);
        //武器ステータス設定
        weapon.SetStatus(_data, index);

    }
    protected override void SetCheckStatus(byte[] _data)
    {
        SetStatus(_data);
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
        Collider[] colliders = Physics.OverlapBox(vector, attackRange, this.transform.localRotation);

        for(int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == Tags.SOLDIER) colliders[i].GetComponent<BaseClient>().CreateDamageEffect();
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

            });
        //WalkForward
        stateMachine.AddState(AnimationKey.WalkForward,
            () =>
            {
                animator.CrossFadeInFixedTime("WalkForward", 0.1f, 0);

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
                animator.speed = 1.5f;
            },
            _end: () => {
                animator.speed = 1.0f;
            });
        //JumpUP
        stateMachine.AddState(AnimationKey.JumpUP,
            () =>
            {
                animator.CrossFadeInFixedTime("JumpUP", 0.05f, 0);
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
    }

}
