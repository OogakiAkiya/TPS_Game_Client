using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] Canvas beforeCanvas;
    [SerializeField] Text timerText;
    public bool stopFlg=false;
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
        if (timerText) timerText.text = CreateNowTime(serverTime);

    }

    private string CreateNowTime(ServerTime _serverTime)
    {
        string text = _serverTime.Minutes == 0 ? "00" : _serverTime.Minutes / 10 == 0 ? "0" + _serverTime.Minutes : _serverTime.Minutes.ToString();
        text += ":";
        text+= _serverTime.Seconds == 0 ? "00" : _serverTime.Seconds / 10 == 0 ? "0" + _serverTime.Seconds : _serverTime.Seconds.ToString();
        return text;
    }

    public void SceneChange()
    {
        SceneManager.LoadScene("Ranking");
/*
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;                        //エディタ(デバッグ)の時のみ動作を止める
#else
        Application.Quit();                                                     //コンパイル後に動作する
#endif
*/
    }
}

public class ServerTime
{
    public int Minutes = 0;
    public int Seconds = 0;
}