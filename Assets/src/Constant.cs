using System.Collections;
using System.Collections.Generic;
public static class Header
{
    //====================================================
    //定数
    //====================================================
    public static readonly int USERID_LENGTH = 12;
    public static readonly int HEADER_SIZE= sizeof(uint) + sizeof(ID)+sizeof(GameCode)+USERID_LENGTH;

    public enum ID : byte
    {
        DEBUG = 0x001,
        INIT = 0x002,
        GAME = 0x003
    }
    public enum GameCode : byte
    {
        BASICDATA = 0x0001
    }

}

public enum Key : short
{
    W = 0x001,
    S = 0x002,
    A = 0x004,
    D = 0x008,
    SHIFT = 0x010,
    G = 0x020,
    R = 0x040,
    LEFT_BUTTON = 0x080,
    RIGHT_BUTTON = 0x100,
    SPACE=0x200
}


public enum WEAPONTYPE : int
{
    MACHINEGUN
}

public enum WEAPONSTATE : int
{
    WAIT,
    ATACK,
    RELOAD

}



