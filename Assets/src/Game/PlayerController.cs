using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    private Vector2 mouse=new Vector2(0,0);


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        //視点移動
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");
        //if (System.Math.Abs(mouse.y+y) > 30)y = 0;
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
        sendKey |= InputTemple(KeyCode.LeftShift, Key.SHIFT);
        sendKey |= InputTemple(KeyCode.Space, Key.SPACE);
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0)) sendKey |= Key.LEFT_BUTTON;
        if (sendKey.HasFlag(Key.SPACE))
        {
            return sendKey;

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



}
