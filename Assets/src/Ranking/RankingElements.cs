using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankingElements
{
    public static RankingData[] rankingDatas;

    public static RankingData[] GetRankingData()
    {
        return rankingDatas;
    }
}

public class RankingData
{
    public string name;
    public FinishData finish=new FinishData();

    public void SetData(string _name,int _kill, int _death, int _timeMinute, int _timeSecond, bool _survivalFlg,byte _objectType)
    {
        name = _name;
        finish.killAmount = _kill;
        finish.deathAmount = _death;
        finish.timeMinute = _timeMinute;
        finish.timeSecond = _timeSecond;
        finish.survivalFlg = _survivalFlg;
        finish.objectType = _objectType;
    }
    public void SetData(string _name, FinishData _finishData)
    {
        name = _name;
        finish = _finishData;
    }

}
