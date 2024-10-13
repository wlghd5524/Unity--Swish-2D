using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class WebConnector : MonoBehaviour
{
    public static WebConnector Instance { get; private set; }

    private void Awake()
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

    private string apiUrl = "https://localhost:7052/api/User"; // 새로운 API 엔드포인트

    // 서버에서 유저 정보를 가져옴
    public bool Login(string userNumber, string name)
    {
        StartCoroutine(GetUser(userNumber, name));
        if (LoginManager.Instance.currentUser == null)
        {
            return false;
        }
        if (LoginManager.Instance.currentUser.PlayCount > 3)
        {
            return false;
        }
        return true;
    }

    // 유저 정보를 서버로 보냄
    public void Register(User user)
    {
        StartCoroutine(SendUserDataToServer(user));
    }

    public void UpdateUsers()
    {
        StartCoroutine(GetAllUsers());
        User.users.Sort((x, y) => x.Score.CompareTo(y.Score)); // 점수를 기준으로 오름차순 정렬
    }


    IEnumerator GetUser(string userNumber, string name)
    {
        // GET 요청 준비 (UserNumber를 URL 경로에 포함)
        UnityWebRequest www = UnityWebRequest.Get(apiUrl + $"/get_user/{userNumber}");

        // 요청 전송
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error: {www.error}");
        }
        else
        {
            string result = www.downloadHandler.text;

            // "User not found" 메시지가 있는지 확인
            ResponseMessage responseMessage = JsonUtility.FromJson<ResponseMessage>(result);

            if (responseMessage != null && responseMessage.message == "User not found")
            {
                // 유저가 없을 때 처리할 로직
                Debug.Log("User not found.");
                LoginManager.Instance.currentUser = new User(userNumber, name);
            }
            else
            {
                // 반환된 JSON 데이터를 User 객체로 변환
                User existingUser = JsonUtility.FromJson<User>(result);

                // 유저 정보를 이용해 필요한 작업을 수행
                Debug.Log($"User found: {existingUser.Name}, PlayCount: {existingUser.PlayCount}");
                LoginManager.Instance.currentUser = existingUser;
            }
        }
    }

    IEnumerator GetAllUsers()
    {
        // GET 요청 준비 (UserNumber를 URL 경로에 포함)
        UnityWebRequest www = UnityWebRequest.Get(apiUrl + $"/get_all_users");

        // 요청 전송
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error: {www.error}");
        }
        else
        {
            string result = www.downloadHandler.text;

            // 반환된 JSON 데이터를 User 객체로 변환
            List<User> allUsers = JsonHelper.FromJsonList<User>(result);
            User.users = allUsers;
        }
    }
    IEnumerator SendUserDataToServer(User user)
    {
        // UserData 객체를 JSON 문자열로 변환
        string jsonData = JsonUtility.ToJson(user);
        // POST 요청 준비
        UnityWebRequest www = new UnityWebRequest(apiUrl + "/register", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        // 요청 전송
        yield return www.SendWebRequest();

        // 응답 처리
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error: {www.error}");
        }
        else
        {
            Debug.Log("User data sent successfully!");
            Debug.Log($"Response: {www.downloadHandler.text}");
        }
    }
}

[System.Serializable]
public class ResponseMessage
{
    public string message;
}

public static class JsonHelper
{
    public static List<T> FromJsonList<T>(string json)
    {
        string newJson = "{\"Items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        if(wrapper.Items == null)
        {
            return new List<T>();
        }
        return new List<T>(wrapper.Items);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}
