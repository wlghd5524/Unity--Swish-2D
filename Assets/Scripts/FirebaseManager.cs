using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Google;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }
    public string GoogleAPI = "67749808151-ufrk524r7ve5mmp11ohj07epiehss5lu.apps.googleusercontent.com";
    private GoogleSignInConfiguration configuration;

    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;
    private FirebaseFirestore db; // Firestore 인스턴스

    public TextMeshProUGUI Username, UserEmail;
    public Button loginButton;

    public User currentUser;

    public Image UserProfilePic;
    private string imageUrl;
    private bool isGoogleSignInInitialized = false;
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
    }
    private void Start()
    {
        InitFirebase();
    }

    void InitFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        // Firestore 초기화
        db = FirebaseFirestore.DefaultInstance;
    }

    public IEnumerator GoogleLogin(System.Action<bool> callback)
    {
        if (!isGoogleSignInInitialized)
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = GoogleAPI,
                RequestEmail = true
            };

            isGoogleSignInInitialized = true;
        }
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            WebClientId = GoogleAPI
        };
        GoogleSignIn.Configuration.RequestEmail = true;

        Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();

        bool isComplete = false;
        bool isSuccess = false;

        signIn.ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.Log("Cancelled");
                isComplete = true;
            }
            else if (task.IsFaulted)
            {
                Debug.Log("Faulted " + task.Exception);
                isComplete = true;
            }
            else
            {
                Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(((Task<GoogleSignInUser>)task).Result.IdToken, null);
                auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
                {
                    if (authTask.IsCanceled)
                    {
                        Debug.Log("Firebase Auth Cancelled");
                        isComplete = true;
                    }
                    else if (authTask.IsFaulted)
                    {
                        Debug.Log("Faulted In Auth " + authTask.Exception);
                        isComplete = true;
                    }
                    else
                    {
                        Debug.Log("Success");
                        user = auth.CurrentUser;
                        GetUserData(user.UserId, user =>
                        {
                            currentUser = user;
                            isSuccess = true;
                            isComplete = true;
                        });
                    }
                });
            }
        });
        yield return new WaitUntil(() => isComplete);
        callback(isSuccess);
    }

    // Firestore에 사용자 정보 저장
    public IEnumerator SaveUser(int score)
    {
        if (user != null)
        {
            // 작업 완료 플래그
            bool isComplete = false;

            // 사용자 정보를 담은 딕셔너리 생성
            Dictionary<string, object> userData = new Dictionary<string, object>
            {
                { "uid", user.UserId },
                { "email", user.Email },
                { "displayName", user.DisplayName },
                { "photoUrl", user.PhotoUrl != null ? user.PhotoUrl.ToString() : "" },
                { "score", score },
                { "playTime", Timestamp.FromDateTime(System.DateTime.UtcNow) }
            };

            // 사용자 컬렉션에 저장 (사용자 ID를 문서 ID로 사용)
            DocumentReference userRef = db.Collection("users").Document(user.UserId);

            // 문서가 존재하는지 확인
            userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DocumentSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        // 문서가 존재하면 score와 playtime 모두 업데이트
                        Dictionary<string, object> updates = new Dictionary<string, object>
                        {
                            { "score", score },
                            { "playTime", Timestamp.FromDateTime(System.DateTime.UtcNow) }
                        };

                        userRef.UpdateAsync(updates)
                            .ContinueWithOnMainThread(updateTask =>
                            {
                                if (updateTask.IsCompleted)
                                {
                                    Debug.Log("User score and last login updated");
                                }
                                else
                                {
                                    Debug.LogError("Error updating user: " + updateTask.Exception);
                                }
                                isComplete = true;
                            });
                    }
                    else
                    {
                        // 문서가 존재하지 않으면 새로 생성
                        userRef.SetAsync(userData).ContinueWithOnMainThread(setTask =>
                        {
                            if (setTask.IsCompleted)
                            {
                                Debug.Log("New user added to Firestore");
                            }
                            else
                            {
                                Debug.LogError("Error adding user: " + setTask.Exception);
                            }
                            isComplete = true;
                        });
                    }
                }
                else
                {
                    Debug.LogError("Error checking user existence: " + task.Exception);
                    isComplete = true;
                }
            });

            // 작업이 완료될 때까지 대기
            yield return new WaitUntil(() => isComplete);
            Debug.Log("SaveUser completed");
        }
        else
        {
            Debug.LogWarning("SaveUser called but user is null");
            yield return null;
        }
    }

    // Firestore에서 사용자 데이터 가져오기
    public void GetUserData(string userId, System.Action<User> callback)
    {
        if (db == null || string.IsNullOrEmpty(userId))
        {
            callback(null);
            return;
        }

        DocumentReference userRef = db.Collection("users").Document(userId);
        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Dictionary<string, object> userData = snapshot.ToDictionary();
                    try
                    {
                        // 필드 존재 여부 확인 및 기본값 사용
                        string uid = userData.ContainsKey("uid") ? userData["uid"].ToString() : userId;
                        string email = userData.ContainsKey("email") ? userData["email"].ToString() : "";
                        string displayName = userData.ContainsKey("displayName") ? userData["displayName"].ToString() : "Unknown";
                        string photoUrl = userData.ContainsKey("photoUrl") ? userData["photoUrl"].ToString() : "";
                        int score = 0;
                        if (userData.ContainsKey("score"))
                        {
                            int.TryParse(userData["score"].ToString(), out score);
                        }
                        string timestamp = userData.ContainsKey("timestamp") ? userData["timestamp"].ToString() : "";

                        User user = new User(uid, email, displayName, photoUrl, score, timestamp);
                        callback(user);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error creating User object: {e.Message}");
                        callback(null);
                    }
                }
                else
                {
                    Debug.Log("User does not exist in Firestore");
                    callback(null);
                }
            }
            else
            {
                Debug.LogError("Error getting user data: " + (task.Exception != null ? task.Exception.Message : "Unknown error"));
                callback(null);
            }
        });
    }

    // 현재 로그인된 사용자 가져오기
    public Firebase.Auth.FirebaseUser GetCurrentUser()
    {
        return user;
    }

    // 로그아웃
    public void SignOut()
    {
        bool firebaseSignedOut = false;

        // Firebase 로그아웃 시도
        if (auth != null && auth.CurrentUser != null)
        {
            try
            {
                auth.SignOut();
                user = null;
                firebaseSignedOut = true;
                Debug.Log("Firebase signed out");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Firebase sign out error: " + e.Message);
            }
        }

        // Google 로그아웃 시도 (Firebase 로그인 여부와 무관하게)
        try
        {
            GoogleSignIn.DefaultInstance.SignOut();
            Debug.Log("Google signed out");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Google sign out error: " + e.Message);
        }

        // 공통 정리 작업
        currentUser = null;

        // 로그아웃 팝업 표시 (적어도 하나는 성공했을 경우)
        if (firebaseSignedOut)
        {
            GameManager.Instance.logOutPopup.SetActive(true);
            GameManager.Instance.logOutCloseButton = GameManager.Instance.logOutPopup.transform.Find("Button_Close").GetComponent<Button>();
            GameManager.Instance.logOutCloseButton.onClick.AddListener(() => GameManager.Instance.ClosePopup(GameManager.Instance.logOutPopup));
        }
        else
        {
            Debug.LogWarning("No active sessions to sign out from");
        }
    }

    // 이미지 로드를 위한 통합 메서드
    public IEnumerator LoadImage(string imageUrl, Image targetImage = null, bool useDefaultIcon = true)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            Debug.LogWarning("이미지 URL이 비어있습니다.");
            if (useDefaultIcon && targetImage != null)
            {
                SetDefaultRandomIcon(targetImage);
            }
            yield break;
        }

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // 대상 이미지가 지정되지 않은 경우 기본 프로필 이미지에 적용
            if (targetImage == null && UserProfilePic != null)
            {
                UserProfilePic.sprite = sprite;
                UserProfilePic.color = Color.white;
                Debug.Log("기본 프로필 이미지 로드 성공: " + imageUrl);
            }
            // 대상 이미지가 지정된 경우
            else if (targetImage != null)
            {
                targetImage.sprite = sprite;
                targetImage.color = Color.white;
                Debug.Log("이미지 로드 성공: " + imageUrl);
            }
        }
        else
        {
            Debug.LogError("이미지 로드 실패: " + www.error);
            if (useDefaultIcon && targetImage != null)
            {
                SetDefaultRandomIcon(targetImage);
            }
        }
    }

    // 기본 랜덤 아이콘 설정
    private void SetDefaultRandomIcon(Image targetImage)
    {
        // 이미지가 이미 있는지 확인
        if (targetImage != null)
        {
            targetImage.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);

            // 자식 오브젝트가 있는지 확인 (아이콘)
            if (targetImage.transform.childCount > 0)
            {
                // 먼저 모든 자식을 비활성화
                for (int i = 0; i < targetImage.transform.childCount; i++)
                {
                    targetImage.transform.GetChild(i).gameObject.SetActive(false);
                }

                // 랜덤 아이콘 활성화
                int randomIcon = UnityEngine.Random.Range(0, targetImage.transform.childCount);
                targetImage.transform.GetChild(randomIcon).gameObject.SetActive(true);
            }
        }
    }

    // 상위 N명의 사용자 데이터 가져오기
    public void GetTopUsers(int limit, System.Action<List<User>> callback)
    {
        if (db == null)
        {
            Debug.LogError("Firestore not initialized");
            callback(new List<User>());
            return;
        }

        CollectionReference usersRef = db.Collection("users");
        // 점수 내림차순으로 정렬하고 limit 개수만큼만 가져오기
        usersRef.OrderByDescending("score").Limit(limit).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                QuerySnapshot querySnapshot = task.Result;
                List<User> users = new List<User>();

                foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
                {
                    Dictionary<string, object> userData = documentSnapshot.ToDictionary();
                    try
                    {
                        // 필드 존재 여부 확인 및 기본값 사용
                        string uid = userData.ContainsKey("uid") ? userData["uid"].ToString() : "";
                        string email = userData.ContainsKey("email") ? userData["email"].ToString() : "";
                        string displayName = userData.ContainsKey("displayName") ? userData["displayName"].ToString() : "Unknown";
                        string photoUrl = userData.ContainsKey("photoUrl") ? userData["photoUrl"].ToString() : "";
                        int score = 0;
                        if (userData.ContainsKey("score"))
                        {
                            int.TryParse(userData["score"].ToString(), out score);
                        }
                        string timestamp = userData.ContainsKey("timestamp") ? userData["timestamp"].ToString() : "";

                        User newUser = new User(uid, email, displayName, photoUrl, score, timestamp);
                        users.Add(newUser);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error creating User object: {e.Message}");
                    }
                }

                Debug.Log($"Found {users.Count} top users.");

                // User.users 리스트에 데이터 저장
                User.users.Clear(); // 기존 목록 초기화
                User.users.AddRange(users);

                callback(users);
            }
            else
            {
                Debug.LogError("Failed to get top users: " + (task.Exception != null ? task.Exception.Message : "Unknown error"));
                callback(new List<User>());
            }
        });
    }
}