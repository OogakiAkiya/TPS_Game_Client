using System.Collections;
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
        foreach(RankingData data in rank)
        {
            
            if (data.finish.objectType == (byte)GameHeader.UserTypeCode.SOLDIER)
            {
                soldier.text += data.name+"kill:"+data.finish.killAmount+"  death:"+data.finish.deathAmount+"\n";
            }
            if(data.finish.objectType==(byte)GameHeader.UserTypeCode.MONSTER)
            {
                monster.text += data.name + "kill:" + data.finish.killAmount + "  death:" + data.finish.deathAmount+ "\n";

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
