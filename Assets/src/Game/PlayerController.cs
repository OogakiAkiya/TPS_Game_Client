using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float yMin = -60;
    [SerializeField] private float yMax = 30;
    [SerializeField] private Image damageUIEffect;

    private Vector3 oldRotation;
    private Vector2 mouse=new Vector2(0,0);
    private bool shootFlg=false;
    public GameHeader.UserTypeCode userType;
    public BaseClient current;


    // Start is called before the first frame update
    void Start()
    {
        current = this.GetComponentInChildren<BaseClient>();
        if (PlayerPrefs.GetString(SavedData.UserType) == "Soldier") userType = GameHeader.UserTypeCode.SOLDIER;
        if (PlayerPrefs.GetString(SavedData.UserType) == "Maynard") userType = GameHeader.UserTypeCode.MONSTER;

        if (PlayerPrefs.HasKey(SavedData.UserID)) this.name = PlayerPrefs.GetString(SavedData.UserID);


        //userIDセット
        if (current) current.userID = this.name;

        oldRotation = current.transform.localEulerAngles;

        if (damageUIEffect)damageUIEffect.color = new Color(0, 0, 0, 0);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (damageUIEffect)damageUIEffect.color = new Color(0, 0, 0, (100 - current.hp)*0.01f);
        
        //視点移動
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");
        if (mouse.y + y > yMax) y = 0;
        if (mouse.y + y < yMin) y = 0;

        mouse += new Vector2(x, y);
        current.transform.rotation = Quaternion.Euler(new Vector3(-mouse.y, mouse.x,0));
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
        if (Input.GetKeyDown(KeyCode.LeftArrow))sendKey |= Key.LEFT_BUTTON;
        if (Input.GetKeyDown(KeyCode.RightArrow)) sendKey |= Key.RIGHT_BUTTON;
        if (Input.GetKeyDown(KeyCode.E)) sendKey |= Key.RIGHT_CLICK;
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
        {
            sendKey |= Key.LEFT_CLICK;
            shootFlg = !shootFlg;
        }
        if (Input.GetKeyDown(KeyCode.G))sendKey |= Key.G;
        if (Input.GetKeyDown(KeyCode.R)) sendKey |= Key.R;
        return sendKey;
    }

    public Key CheckNowInput()
    {
        Key sendKey = 0;
        if (Input.GetKey(KeyCode.W)) sendKey |= Key.W;
        if (Input.GetKey(KeyCode.S)) sendKey |= Key.S;
        if (Input.GetKey(KeyCode.A)) sendKey |= Key.A;
        if (Input.GetKey(KeyCode.D)) sendKey |= Key.D;
        if (Input.GetKey(KeyCode.LeftShift)) sendKey |= Key.SHIFT;
        if (Input.GetKey(KeyCode.Space)) sendKey |= Key.SPACE;
        if (Input.GetKey(KeyCode.LeftArrow)) sendKey |= Key.LEFT_BUTTON;
        if (Input.GetKey(KeyCode.RightArrow)) sendKey |= Key.RIGHT_BUTTON;
        if (Input.GetMouseButton(0)) sendKey |= Key.LEFT_CLICK;
        if (Input.GetKeyDown(KeyCode.G)) sendKey |= Key.G;
        if (Input.GetKeyDown(KeyCode.R)) sendKey |= Key.R;
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
        if (Mathf.Abs(oldRotation.y - current.transform.localEulerAngles.y) > 1 || Mathf.Abs(oldRotation.x - current.transform.localEulerAngles.x) > 1)
        {
            oldRotation = current.transform.localEulerAngles;
            return true;
        }
        return false;
    }

}
