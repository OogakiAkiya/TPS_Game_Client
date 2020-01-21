using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] Canvas beforeCanvas;
    [SerializeField] Text timerText;
    [SerializeField] Text humanPowerText;
    [SerializeField] Text monsterPowerText;
    [SerializeField] GameObject itemsObject;


    public bool stopFlg=false;
    public ServerTime serverTime = new ServerTime();
    public int humanPower=0;
    public int monsterPower = 0;
    public BaseItem[] items;

    private bool mauseLock = true;


    // Start is called before the first frame update
    void Start()
    {
        if (beforeCanvas) stopFlg = true;
        if(itemsObject)items = itemsObject.GetComponentsInChildren<BaseItem>();

        //マウスポインター非表示
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.lockState = CursorLockMode.Locked;
        mauseLock = true;


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

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {
        if (timerText) timerText.text = CreateNowTime(serverTime);
        if (humanPowerText) humanPowerText.text = humanPower.ToString();
        if (monsterPowerText) monsterPowerText.text = monsterPower.ToString();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            mauseLock = !mauseLock;

            if (mauseLock)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    private string CreateNowTime(ServerTime _serverTime)
    {
        ServerTime time = new ServerTime(5,0);
        int mi = time.Minutes - _serverTime.Minutes;
        int se = time.Seconds - _serverTime.Seconds;
        if (se < 0)
        {
            mi--;
            se += 60;
        }

        string text = mi == 0 ? "00" : mi / 10 == 0 ? "0" + mi : mi.ToString();
        text += ":";
        text += se == 0 ? "00" : se/ 10 == 0 ? "0" + se : se.ToString();

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
    public ServerTime() { }
    public ServerTime(int _minutes,int _second)
    {
        Minutes = _minutes;
        Seconds = _second;
    }
}