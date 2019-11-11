using System.Collections;
using System.Collections.Generic;

public static  class SavedData{
    public static string ServerIP = "ServerIP";
    public static string UserID = "UserID";

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
    LEFT_CLICK = 0x080,        //左クリック
    RIGHT_CLICK = 0x100,       //右クリック
    SPACE=0x200,
    LEFT_BUTTON = 0x400,        //左クリック
    RIGHT_BUTTON = 0x800       //右クリック

}


public enum WEAPONTYPE : int
{
    BASE,
    MACHINEGUN,
    HANDGUN,
    CLAW
}

public enum WEAPONSTATE : int
{
    WAIT,
    ATACK,
    RELOAD

}



