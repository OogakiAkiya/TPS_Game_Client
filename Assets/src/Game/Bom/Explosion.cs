using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] long deleteTime = 2000;
    bool explosionFlg = false;
    protected System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

    // Start is called before the first frame update
    void Start()
    {
        timer.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (timer.ElapsedMilliseconds>deleteTime) Destroy(this.gameObject);
    }

}
