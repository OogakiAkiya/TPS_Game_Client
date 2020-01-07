using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StartCanvasController : MonoBehaviour
{
    [SerializeField] Text text;
    [SerializeField] GameController gameController;
    [SerializeField] Canvas nextCanvas;
    bool flg = false;
    // Start is called before the first frame update
    void Start()
    {
        text.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        text.text = "Are You Ready";
        if (nextCanvas) nextCanvas.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        if (!text) return;
        if (!flg)
        {
            text.transform.localScale += new Vector3(0.01f, 0.01f, 0.01f);
            if (text.transform.localScale.x > 1.2)
            {
                flg = true;
                text.text = "Game Start";
                text.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            }
            return;
        }

        text.transform.localScale += new Vector3(0.03f, 0.03f, 0.03f);
        if (text.transform.localScale.x > 1.2)
        {
            text = null;
            if (gameController) gameController.stopFlg = false;
            if (nextCanvas) nextCanvas.gameObject.SetActive(true);
            gameController.stopFlg = false;
            this.gameObject.SetActive(false);
        }
        return;

    }
}
