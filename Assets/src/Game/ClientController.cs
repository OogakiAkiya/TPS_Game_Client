using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ClientController : MonoBehaviour
{

    GameObject userPrefab;
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

    public void AddGrenade(string _name,Vector3 _pos,Vector3 _direction)
    {
        GameObject bom = Instantiate(Resources.Load("Bom") as GameObject) as GameObject;
        bom.name = _name;
        bom.transform.position = _pos;
        bom.GetComponent<Grenade>().SetDirection(_direction);
        //bom.transform.position = this.transform.forward + new Vector3(0, 1, 0);
        //bom.transform.rotation = this.transform.rotation;
    }
}