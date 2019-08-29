﻿using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ClientController : MonoBehaviour
{

    GameObject userPrefab;
    //List<GameObject> userGameObjectList = new List<GameObject>();
    //public GameObject[] objects { get; private set; } = new GameObject[0];
    public List<Client> clientList { get; private set; } = new List<Client>();
    public Text ranking;

    // Start is called before the first frame update
    void Start()
    {
        userPrefab = (GameObject)Resources.Load("user");
        clientList.Add(GameObject.FindGameObjectWithTag("Player").GetComponent<Client>());


    }

    // Update is called once per frame
    void Update()
    {

        //ランキング表示
        Dictionary<string, int> dic = new Dictionary<string, int>();
        foreach (var user in clientList)
        {
            dic.Add(user.name, user.killAmount);
        }
        var sorted = dic.OrderByDescending((x) => x.Value);     //降順ソート

        String rankingText="";
        int rank = 1;
        foreach (var user in dic)
        {
            rankingText += rank + "位:" + user.Key + ":" + user.Value + "\n";
            if (++rank > 5) break;
        }
        

        if (ranking) ranking.text = rankingText;

    }

    public void AddUser(string _userID,Vector3 _pos)
    {
        var add = Instantiate(userPrefab, _pos, Quaternion.identity) as GameObject;
        add.name = _userID;
        add.GetComponent<Client>().userID=_userID;
        //userGameObjectList.Add(add);
        //objects= GameObject.FindGameObjectsWithTag("users");
        clientList.Clear();

        clientList.Add(GameObject.FindGameObjectWithTag("Player").GetComponent<Client>());
        foreach(var user in GameObject.FindGameObjectsWithTag("users"))
        {
            clientList.Add(user.GetComponent<Client>());
        }
    }

}