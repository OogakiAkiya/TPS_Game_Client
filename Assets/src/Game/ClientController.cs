using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ClientController : MonoBehaviour
{
    [SerializeField] GameObject userList;
    [SerializeField] Text ranking;
    GameObject soldierPrefab;
    GameObject maynardPrefab;

    public BaseClient[] clientArray { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        soldierPrefab = (GameObject)Resources.Load("Soldier");
        maynardPrefab = (GameObject)Resources.Load("Monster_U");

        //clientArray = userList.transform.GetComponentsInChildren<BaseClient>();
        UpdateUserArray();
    }

    // Update is called once per frame
    void Update()
    {
        if (!ranking) return;
        //ランキング表示
        Dictionary<string, int> dic = new Dictionary<string, int>();
        foreach (var user in clientArray)
        {
            dic.Add(user.userID, user.killAmount);
        }
        var sorted = dic.OrderByDescending((x) => x.Value);     //降順ソート

        String rankingText="";
        int rank = 1;
        int beforeKillAmount = 0;
        int oldRank = 1;
        foreach (var user in sorted)
        {
            if (beforeKillAmount == user.Value)
            {
                rankingText += oldRank + "位:" + user.Key + ":" + user.Value + "\n";
                rank++;
                continue;
            }
                rankingText += rank + "位:" + user.Key + ":" + user.Value + "\n";
            beforeKillAmount = user.Value;
            oldRank = rank;
            if (++rank > 5) break;
        }
        

        ranking.text = rankingText;

    }

    public BaseClient AddMaynardUser(string _userID,Vector3 _pos)
    {
        var add = Instantiate(maynardPrefab,userList.transform) as GameObject;
        add.transform.position = _pos;
        add.name = _userID;
        BaseClient client = add.GetComponentInChildren<BaseClient>();
        client.userID=_userID;
        //clientArray = userList.transform.GetComponentsInChildren<BaseClient>();
        UpdateUserArray();
        return client;
    }

    public BaseClient AddSoldierUser(string _userID, Vector3 _pos)
    {
        var add = Instantiate(soldierPrefab, userList.transform) as GameObject;
        add.transform.position = _pos;
        add.name = _userID;
        BaseClient client = add.GetComponent<BaseClient>();
        client.userID = _userID;
        //clientArray = userList.transform.GetComponentsInChildren<BaseClient>();
        UpdateUserArray();

        return client;
    }



    public void AddGrenade(string _name,Vector3 _pos,Vector3 _direction)
    {
        GameObject bom = Instantiate(Resources.Load("Bom") as GameObject) as GameObject;
        bom.name = _name;
        bom.transform.position = _pos;
        bom.GetComponent<Grenade>().SetDirection(_direction);
    }

    public void UpdateUserArray()
    {
        List<BaseClient> list = new List<BaseClient>();
        GameObject[] users = GameObject.FindGameObjectsWithTag("users");
        for (int i = 0; i < users.Length; i++)
        {
            if (users[i].activeSelf)
            {
                BaseClient add= users[i].GetComponent<BaseClient>();
                if (!add) continue;
                list.Add(add);
            }
        }
        clientArray = list.ToArray();

    }
}