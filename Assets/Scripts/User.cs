using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;

[System.Serializable]
public class User
{
    public User(string uid, string email, string displayName, string photoUrl, int score, string timestamp)
    {
        this.uid = uid;
        this.email = email;
        this.displayName = displayName;
        this.photoUrl = photoUrl;
        this.score = score;
        this.timestamp = timestamp;
    }
    
    public string uid;
    public string email;
    public string displayName;
    public string photoUrl;
    public int score;
    public string timestamp;

    // static 변수는 JSON으로 직렬화되지 않음
    public static List<User> users = new List<User>();
}

[System.Serializable]
public class UserListWrapper
{
    public List<User> users;
}