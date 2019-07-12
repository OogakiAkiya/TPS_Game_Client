using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Vector2 mouse=new Vector2(0,0);

    private Camera cam;
    private RectTransform imageRect;
    private Canvas canvas;

    // Start is called before the first frame update
    void Start()
    {
        cam= transform.FindChild("Camera").gameObject.GetComponent<Camera>();
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        imageRect = GameObject.Find("Canvas").transform.FindChild("Pointer").GetComponent<RectTransform>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            var move = cam.transform.forward * 1.5f;
            cam.transform.position = cam.transform.position + move;

            Ray ray = cam.ScreenPointToRay(GetUIScreenPos(imageRect));

            cam.transform.position=cam.transform.position - move;
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow,5.0f);

            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "Finish") Debug.Log("Hit");
            }
        }

        /*
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        */

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


    Vector2 GetUIScreenPos(RectTransform rt)
    {

        //UIのCanvasに使用されるカメラは Hierarchy 上には表示されないので、
        //変換したいUIが所属しているcanvasを映しているカメラを取得し、 WorldToScreenPoint() で座標変換する
        return RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rt.position);

    }
}
