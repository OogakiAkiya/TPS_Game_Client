using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Vector2 mouse=new Vector2(0,0);
    public GameObject effect;

    private Camera cam;
    private RectTransform imageRect;
    private Canvas canvas;

    //Bullet
    private int count = 0;
    public int interval = 3;

    //Timer
    public float timeOut=0.5f;
    public float timeElapsed=0;

    // Start is called before the first frame update
    void Start()
    {
        cam= transform.FindChild("Camera").gameObject.GetComponent<Camera>();
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        imageRect = GameObject.Find("Canvas").transform.FindChild("Pointer").GetComponent<RectTransform>();
        if (effect) effect.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            //timeElapsed

        }

        //発砲エフェクト表示
        if (Input.GetMouseButtonDown(0))
        {
            effect.SetActive(true);
        }
        if (Input.GetMouseButtonUp(0))
        {
            effect.SetActive(false);
        }

        //発砲
        if (Input.GetMouseButton(0))
        {
            count++;
            if (count > interval)
            {
                //レイの作成
                Ray ray = cam.ScreenPointToRay(GetUIScreenPos(imageRect));
                //レイの可視化
                //Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow,10f);

                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.tag == "users")
                    {
                        hit.collider.GetComponent<Client>().CreateDamageEffect();
                    }
                }

                count = 0;
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
