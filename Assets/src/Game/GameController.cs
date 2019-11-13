using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] Canvas beforeCanvas;
    public bool stopFlg=false;
    

    // Start is called before the first frame update
    void Start()
    {
        if (beforeCanvas) stopFlg = true;

        //プレイヤー用オブジェクト準備
        /*
        var objs = GameObject.FindGameObjectsWithTag("Player");
        foreach (var obj in objs) obj.SetActive(false);
        foreach (var obj in objs)
        {
            if(PlayerPrefs.GetString(SavedData.UserType) == "Soldier"&& obj.GetComponent<BaseClient>().GetType().Name==typeof(soldierClient).Name)obj.SetActive(true);
            if (PlayerPrefs.GetString(SavedData.UserType) == "Maynard" && obj.GetComponent<BaseClient>().GetType().Name == typeof(MaynardClient).Name) obj.SetActive(true);
        }
        */
    }

    // Update is called once per frame
    void Update()
    {

    }

}
