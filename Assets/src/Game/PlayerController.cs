using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private Vector3 oldRotation;
    private Vector2 mouse=new Vector2(0,0);
    private bool shootFlg=false;


    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey(SavedData.UserID)) this.name = PlayerPrefs.GetString(SavedData.UserID);
        oldRotation = transform.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {

        //視点移動
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");
        if (mouse.y + y > 30) y = 0;
        if (mouse.y + y < -60) y = 0;

        mouse += new Vector2(x, y);
        this.transform.rotation = Quaternion.Euler(new Vector3(-mouse.y, mouse.x,0));

    }

    //キー入力を返す
    public Key InputUpdate()
    {
        Key sendKey = 0;

        sendKey |= InputTemple(KeyCode.W, Key.W);
        sendKey |= InputTemple(KeyCode.S, Key.S);
        sendKey |= InputTemple(KeyCode.A, Key.A);
        sendKey |= InputTemple(KeyCode.D, Key.D);
        sendKey |= InputTemple(KeyCode.R, Key.R);
        sendKey |= InputTemple(KeyCode.LeftShift, Key.SHIFT);
        sendKey |= InputTemple(KeyCode.Space, Key.SPACE);
        if (Input.GetKeyDown(KeyCode.LeftArrow))sendKey |= Key.LEFT_BUTTON;
        if (Input.GetKeyDown(KeyCode.RightArrow)) sendKey |= Key.RIGHT_BUTTON;
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
        {
            sendKey |= Key.LEFT_CLICK;
            shootFlg = !shootFlg;
        }
        return sendKey;
    }

    private Key InputTemple(KeyCode _key, Key _keyCode)
    {
        if (Input.GetKeyUp(_key) || Input.GetKeyDown(_key))
        {
            return _keyCode;
        }
        return 0;
    }

    public bool GetRotationSendFlg()
    {
        //撃った時は常に送る
        if (shootFlg) return true;

        //一度以上回転した場合送信する
        if (Mathf.Abs(oldRotation.y - transform.localEulerAngles.y) > 1 || Mathf.Abs(oldRotation.x - transform.localEulerAngles.x) > 1)
        {
            oldRotation = transform.localEulerAngles;
            return true;
        }
        return false;
    }

}
