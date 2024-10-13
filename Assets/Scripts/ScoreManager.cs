using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; } // 싱글톤 인스턴스
    public TextMeshProUGUI scoreText;
    public int currentScore;
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
    }

    // Start is called before the first frame update
    void Start()
    {
        rankingItemsTransform = rankingPanel.transform.Find("Popup/ScrollRect/Content");
        currentScore = 0;
        UpdateScoreText();
    }

    public void OpenRankingPopup()
    {
        WebConnector.Instance.UpdateUsers();  // 서버에서 모든 유저 목록 불러오기
        rankingPanel.SetActive(true);
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
            rankText.text = index.ToString();
        }
        if (nameText != null) nameText.text = user.Name;
        if (userNumberText != null) userNumberText.text = user.UserNumber;
        if (scoreText != null) scoreText.text = user.Score.ToString();


        // 리스트에 추가하여 관리
        rankingItems.Add(rankingItem);
    }
    // 점수 업데이트 함수
    public void AddScore(int value)
    {
        currentScore += value;
        UpdateScoreText();
    }

    public void GameOver()
    {
        LoginManager.Instance.currentUser.Score = currentScore;
        WebConnector.Instance.Register(LoginManager.Instance.currentUser); //서버로 유저 데이터 보내기
        OpenRankingPopup();
    }

    // 점수 UI 업데이트 함수
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }
}