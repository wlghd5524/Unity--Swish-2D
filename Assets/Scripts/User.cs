using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class User
{
    public User(string userNumber, string name, string major)
    {
        this.userNumber = userNumber;
        this.name = name;
        this.score = 0;
        this.playCount = 0;
        this.major = major;
        this.timestamp = System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
    }

    public string userNumber;
    public string name;
    public string major;
    public int score;
    public int playCount;
    public string timestamp;

    // static 변수는 JSON으로 직렬화되지 않음
    public static List<User> users = new List<User>();

    // Equals 메서드 재정의 (userNumber를 기준으로 비교)
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        User other = (User)obj;
        return this.userNumber == other.userNumber;
    }

    // GetHashCode 재정의
    public override int GetHashCode()
    {
        return userNumber.GetHashCode();
    }
}

[System.Serializable]
public class UserListWrapper
{
    public List<User> users;
}