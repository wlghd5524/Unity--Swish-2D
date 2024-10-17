using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public GameObject replayPopupPanel;
    public GameObject homePopupPanel;
    public GameObject itemUI;

    public List<GameObject> goalEffect = new List<GameObject>();
    //public Slider itemSlider;

    private void Awake()
    {
        Instance = this;
    }
    void OnEnable()
    {
        itemUI = GameObject.Find("Canvas/Item");
        itemUI.SetActive(false);
        //itemSlider = itemUI.GetComponentInChildren<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (itemSlider.IsActive() && itemSlider.value == 0)
        //{
        //    itemUI.SetActive(false);
        //    ItemManager.Instance.currentItemState = ItemState.Normal;
        //}
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
