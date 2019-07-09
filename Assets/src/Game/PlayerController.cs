using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        /*
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        */
        
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");
        if (System.Math.Abs(mouse.y+y) > 30)y = 0;
        mouse += new Vector2(x, y);


        this.transform.rotation = Quaternion.Euler(new Vector3(-mouse.y, mouse.x,0));
    }
}
