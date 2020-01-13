using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class BaseWeapon
{
    public StateMachine<WEAPONSTATE> state = new StateMachine<WEAPONSTATE>();

    protected long interval;                                //撃つ間隔
    public int power { get; protected set; }                //威力
    protected int reloadTime;                               //リロード時間
    public int magazine { get; protected set; }             //弾数
    public int remainingBullet { get; protected set; }      //残弾数
    public float range { get; protected set; }              //射程
    public WEAPONTYPE type { get; protected set; }
    protected Action atackMethod;                           //攻撃時メソッド
    protected Action atackAnimationInit;                           //攻撃時メソッド
    protected Action atackAnimationEnd;                           //攻撃時メソッド

    protected Texture2D texture = null;                      //UIテクスチャ
    public GameObject model = null;                      //武器のプレハブ

    protected System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();


    protected byte[] GetStatus(WEAPONTYPE _type)
    {
        List<byte> returnData = new List<byte>();
        returnData.AddRange(Convert.Conversion((int)_type));
        returnData.AddRange(Convert.Conversion((int)state.currentKey));
        returnData.AddRange(Convert.Conversion(remainingBullet));
        return returnData.ToArray();
    }

    public void SetStatus(byte[] _data, int _index)
    {
        type = (WEAPONTYPE)System.BitConverter.ToInt32(_data, _index);             //武器の種類
        var nowStatus = (WEAPONSTATE)System.BitConverter.ToInt32(_data, _index + sizeof(WEAPONTYPE));
        if (state.currentKey != nowStatus) state.ChangeState(nowStatus);             //状態
        remainingBullet = System.BitConverter.ToInt32(_data, _index + sizeof(WEAPONTYPE) + sizeof(WEAPONSTATE));             //残弾数

    }

    public virtual void SetTexture(Image _image)
    {
        if (!texture) return;
        _image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }

    public static BaseWeapon CreateInstance(WEAPONTYPE _type, Action _action = null, Action _animationInit = null, Action _animationEnd = null)
    {
        if (_type == WEAPONTYPE.MACHINEGUN) return new MachineGun(_action,_animationInit,_animationEnd);
        if (_type == WEAPONTYPE.HANDGUN) return new HandGun(_action, _animationInit, _animationEnd);
        if (_type == WEAPONTYPE.UMP45) return new UMP45(_action,_animationInit,_animationEnd);
        if (_type == WEAPONTYPE.CLAW) return new Claw(_action);

        return null;
    }
}


public class MachineGun : BaseWeapon
{
    public MachineGun(Action _atack, Action _animationInit = null, Action _animationEnd = null)
    {
        interval = 100;
        power = 10;
        reloadTime = 1000;      //1秒
        magazine = 60;
        remainingBullet = magazine;
        range = 15;
        atackMethod = _atack;
        atackAnimationInit = _animationInit;
        atackAnimationEnd = _animationEnd;
        texture = Resources.Load("Weapon_MachineGun") as Texture2D;
        model = Resources.Load("M4a1") as GameObject;
        type = WEAPONTYPE.BASE;

        state.AddState(WEAPONSTATE.WAIT,()=>
        {
            atackAnimationEnd?.Invoke();
        });
        state.AddState(WEAPONSTATE.ATACK,
            () =>
            {
                timer.Restart();
                atackAnimationInit?.Invoke();
            },
            () =>
            {
                if (timer.ElapsedMilliseconds > interval)
                {
                    atackMethod?.Invoke();
                    timer.Restart();
                }
            },
            () =>
            {
                timer.Stop();
            }
            );
        state.AddState(WEAPONSTATE.RELOAD,
            () =>
            {
                atackAnimationInit?.Invoke();
            },
            () =>
            {
            },
            () =>
            {
            }
            );

        state.ChangeState(WEAPONSTATE.WAIT);
    }

    public byte[] GetStatus()
    {
        return GetStatus(type);
    }
    public override void SetTexture(Image _image)
    {
        if (!texture) return;
        _image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        _image.GetComponent<Transform>().localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }


}

public class UMP45 : BaseWeapon
{
    public UMP45(Action _atack, Action _animationInit = null, Action _animationEnd = null)
    {
        interval = 20;
        power = 3;
        reloadTime = 500;      //1秒
        magazine = 120;
        remainingBullet = magazine;
        range = 15;
        atackMethod = _atack;
        atackAnimationInit = _animationInit;
        atackAnimationEnd = _animationEnd;
        texture = Resources.Load("Weapon_UMP-45") as Texture2D;
        model = Resources.Load("UMP-45") as GameObject;
        type = WEAPONTYPE.UMP45;

        state.AddState(WEAPONSTATE.WAIT, () =>
        {
            atackAnimationEnd?.Invoke();
        });
        state.AddState(WEAPONSTATE.ATACK,
            () =>
            {
                timer.Restart();
                atackAnimationInit?.Invoke();
            },
            () =>
            {
                if (timer.ElapsedMilliseconds > interval)
                {
                    atackMethod?.Invoke();
                    timer.Restart();
                }
            },
            () =>
            {
                timer.Stop();
            }
            );
        state.AddState(WEAPONSTATE.RELOAD,
            () =>
            {
                atackAnimationInit?.Invoke();
            },
            () =>
            {
            },
            () =>
            {
            }
            );

        state.ChangeState(WEAPONSTATE.WAIT);
    }

    public byte[] GetStatus()
    {
        return GetStatus(type);
    }
    public override void SetTexture(Image _image)
    {
        if (!texture) return;
        _image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        _image.GetComponent<Transform>().localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }
}


public class HandGun : BaseWeapon
{
    public HandGun(Action _atack, Action _animationInit = null, Action _animationEnd = null)
    {
        interval = 1000;
        power = 10;
        reloadTime = 1000;      //1秒
        magazine = 12;
        remainingBullet = magazine;
        range = 5;
        atackMethod = _atack;
        atackAnimationInit = _animationInit;
        atackAnimationEnd = _animationEnd;

        texture = Resources.Load("Weapon_HandGun") as Texture2D;
        model = Resources.Load("M9") as GameObject;

        type = WEAPONTYPE.HANDGUN;

        state.AddState(WEAPONSTATE.WAIT,()=> {
            atackAnimationEnd?.Invoke();

        },()=>
        {
            atackAnimationInit?.Invoke();
        });
        state.AddState(WEAPONSTATE.ATACK,
            () =>
            {
                timer.Restart();
                atackAnimationInit?.Invoke();
            },
            () =>
            {
                if (timer.ElapsedMilliseconds > interval)
                {
                    atackMethod?.Invoke();
                    timer.Restart();
                }
            },
            () =>
            {
                timer.Stop();
            }
            );
        state.AddState(WEAPONSTATE.RELOAD,
            () =>
            {
                atackAnimationInit?.Invoke();
            },
            () =>
            {
            },
            () =>
            {
            }
            );

        state.ChangeState(WEAPONSTATE.WAIT);
    }

    public byte[] GetStatus()
    {
        return GetStatus(type);
    }

    public override void SetTexture(Image _image)
    {
        if (!texture) return;
        _image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        _image.GetComponent<Transform>().localScale = new Vector3(0.3f, 0.3f, 0.3f);
    }

}


public class Claw : BaseWeapon
{
    public Claw(Action _atack)
    {
        interval = 1000;
        power = 10;
        reloadTime = 1000;      //1秒
        magazine = 12;
        remainingBullet = magazine;
        range = 5;
        atackMethod = _atack;

        texture = Resources.Load("Claw") as Texture2D;
        model = Resources.Load("M9") as GameObject;

        type = WEAPONTYPE.CLAW;

        state.AddState(WEAPONSTATE.WAIT, () => {

        }, () =>
        {
        });
        state.AddState(WEAPONSTATE.ATACK,
            () =>
            {
                timer.Restart();
            },
            () =>
            {
                if (timer.ElapsedMilliseconds > interval)
                {
                    atackMethod?.Invoke();
                    timer.Restart();
                }
            },
            () =>
            {
                timer.Stop();
            }
            );
        state.AddState(WEAPONSTATE.RELOAD,
            () =>
            {
                atackAnimationInit?.Invoke();
            },
            () =>
            {
            },
            () =>
            {
            }
            );

        state.ChangeState(WEAPONSTATE.WAIT);
    }

    public byte[] GetStatus()
    {
        return GetStatus(type);
    }

    public override void SetTexture(Image _image)
    {
        if (!texture) return;
        _image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        _image.GetComponent<Transform>().localScale = new Vector3(0.25f, 0.25f, 0.25f);
    }

}

