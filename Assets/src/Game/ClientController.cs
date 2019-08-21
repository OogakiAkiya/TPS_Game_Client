using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;


public class ClientController : MonoBehaviour
{

    GameObject userPrefab;
    List<GameObject> userList = new List<GameObject>();
    //public GameObject[] objects { get; private set; } = new GameObject[0];
    public List<Client> objects { get; private set; } = new List<Client>();

    // Start is called before the first frame update
    void Start()
    {
        userPrefab = (GameObject)Resources.Load("user");
        objects.Add(GameObject.FindGameObjectWithTag("Player").GetComponent<Client>());


    }

    // Update is called once per frame
    void Update()
    {
    }

    public void AddUser(string _userID,Vector3 _pos)
    {
        var add = Instantiate(userPrefab, _pos, Quaternion.identity) as GameObject;
        add.name = _userID;
        add.GetComponent<Client>().userID=_userID;
        userList.Add(add);
        //objects= GameObject.FindGameObjectsWithTag("users");
        objects.Clear();

        objects.Add(GameObject.FindGameObjectWithTag("Player").GetComponent<Client>());
        foreach(var user in GameObject.FindGameObjectsWithTag("users"))
        {
            objects.Add(user.GetComponent<Client>());
        }
    }

}