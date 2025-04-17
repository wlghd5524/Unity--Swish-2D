//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Text;
//using UnityEngine;
//using UnityEngine.Networking;

//public class WebConnector : MonoBehaviour
//{
//    public static WebConnector Instance { get; private set; }

//    private void Awake()
//    {
//        // 싱글톤 패턴 구현
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//        DontDestroyOnLoad(gameObject);
//    }

//    private string apiUrl = "http://gomgom5524.iptime.org:40000/api/User"; // 새로운 API 엔드포인트

//    // 서버에서 유저 정보를 가져옴
//    public IEnumerator Login(string userNumber, string name, string major)
//    {
//        yield return StartCoroutine(GetUser(userNumber, name, major));
//        if (LoginManager.Instance.currentUser != null && LoginManager.Instance.currentUser.playCount < 3)
//        {
//            LoginManager.Instance.loginSuccess = true;
//        }
//        else
//        {
//            LoginManager.Instance.loginSuccess = false;
//        }
//    }

//    public IEnumerator UpdateUsers()
//    {
//        yield return StartCoroutine(GetAllUsers());
//        User.users.Sort((x, y) =>
//        {
//            int scoreComparison = y.score.CompareTo(x.score);
//            if (scoreComparison != 0) return scoreComparison;

//            int playCountComparison = x.playCount.CompareTo(y.playCount);
//            if (playCountComparison != 0) return playCountComparison;

//            return x.timestamp.CompareTo(y.timestamp);
//        });
//    }


//    IEnumerator GetUser(string userNumber, string name, string major)
//    {
//        // GET 요청 준비 (UserNumber를 URL 경로에 포함)
//        UnityWebRequest www = UnityWebRequest.Get(apiUrl + $"/get_user/{userNumber}");

//        // 요청 전송
//        yield return www.SendWebRequest();

//        if (www.result != UnityWebRequest.Result.Success)
//        {
//            Debug.LogError($"Error: {www.error}");
//        }
//        else
//        {
//            string result = www.downloadHandler.text;

//            // "User not found" 메시지가 있는지 확인
//            ResponseMessage responseMessage = JsonUtility.FromJson<ResponseMessage>(result);

//            if (responseMessage != null && responseMessage.message == "User not found")
//            {
//                // 유저가 없을 때 처리할 로직
//                Debug.Log("User not found.");
//                LoginManager.Instance.currentUser = new User(userNumber, name, major);
//            }
//            else
//            {
//                // 반환된 JSON 데이터를 User 객체로 변환
//                User existingUser = JsonUtility.FromJson<User>(result);

//                // 유저 정보를 이용해 필요한 작업을 수행
//                Debug.Log($"User found: {existingUser.name}, PlayCount: {existingUser.playCount}");
//                LoginManager.Instance.currentUser = existingUser;
//            }
//        }
//    }

//    IEnumerator GetAllUsers()
//    {
//        // GET 요청 준비 (UserNumber를 URL 경로에 포함)
//        UnityWebRequest www = UnityWebRequest.Get(apiUrl + $"/get_all_users");

//        // 요청 전송
//        yield return www.SendWebRequest();

//        if (www.result != UnityWebRequest.Result.Success)
//        {
//            Debug.LogError($"Error: {www.error}");
//        }
//        else
//        {
//            string result = www.downloadHandler.text;
//            ResponseMessage responseMessage = JsonUtility.FromJson<ResponseMessage>(result);
//            // 메시지에 따라 처리
//            if (responseMessage.message == "User not found")
//            {
//                Debug.Log("No users found.");
//                yield break;
//            }

//            // 유저 배열이 null이 아닌 경우 리스트로 변환
//            if (responseMessage.users != null)
//            {
//                User.users = new List<User>(responseMessage.users);
//                Debug.Log("Users loaded successfully.");
//            }
//        }
//    }
//    public IEnumerator SendUserDataToServer(User user)
//    {
//        // UserData 객체를 JSON 문자열로 변환
//        string jsonData = JsonUtility.ToJson(user);
//        UnityWebRequest www;
//        if (user.playCount == 1)
//        {
//            // POST 요청 준비
//            www = new UnityWebRequest(apiUrl + "/register", "POST");
//        }
//        else
//        {
//            www = new UnityWebRequest(apiUrl + $"/update/{user.userNumber}", "PUT");
//        }
//        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
//        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
//        www.downloadHandler = new DownloadHandlerBuffer();
//        www.SetRequestHeader("Content-Type", "application/json");

//        // 요청 전송
//        yield return www.SendWebRequest();

//        // 응답 처리
//        if (www.result != UnityWebRequest.Result.Success)
//        {
//            Debug.LogError($"Error: {www.error}, Status Code: {www.responseCode}");
//        }
//        else
//        {
//            Debug.Log("User data sent successfully!");
//            Debug.Log($"Response: {www.downloadHandler.text}");
//        }

//        // 업로드 핸들러 및 다운로드 핸들러 해제
//        www.uploadHandler.Dispose();
//        www.downloadHandler.Dispose();
//    }
//}

//[System.Serializable]
//public class ResponseMessage
//{
//    public string message;
//    public User[] users;
//}

//[System.Serializable]
//public class UserArrayWrapper
//{
//    public User[] users;
//}
