using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;


public class ClientController : MonoBehaviour
{

    GameObject userPrefab;
    List<GameObject> userList = new List<GameObject>();
    public GameObject[] objects { get; private set; } = new GameObject[0];
    // Start is called before the first frame update
    void Start()
    {
        userPrefab = (GameObject)Resources.Load("user");


    }

    // Update is called once per frame
    void Update()
    {
    }

    public void AddUser(string _userID)
    {
        var add = Instantiate(userPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        add.name = _userID;
        add.GetComponent<Client>().SetUserID(_userID);
        userList.Add(add);
        objects= GameObject.FindGameObjectsWithTag("users");
    }

}