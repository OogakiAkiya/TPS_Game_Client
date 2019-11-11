using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soldierClient : Client
{
    //当たり判定
    public Camera cam;
    public RectTransform imageRect;
    public Canvas canvas;

    //ボーン要素
    private Transform hip;
    public Vector3 hipRotation = Vector3.zero;
    private GameObject modelVisual;

    public void Awake()
    {
        hip = this.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips);
        cam = transform.Find("Camera").gameObject.GetComponent<Camera>();
        modelVisual = transform.Find("Geo").gameObject;
        GameObject gameCanvas = GameObject.Find("GameCanvas");
        canvas = gameCanvas.GetComponent<Canvas>();
        imageRect = gameCanvas.transform.Find("Pointer").GetComponent<RectTransform>();
        

    }
    // Start is called before the first frame update
    public void Start()
    {
        base.Init();
        AddStates();
        stateMachine.ChangeState(AnimationKey.Idle);

    }

    // Update is called once per frame
    public void Update()
    {
        base.update();
    }

    public void LateUpdate()
    {
        Quaternion rotation = Quaternion.Euler(hipRotation.x, hipRotation.y, hipRotation.x);
        hip.rotation *= rotation;

    }

    protected override void SetStatus(byte[] _data)
    {
        int index = bodyData.Deserialize(_data, GameHeader.HEADER_SIZE);
        this.transform.position = bodyData.position;
        animationState = (AnimationKey)bodyData.animationKey;
        hp = bodyData.hp;
        if (weapon != null)
        {
            //武器の変更
            ChangeWeapon((WEAPONTYPE)System.BitConverter.ToInt32(_data, index),Atack,ShootAnimationChengeInit,ShootAnimationChengeEnd);
            //武器ステータス設定
            weapon.SetStatus(_data, index);
        }
    }
    protected override void SetCheckStatus(byte[] _data)
    {
        SetStatus(_data);
    }

    public override void CreateDamageEffect()
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

    protected override void Atack()
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
                if ((stateInfo.nameHash != Animator.StringToHash("Upper.Reloading")))
                {
                    animator.CrossFadeInFixedTime("Reloading", 0.1f, 1);
                    animator.SetLayerWeight(1, 1.0f);
                }
            }

        }

        if (weapon.type == WEAPONTYPE.HANDGUN)
        {
            if (weapon.state.currentKey == WEAPONSTATE.WAIT || weapon.state.currentKey == WEAPONSTATE.ATACK)
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


    Vector2 GetUIScreenPos(RectTransform rt)
    {

        //UIのCanvasに使用されるカメラは Hierarchy 上には表示されないので、
        //変換したいUIが所属しているcanvasを映しているカメラを取得し、 WorldToScreenPoint() で座標変換する
        return RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rt.position);

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
                animator.speed = 1.5f;
            },
            _end: () => {
                animator.speed = 1.0f;
            });
        //RunBack
        stateMachine.AddState(AnimationKey.RunBack,
            () =>
            {
                animator.CrossFadeInFixedTime("RunBack", 0.1f, 0);
                animator.speed = 1.5f;

            },
            _end: () => {
                animator.speed = 1.0f;
            });
        //RunLeft
        stateMachine.AddState(AnimationKey.RunLeft,
            () =>
            {
                animator.CrossFadeInFixedTime("RunLeft", 0.1f, 0);
                animator.speed = 1.5f;
            },
            _end: () => {
                animator.speed = 1.0f;
            });
        //RunRight
        stateMachine.AddState(AnimationKey.RunRight,
            () =>
            {
                animator.CrossFadeInFixedTime("RunRight", 0.1f, 0);
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
