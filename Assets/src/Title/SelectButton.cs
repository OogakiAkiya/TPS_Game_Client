using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SelectButton : MonoBehaviour
{
    public Color onColor=Color.blue;
    public Color notOnColor = Color.white;

    Image image;
    private void Start()
    {
        image = this.GetComponent<Image>();
        PlayerPrefs.SetString(SavedData.UserType, "Soldier");

    }
    public void OnClick()
    {
        image.color = onColor;
        PlayerPrefs.SetString(SavedData.UserType, this.GetComponentInChildren<Text>().text);

    }
    public void NotClick()
    {
        image.color = notOnColor;
    }
}
