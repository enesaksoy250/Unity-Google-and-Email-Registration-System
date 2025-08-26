using Firebase.Database;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    [HideInInspector] public string userID;
    private DatabaseReference dbReference;

    private FirebaseFirestore firestore;
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

    public void Initialize(string userId)
    {
        userID = userId;
        firestore = FirebaseFirestore.DefaultInstance;
        GetInfoFromFirestore();
    }


    public void UpdateFirestoreInfo<T>(string statName, T newValue, Action onComplete = null)
    {
        DocumentReference docRef = firestore.Collection("users").Document(userID);

        var updates = new Dictionary<string, object>
        {
            { statName, newValue }
        };

        docRef.UpdateAsync(updates).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log(statName + " Firestore'a kaydedildi!");
                onComplete?.Invoke();
            }
            else
            {
                Debug.LogError(statName + " Firestore'a kaydedilemedi! " + task.Exception);
            }
        });
    }

    // Belirli bir alaný artýr
    public void IncreaseFirestoreInfo(string statName, int incrementValue)
    {
        DocumentReference docRef = firestore.Collection("users").Document(userID);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists && snapshot.ContainsField(statName))
                {
                    int currentValue = 0;

                    try
                    {
                        // Firestore'daki deðeri int olarak al
                        currentValue = Convert.ToInt32(snapshot.GetValue<long>(statName));
                    }
                    catch
                    {
                        Debug.LogWarning(statName + " alaný int olarak alýnamadý. 0 kabul edildi.");
                    }

                    int newValue = currentValue + incrementValue;
                    UpdateFirestoreInfo(statName, newValue);
                }
                else
                {
                    // Alan yoksa yeni alan oluþtur
                    UpdateFirestoreInfo(statName, incrementValue);
                }
            }
            else
            {
                Debug.LogError("Firestore verisi çekilemedi! " + task.Exception);
            }
        });
    }

    public void GetUserDataFromFirestore(string key, Action<object> onSuccess)
    {
        firestore.Collection("users").Document(userID).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Firestore'dan {key} alýnýrken hata oluþtu: {task.Exception}");
            }
            else if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists && snapshot.ContainsField(key))
                {
                    object value = snapshot.GetValue<object>(key);
                    onSuccess(value);
                }
                else
                {
                    Debug.LogError($"{key} alaný bulunamadý!");
                }
            }
        });
    }

    public void GetInfoFromFirestore(Action onComplete = null)
    {
        var dataKeys = new Dictionary<string, Action<object>>
        {
            { "username", value => UserFirebaseInformation.Instance.username = value.ToString() },
            { "gameTime", value => UserFirebaseInformation.Instance.gameTime = Convert.ToInt32(value) },
            { "win", value => UserFirebaseInformation.Instance.win = Convert.ToInt32(value) },
            { "lose", value => UserFirebaseInformation.Instance.lose = Convert.ToInt32(value) }
        };

        int remainingKeys = dataKeys.Count;

        foreach (var entry in dataKeys)
        {
            GetUserDataFromFirestore(entry.Key, value =>
            {
                entry.Value(value);
                Debug.Log($"{entry.Key}: {value}");
                remainingKeys--;

                if (remainingKeys == 0)
                {
                    onComplete?.Invoke();
                }
            });
        }
    }


}
