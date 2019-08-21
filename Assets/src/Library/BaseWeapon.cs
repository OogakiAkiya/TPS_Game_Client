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
    protected Texture2D texture =null;
    protected System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();


    protected byte[] GetStatus(WEAPONTYPE _type)
    {
        List<byte> returnData = new List<byte>();
        returnData.AddRange(Convert.Conversion((int)_type));
        returnData.AddRange(Convert.Conversion((int)state.currentKey));
        returnData.AddRange(Convert.Conversion(remainingBullet));
        return returnData.ToArray();
    }

    public void SetStatus(byte[] _data,int _index)
    {
        type=(WEAPONTYPE)System.BitConverter.ToInt32(_data, _index);             //武器の種類
        var nowStatus = (WEAPONSTATE)System.BitConverter.ToInt32(_data, _index + sizeof(WEAPONTYPE));
        if(state.currentKey!=nowStatus)state.ChangeState(nowStatus);             //状態
        remainingBullet = System.BitConverter.ToInt32(_data, _index + sizeof(WEAPONTYPE) + sizeof(WEAPONSTATE));             //残弾数

    }

    public virtual void SetTexture(Image _image)
    {
        if (!texture) return;
        _image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }
}


public class MachineGun : BaseWeapon
{
    public MachineGun(Action _atack)
    {
        interval = 100;
        power = 10;
        reloadTime = 1000;      //1秒
        magazine = 60;
        remainingBullet = magazine;
        range = 10;
        atackMethod = _atack;
        texture = Resources.Load("Weapon_MachineGun") as Texture2D;

        state.AddState(WEAPONSTATE.WAIT);
        state.AddState(WEAPONSTATE.ATACK,
            () =>
            {
                timer.Restart();
            },
            () =>
            {
                if (timer.ElapsedMilliseconds > interval)
                {
                    atackMethod();
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
        return GetStatus(WEAPONTYPE.MACHINEGUN);
    }

}
