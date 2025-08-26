using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class User
{

    public string username;
    public int gameTime;
    public int win;
    public int lose;

    public User(string username, int gameTime, int win, int lose)
    {
        this.username = username;
        this.gameTime = gameTime;
        this.win = win;
        this.lose = lose;

    }
}
