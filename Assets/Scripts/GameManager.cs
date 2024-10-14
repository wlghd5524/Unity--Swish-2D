using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public Button signInButton;

    public GameObject loginPanel;
    public GameObject rankingPanel;
    public GameObject RankingListItemPrefab;
    private List<GameObject> rankingItems = new List<GameObject>();
    public Transform rankingItemsTransform;

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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        // 씬 로드 이벤트 해제 (중복 등록 방지)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드될 때 호출되는 콜백 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 이름에 따라 다른 함수 호출
        if (scene.name == "MainScene")
        {
            startButton = GameObject.Find("Canvas/StartButton").GetComponent<Button>();
            howToPlayButton = GameObject.Find("Canvas/HowToPlayButton").GetComponent<Button>();
            rankingButton = GameObject.Find("Canvas/RankingButton").GetComponent<Button>();
            quitButton = GameObject.Find("Canvas/QuitButton").GetComponent<Button>();
            loginPanel = GameObject.Find("Canvas").transform.Find("LoginPanel").gameObject;
            signInButton = loginPanel.transform.Find("Popup_Login/Popup/Button_SignIn").GetComponent<Button>();
            rankingPanel = GameObject.Find("Canvas").transform.Find("Popup_Ranking").gameObject;
            rankingItemsTransform = rankingPanel.transform.Find("Popup/ScrollRect/Content");
            startButton.onClick.AddListener(OpenLoginPopup);
            howToPlayButton.onClick.AddListener(OpenHowToPlay);
            rankingButton.onClick.AddListener(() => StartCoroutine(OpenRanking()));
            quitButton.onClick.AddListener(QuitGame);
            signInButton.onClick.AddListener(() => LoginManager.Instance.StartCoroutine(LoginManager.Instance.ValidateSignIn()));
        }
        else if (scene.name == "GameScene")
        {
            ScoreManager.Instance.currentScore = 0;
        }
    }

    void OpenPopup()
    {

    }
    void OpenLoginPopup()
    {
        loginPanel.SetActive(true);
        LoginManager.Instance.UpdateInputFieldForRememberedUser();
    }

    public void CloseLoginPopup() { loginPanel.SetActive(false); }

    public void GoToGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    void OpenHowToPlay()
    {

    }
    void CloseHowToPlay() { }


    IEnumerator OpenRanking()
    {
        yield return WebConnector.Instance.StartCoroutine(WebConnector.Instance.UpdateUsers());  // 서버에서 모든 유저 목록 불러오기
        GameObject cansvas = GameObject.Find("Canvas");
        Transform rankingPanelTransform = cansvas.transform.Find("Popup_Ranking");
        rankingPanel = rankingPanelTransform.gameObject;
        rankingItemsTransform = rankingPanel.transform.Find("Popup/ScrollRect/Content");
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            GameObject myRankingItem = rankingPanel.transform.Find("Popup/Ranking_Me").gameObject;
            TextMeshProUGUI rankText = myRankingItem.transform.Find("Text_Rank").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nameText = myRankingItem.transform.Find("Text_UserName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI userNumberText = nameText.transform.Find("Group_Text/Text_UserNumber").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = nameText.transform.Find("Group_Text/Text_Score").GetComponent<TextMeshProUGUI>();
            int rank = User.users.IndexOf(LoginManager.Instance.currentUser);
            if (rank == 0)
            {
                myRankingItem.transform.Find("Icon_Medal_Gold").gameObject.SetActive(true);
            }
            else if (rank == 1)
            {
                myRankingItem.transform.Find("Icon_Medal_Silver").gameObject.SetActive(true);
            }
            else if (rank == 2)
            {
                myRankingItem.transform.Find("Icon_Medal_Bronze").gameObject.SetActive(true);
            }
            else
            {
                rankText.text = (rank + 1).ToString();
            }
            nameText.text = LoginManager.Instance.currentUser.name;
            userNumberText.text = LoginManager.Instance.currentUser.userNumber;
            scoreText.text = LoginManager.Instance.currentUser.score.ToString();
        }

        // 기존에 생성된 랭킹 아이템들 삭제
        foreach (Transform item in rankingItemsTransform)
        {
            Destroy(item.gameObject);
        }
        rankingItems.Clear();

        // User.users 리스트를 기반으로 프리팹 생성
        for (int i = 0; i < User.users.Count; i++)
        {
            CreateRankingItem(User.users[i], i);
        }
        rankingPanel.SetActive(true);
    }

    public void CloseRanking()
    {
        rankingPanel.SetActive(false);
    }
    void QuitGame()
    {

    }
    // 랭킹 아이템 생성 함수
    private void CreateRankingItem(User user, int index)
    {
        // Resources 폴더에서 프리팹 로드 (만약 Inspector에서 연결하지 않는 경우)
        if (RankingListItemPrefab == null)
        {
            RankingListItemPrefab = Resources.Load<GameObject>("Prefabs/RankingItemPrefab");
        }

        // 프리팹 인스턴스화
        GameObject rankingItem = Instantiate(RankingListItemPrefab, rankingPanel.transform.Find("Popup/ScrollRect/Content"));

        TextMeshProUGUI rankText = rankingItem.transform.Find("Text_Rank").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI nameText = rankingItem.transform.Find("Text_UserName").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI userNumberText = nameText.transform.Find("Group_Text/Text_UserNumber").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI scoreText = nameText.transform.Find("Group_Text/Text_Score").GetComponent<TextMeshProUGUI>();
        if (index == 0)
        {
            rankingItem.transform.Find("Icon_Medal_Gold").gameObject.SetActive(true);
        }
        else if (index == 1)
        {
            rankingItem.transform.Find("Icon_Medal_Silver").gameObject.SetActive(true);
        }
        else if (index == 2)
        {
            rankingItem.transform.Find("Icon_Medal_Bronze").gameObject.SetActive(true);
        }
        else
        {
            rankText.text = (index + 1).ToString();
        }
        if (nameText != null) nameText.text = user.name;
        if (userNumberText != null) userNumberText.text = user.userNumber;
        if (scoreText != null) scoreText.text = user.score.ToString();


        // 리스트에 추가하여 관리
        rankingItems.Add(rankingItem);
    }
    public void GameOver()
    {
        LoginManager.Instance.currentUser.score = ScoreManager.Instance.currentScore;
        // Register 코루틴이 완료된 후 OpenRanking 실행
        StartCoroutine(SubmitDataAndOpenRanking());
    }

    IEnumerator SubmitDataAndOpenRanking()
    {
        // Register 코루틴 실행 및 완료 대기
        yield return WebConnector.Instance.StartCoroutine(WebConnector.Instance.SendUserDataToServer(LoginManager.Instance.currentUser));

        // Register가 완료되면 OpenRanking 실행
        yield return StartCoroutine(OpenRanking());
    }
}
