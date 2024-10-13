using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class User
{
    public User(string userNumber, string name)
    {
        UserNumber = userNumber;
        Name = name;
        Score = 0;
        PlayCount = 0;
    }
    public static List<User> users = new List<User>();
    public string UserNumber { get; set; }
    public string Name { get; set; }
    public int Score { get; set; }
    public int PlayCount { get; set; }
}

[System.Serializable]
public class UserListWrapper
{
    public List<User> users;
}