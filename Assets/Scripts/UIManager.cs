using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject replayPopupPanel;
    public GameObject homePopupPanel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PressRePlayButton()
    {
        replayPopupPanel.gameObject.SetActive(true);
        TextMeshProUGUI info = replayPopupPanel.transform.Find("Popup/Text_Info").GetComponent<TextMeshProUGUI>();
        info.text = $"Would you like to play again?\nYou've played {LoginManager.Instance.currentUser.playCount} times.";
    }
    public void PressHomeButton()
    {
        homePopupPanel.gameObject.SetActive(true);
    }
    public void PressCancelButton()
    {
        replayPopupPanel.gameObject.SetActive(false);
        homePopupPanel.gameObject.SetActive(false);
    }
    public void PressOKButton()
    {
        if (replayPopupPanel.gameObject.activeInHierarchy)
        {

        }
        else if (homePopupPanel.gameObject.activeInHierarchy)
        {
            SceneManager.LoadScene("MainScene");
        }
    }
}
