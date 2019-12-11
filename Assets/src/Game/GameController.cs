using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] Canvas beforeCanvas;
    [SerializeField] Text timerText;
    public bool stopFlg=false;
    public System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
    public ServerTime serverTime = new ServerTime();
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
        if (timerText)timerText.text = CreateNowTime(timer, serverTime);
         

    }

    private string CreateNowTime(System.Diagnostics.Stopwatch _stopwatch,ServerTime _serverTime)
    {
        int time = _stopwatch.Elapsed.Minutes + _serverTime.Minutes;
        string text = time == 0 ? "00" : time / 10 == 0 ? "0" + time : time.ToString();
        time = _stopwatch.Elapsed.Seconds + _serverTime.Seconds;
        text+= time == 0 ? "00" : time / 10 == 0 ? "0" + time : time.ToString();
        return text;
    }
}

public class ServerTime
{
    public int Minutes = 0;
    public int Seconds = 0;
}