using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseItem : MonoBehaviour
{
    [SerializeField] public int itemNumber=-999;
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        if (itemNumber == -999)Debug.Log("Item Number Error "+this.name);
#endif

    }

    public void Delete()
    {
        this.gameObject.SetActive(false);
    }
}
