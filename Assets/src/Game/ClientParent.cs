using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientParent : MonoBehaviour
{
    public AudioSource audioSource;
    
    private void Start()
    {
        audioSource.GetComponent<AudioSource>();
        this.transform.position = Vector3.zero;
    }

    private void Update()
    {
        
    }

    public GameObject ChangeModel(string _modelName)
    {
        Transform client=this.transform.Find(_modelName);

        client.gameObject.SetActive(true);
        this.transform.position = Vector3.zero;

        return client.gameObject;
    }

    public GameObject ChangeModel(MonsterType _type)
    {
        if (_type == MonsterType.MAYNARD)
        {
            return ChangeModel("Maynard");
        }
        if (_type == MonsterType.MUTANT)
        {
            return ChangeModel("Mutant");
        }
        return null;
    }


}
