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

    }

    // Update is called once per frame
    void Update()
    {

    }

}
