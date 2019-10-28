﻿using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ClientController : MonoBehaviour
{
    [SerializeField] GameObject userList;
    [SerializeField] Text ranking;
    GameObject userPrefab;
    public Client[] clientArray { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        userPrefab = (GameObject)Resources.Load("user");
        clientArray = userList.transform.GetComponentsInChildren<Client>();

    }

    // Update is called once per frame
    void Update()
    {

        //ランキング表示
        Dictionary<string, int> dic = new Dictionary<string, int>();
        foreach (var user in clientArray)
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

    public Client AddUser(string _userID,Vector3 _pos)
    {
        var add = Instantiate(userPrefab,userList.transform) as GameObject;
        add.transform.position = _pos;
        add.name = _userID;
        Client client = add.GetComponent<Client>();
        client.userID=_userID;
        clientArray = userList.transform.GetComponentsInChildren<Client>();
        return client;
    }

    public void AddGrenade(string _name,Vector3 _pos,Vector3 _direction)
    {
        GameObject bom = Instantiate(Resources.Load("Bom") as GameObject) as GameObject;
        bom.name = _name;
        bom.transform.position = _pos;
        bom.GetComponent<Grenade>().SetDirection(_direction);
    }
}