﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LastRanking : MonoBehaviour
{
    [SerializeField] Text soldier;
    [SerializeField] Text monster;

    // Start is called before the first frame update
    void Start()
    {
        if (!soldier) return;
        if (!monster) return;
        soldier.text = "";
        monster.text = "";
        var rank = RankingElements.GetRankingData();

        foreach (RankingData data in rank)
        {
            FinishData finish = data.finish;
            int score = ScoreCount(finish);
            if (data.finish.objectType == (byte)GameHeader.UserTypeCode.SOLDIER)
            {
                soldier.text += "1. "+data.name+"kill:"+finish.killAmount+"  death:"+finish.deathAmount+"  "+score+"p"+"\n";
            }
            if(data.finish.objectType==(byte)GameHeader.UserTypeCode.MONSTER)
            {
                monster.text += "1. " + data.name + "kill:" + data.finish.killAmount + "  death:" + data.finish.deathAmount+"  100p" + "\n";
            }
            
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private int ScoreCount(FinishData _finish)
    {
        int score = 0;
        score += _finish.deathAmount * -10;
        score += _finish.killAmount * 100;
        score += _finish.timeSecond * 10;
        score += _finish.timeMinute * 1;
        if (_finish.survivalFlg) score += 500;
        return score;
    }
}
