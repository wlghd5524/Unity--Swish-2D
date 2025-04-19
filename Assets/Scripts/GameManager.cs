using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject canvas;
    public Button startButton;
    public Button howToPlayButton;
    public Button rankingButton;
    public Button quitButton;
    public GameObject quitPopup;
    public Button quitCancelButton;
    public Button quitConfirmButton;
    public Button logOutButton;

    public GameObject loginFailedPopup;
    public Button loginPopupCloseButton;
    public GameObject rankingPopup;
    public GameObject RankingListItemPrefab;
    private List<GameObject> rankingItems = new List<GameObject>();
    public Transform rankingItemsTransform;
    public Button rankingCloseButton;
    public GameObject howToPlayPopup;
    public Button howToPlayCloseButton;
    public Button returnPage;
    public Button nextPage;
    public GameObject logOutPopup;
    public Button logOutCloseButton;
    List<GameObject> pages = new List<GameObject>();

    void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        QualitySettings.vSyncCount = 0;  // V-Sync 비활성화
        Application.targetFrameRate = 60;  // 프레임 제한 설정
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
            canvas = GameObject.Find("Canvas");
            startButton = GameObject.Find("Canvas/StartButton").GetComponent<Button>();
            howToPlayButton = GameObject.Find("Canvas/HowToPlayButton").GetComponent<Button>();
            rankingButton = GameObject.Find("Canvas/RankingButton").GetComponent<Button>();
            quitButton = GameObject.Find("Canvas/QuitButton").GetComponent<Button>();
            logOutButton = GameObject.Find("Canvas/LogoutButton").GetComponent<Button>();

            quitPopup = canvas.transform.Find("QuitButtonPopup").gameObject;
            quitPopup.SetActive(false);

            logOutPopup = canvas.transform.Find("LogoutPopup").gameObject;
            logOutPopup.SetActive(false);



            howToPlayPopup = canvas.transform.Find("HowToPlayPopup").gameObject;
            howToPlayCloseButton = howToPlayPopup.transform.Find("Button_Close").GetComponent<Button>();
            returnPage = howToPlayPopup.transform.Find("Button_ReturnPage").GetComponent<Button>();
            nextPage = howToPlayPopup.transform.Find("Button_NextPage").GetComponent<Button>();
            pages.Clear();
            foreach (Transform page in howToPlayPopup.transform.Find("Content"))
            {
                pages.Add(page.gameObject);
            }
            loginFailedPopup = canvas.transform.Find("LoginFailedPopup").gameObject;
            loginFailedPopup.SetActive(false);

            loginPopupCloseButton = loginFailedPopup.transform.Find("Button_Close").GetComponent<Button>();
            loginPopupCloseButton.onClick.AddListener(() => ClosePopup(loginFailedPopup));
            rankingPopup = canvas.transform.Find("Popup_Ranking").gameObject;
            rankingItemsTransform = rankingPopup.transform.Find("Popup/ScrollRect/Content");
            startButton.onClick.AddListener(() => StartCoroutine(StartGame()));
            howToPlayButton.onClick.AddListener(OpenHowToPlay);
            rankingButton.onClick.AddListener(() => StartCoroutine(OpenRanking()));
            quitButton.onClick.AddListener(QuitGame);
            logOutButton.onClick.AddListener(FirebaseManager.Instance.SignOut);
            //signInButton.onClick.AddListener(() => LoginManager.Instance.StartCoroutine(LoginManager.Instance.ValidateSignIn()));
        }
        else if (scene.name == "GameScene")
        {
            ScoreManager.Instance.currentScore = 0;
        }
    }

    public IEnumerator StartGame()
    {
        if (FirebaseManager.Instance.GetCurrentUser() == null)
        {
            yield return FirebaseManager.Instance.StartCoroutine(FirebaseManager.Instance.GoogleLogin(LoginSuccess =>
            {
                if (LoginSuccess)
                {
                    SceneManager.LoadScene("GameScene");
                }
                else
                {
                    FirebaseManager.Instance.SignOut();
                    loginFailedPopup.SetActive(true);
                }
            }));
        }
        else
        {
            SceneManager.LoadScene("GameScene");
        }
    }

    public void ClosePopup(GameObject popup)
    {
        popup.SetActive(false);
    }

    int pageIdex = 0;
    void OpenHowToPlay()
    {
        pageIdex = 0;
        pages.ForEach(page => page.SetActive(false));
        howToPlayCloseButton.onClick.AddListener(() => ClosePopup(howToPlayPopup));

        howToPlayPopup.SetActive(true);
        pages[pageIdex].SetActive(true);
        returnPage.onClick.AddListener(PressReturnPage);
        nextPage.onClick.AddListener(PressNextPage);
    }

    void PressReturnPage()
    {
        if (pageIdex == 0)
        {
            return;
        }
        pages[pageIdex--].SetActive(false);
        pages[pageIdex].SetActive(true);
    }

    void PressNextPage()
    {
        if (pageIdex == pages.Count - 1)
        {
            return;
        }
        pages[pageIdex++].SetActive(false);
        pages[pageIdex].SetActive(true);
    }

    Color iconFrameColor;
    int iconIndex;

    IEnumerator OpenRanking()
    {
        bool loginSuccessful = false;
        if (FirebaseManager.Instance.GetCurrentUser() == null)
        {

            yield return StartCoroutine(FirebaseManager.Instance.GoogleLogin(loginSuccess =>
            {
                loginSuccessful = loginSuccess;
                if (!loginSuccess)
                {
                    FirebaseManager.Instance.SignOut();
                    loginFailedPopup.SetActive(true);
                }

            }));

            // 로그인 실패 시 여기서 코루틴 종료
            if (!loginSuccessful)
            {
                yield break;
            }
        }
        if (loginSuccessful && FirebaseManager.Instance.currentUser == null)
        {
            FirebaseManager.Instance.currentUser = new User(
                FirebaseManager.Instance.GetCurrentUser().UserId,
                FirebaseManager.Instance.GetCurrentUser().Email,
                FirebaseManager.Instance.GetCurrentUser().DisplayName,
                FirebaseManager.Instance.GetCurrentUser().PhotoUrl.ToString(),
                0,
                ""
            );
            yield return FirebaseManager.Instance.SaveUser(0);
        }
        //랭킹 유저 로드
        yield return StartCoroutine(LoadUsersForRanking());

        canvas = GameObject.Find("Canvas");
        Transform rankingPanelTransform = canvas.transform.Find("Popup_Ranking");
        rankingPopup = rankingPanelTransform.gameObject;
        rankingCloseButton = rankingPopup.transform.Find("Popup/Popup_TopBar/Button_Close").GetComponent<Button>();
        rankingCloseButton.onClick.AddListener(() => ClosePopup(rankingPopup));
        rankingItemsTransform = rankingPopup.transform.Find("Popup/ScrollRect/Content");

        GameObject myRankingItem = rankingPopup.transform.Find("Popup/Ranking_Me").gameObject;
        TextMeshProUGUI rankText = myRankingItem.transform.Find("Text_Rank").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI nameText = myRankingItem.transform.Find("Text_UserName").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI scoreText = nameText.transform.Find("Group_Text/Text_Score").GetComponent<TextMeshProUGUI>();
        Image iconFrame = myRankingItem.transform.Find("IconFrame").GetComponent<Image>();

        // 프로필 이미지 로드 (photoUrl이 있을 경우)
        if (!string.IsNullOrEmpty(FirebaseManager.Instance.currentUser.photoUrl))
        {
            // FirebaseManager의 이미지 로드 메서드 사용
            StartCoroutine(FirebaseManager.Instance.LoadImage(FirebaseManager.Instance.currentUser.photoUrl, iconFrame));
            // 아이콘 프레임 내부의 모든 아이콘 숨기기
            for (int i = 0; i < iconFrame.transform.childCount; i++)
            {
                iconFrame.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else
        {
            // photoUrl이 없을 경우 기존 랜덤 아이콘 표시
            iconFrameColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            iconFrame.color = iconFrameColor;
            iconIndex = UnityEngine.Random.Range(0, 5);
            iconFrame.transform.GetChild(iconIndex).gameObject.SetActive(true);
        }

        // 내 랭킹 위치 찾기
        int rank = User.users.FindIndex(u => u.uid == FirebaseManager.Instance.currentUser.uid) + 1;

        // 랭킹에 따른 메달 표시 설정
        myRankingItem.transform.Find("Icon_Medal_Gold").gameObject.SetActive(false);
        myRankingItem.transform.Find("Icon_Medal_Silver").gameObject.SetActive(false);
        myRankingItem.transform.Find("Icon_Medal_Bronze").gameObject.SetActive(false);

        if (rank == 1)
        {
            myRankingItem.transform.Find("Icon_Medal_Gold").gameObject.SetActive(true);
            rankText.text = "";
        }
        else if (rank == 2)
        {
            myRankingItem.transform.Find("Icon_Medal_Silver").gameObject.SetActive(true);
            rankText.text = "";
        }
        else if (rank == 3)
        {
            myRankingItem.transform.Find("Icon_Medal_Bronze").gameObject.SetActive(true);
            rankText.text = "";
        }
        else
        {
            rankText.text = rank.ToString();
        }

        nameText.text = $"{FirebaseManager.Instance.currentUser.displayName}";
        scoreText.text = FirebaseManager.Instance.currentUser.score.ToString();



        // 기존에 생성된 랭킹 아이템들 삭제
        foreach (Transform item in rankingItemsTransform)
        {
            Destroy(item.gameObject);
        }
        rankingItems.Clear();

        // 상위 20명의 유저만 표시
        int maxRankingCount = Mathf.Min(20, User.users.Count);
        for (int i = 0; i < maxRankingCount; i++)
        {
            CreateRankingItem(User.users[i], i);
        }
        rankingPopup.SetActive(true);
    }

    void QuitGame()
    {
        quitPopup.SetActive(true);
        quitCancelButton = quitPopup.transform.Find("Popup/Button_Cancel").GetComponent<Button>();
        quitCancelButton.onClick.AddListener(() => ClosePopup(quitPopup));
        quitConfirmButton = quitPopup.transform.Find("Popup/Button_Ok").GetComponent<Button>();
        quitConfirmButton.onClick.AddListener(PressConfirmButton);
    }


    void PressConfirmButton()
    {
        //게임 종료
        Application.Quit();
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
        GameObject rankingItem = Instantiate(RankingListItemPrefab, rankingPopup.transform.Find("Popup/ScrollRect/Content"));

        TextMeshProUGUI rankText = rankingItem.transform.Find("Text_Rank").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI nameText = rankingItem.transform.Find("Text_UserName").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI scoreText = nameText.transform.Find("Group_Text/Text_Score").GetComponent<TextMeshProUGUI>();
        Image iconFrame = rankingItem.transform.Find("IconFrame").GetComponent<Image>();

        // 프로필 이미지 로드 (photoUrl이 있을 경우)
        if (!string.IsNullOrEmpty(user.photoUrl))
        {
            // FirebaseManager의 이미지 로드 메서드 사용
            StartCoroutine(FirebaseManager.Instance.LoadImage(user.photoUrl, iconFrame));
            // 아이콘 프레임 내부의 모든 아이콘 숨기기
            for (int i = 0; i < iconFrame.transform.childCount; i++)
            {
                iconFrame.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else
        {
            // 그 외 사용자는 랜덤 아이콘 사용
            iconFrame.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            iconFrame.transform.GetChild(UnityEngine.Random.Range(0, 5)).gameObject.SetActive(true);
        }

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
        if (nameText != null) nameText.text = $"{user.displayName}";
        if (scoreText != null) scoreText.text = user.score.ToString();

        // 리스트에 추가하여 관리
        rankingItems.Add(rankingItem);
    }

    public IEnumerator GameOver()
    {
        if (ScoreManager.Instance.currentScore > FirebaseManager.Instance.currentUser.score)
        {
            FirebaseManager.Instance.currentUser.score = ScoreManager.Instance.currentScore;
            yield return FirebaseManager.Instance.SaveUser(ScoreManager.Instance.currentScore);
        }
        yield return OpenRanking();
    }

    // 랭킹을 위한 유저 정보 로드 코루틴
    private IEnumerator LoadUsersForRanking()
    {
        bool topUsersLoaded = false;
        bool currentUserLoaded = false;
        User currentUserData = null;
        string currentUserId = FirebaseManager.Instance.GetCurrentUser().UserId;
        bool isCurrentUserInTop20 = false;

        // 상위 20명의 유저 데이터 가져오기
        FirebaseManager.Instance.GetTopUsers(20, users =>
        {
            topUsersLoaded = true;
            Debug.Log($"랭킹을 위해 상위 {users.Count}명의 유저 정보를 로드했습니다.");

            // 현재 유저가 상위 20명에 포함되어 있는지 확인
            isCurrentUserInTop20 = User.users.Any(u => u.uid == currentUserId);

            // 리스트에 현재 로그인된 유저가 있다면 FirebaseManager.Instance.currentUser에 설정
            if (isCurrentUserInTop20)
            {
                User currentUserInList = User.users.Find(u => u.uid == currentUserId);
                if (currentUserInList != null)
                {
                    FirebaseManager.Instance.currentUser = currentUserInList;
                    Debug.Log("상위 랭킹 리스트에서 현재 유저 정보를 설정했습니다.");
                }
            }
        });

        // 현재 유저가 상위 20명에 없는 경우에만 개별적으로 정보 가져오기
        yield return new WaitUntil(() => topUsersLoaded);

        if (!isCurrentUserInTop20)
        {
            FirebaseManager.Instance.GetUserData(currentUserId, user =>
            {
                if (user != null)
                {
                    currentUserData = user;
                    FirebaseManager.Instance.currentUser = user;
                    currentUserLoaded = true;
                    Debug.Log("현재 유저 정보를 추가로 로드했습니다.");
                }
                else
                {
                    Debug.LogError("현재 유저 정보를 가져오는데 실패했습니다.");
                    currentUserLoaded = true; // 실패했지만 완료 처리
                }
            });

            yield return new WaitUntil(() => currentUserLoaded);

            if (currentUserData != null)
            {
                // 현재 유저가 상위 20명에 포함되지 않았을 때만 추가
                bool alreadyExists = User.users.Any(u => u.uid == currentUserData.uid);
                if (!alreadyExists)
                {
                    User.users.Add(currentUserData);
                    Debug.Log("현재 유저 정보를 유저 목록에 추가했습니다.");
                }
            }
        }

        // User.users 리스트가 점수 내림차순으로 정렬되고, 점수가 같으면 타임스탬프가 빠른 순으로 정렬
        User.users = User.users
            .OrderByDescending(u => u.score)
            .ThenBy(u => u.timestamp) // 타임스탬프가 빠른 순서대로 정렬
            .ToList();
    }
}
