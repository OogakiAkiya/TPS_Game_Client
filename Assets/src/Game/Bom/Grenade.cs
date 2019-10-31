using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    protected System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
    GameObject explosionPref;

    // Start is called before the first frame update
    void Start()
    {
        timer.Start();
        explosionPref = Resources.Load("Explosion") as GameObject;

    }

    // Update is called once per frame
    void Update()
    {
        if (timer.ElapsedMilliseconds > 5000)
        {
            GameObject explosion = Instantiate(explosionPref) as GameObject;
            explosion.SetActive(true);
            explosion.transform.position = this.transform.position;
            Destroy(this.gameObject);
            timer.Reset();
        }
    }

    public void SetDirection(Vector3 _direction)
    {
        this.GetComponent<Rigidbody>().AddForce(_direction.normalized * 250);
    }
}
