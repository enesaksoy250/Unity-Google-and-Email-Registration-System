using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserFirebaseInformation : MonoBehaviour
{
    public static UserFirebaseInformation Instance { get; private set; }

    [HideInInspector] public string username;
    [HideInInspector] public int gameTime;
    [HideInInspector] public int win;
    [HideInInspector] public int lose;

    private void Awake()
    {
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


    public void SetUserData(string username, int gameTime, int win, int lose)
    {
        this.username = username;
        this.gameTime = gameTime;
        this.win = win;
        this.lose = lose;
    }

}
