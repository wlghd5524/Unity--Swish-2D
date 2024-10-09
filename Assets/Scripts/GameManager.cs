using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Button startButton;
    public Button howToPlayButton;
    public Button rankingButton;
    public Button quitButton;
    void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        startButton.onClick.AddListener(GoToGameScene);
        howToPlayButton.onClick.AddListener(OpenHowToPlay);
        rankingButton.onClick.AddListener(OpenRanking);
        quitButton.onClick.AddListener(QuitGame);
    }

    void GoToGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    void OpenHowToPlay()
    {

    }

    void OpenRanking()
    {

    }
    void QuitGame()
    {

    }
}
