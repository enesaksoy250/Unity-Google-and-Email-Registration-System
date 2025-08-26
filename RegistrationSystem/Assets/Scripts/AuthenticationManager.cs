using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Firebase.Firestore;

public class AuthenticationManager : MonoBehaviour
{
    FirebaseAuth auth;

    private FirebaseFirestore firestoreDb;

    [SerializeField] TMP_InputField saveUsernameInput, saveEmailInput, savePasswordInput;
    [SerializeField] TMP_InputField loginEmailInput, loginPasswordInput;

    private string userID;

    [HideInInspector] public bool onConnectDatabase;

    public static AuthenticationManager Instance { get; private set; }

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

    void Start()
    {
        LoginControl();

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {

            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase'e baglandi!");
                onConnectDatabase = true;
                auth = FirebaseAuth.DefaultInstance;
              
                firestoreDb = FirebaseFirestore.DefaultInstance;

                if (!PlayerPrefs.HasKey("Login"))
                    return;

                userID = auth.CurrentUser.UserId;
                DatabaseManager.Instance.Initialize(userID);
                LoginWithGoogle.instance.Initialize();
            }
        });


    }

    private void LoginControl()
    {
        if (!PlayerPrefs.HasKey("Login"))
        {
            MainMenuUI.Instance.ClosePanel("AnaMenuPanel");
            MainMenuUI.Instance.OpenPanel("WelcomePanel");
        }
    }

    public void SaveUser()
    {

        MainMenuUI.Instance.OpenPanel("LoadingPanel");

        string username;
        string email;
        string password;

        username = saveUsernameInput.text;
        email = saveEmailInput.text;
        password = savePasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            MainMenuUI.Instance.OpenPanel("ErrorPanel", "Tüm alanları doldurunuz!", "LoadingPanel");
            Debug.LogError("Tum alanlari doldurunuz!");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                HandleAuthError(task.Exception);
            }
            else
            {
                FirebaseUser newUser = task.Result.User;
                userID = newUser.UserId;
                Debug.Log("Kullanici kaydi basarili: " + newUser.UserId);
                WriteNewUser(username, userID);

            }
        });
    }


    public void WriteNewUser(string username, string userId)
    {
      
        Dictionary<string, object> userData = new()
        {
            { "username",username },
            { "gameTime",0 },
            { "win",0 },
            { "lose",0}
        };
  
        firestoreDb.Collection("users").Document(userId).SetAsync(userData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Kayıt başarılı!");
                FinalizeSaveProcess(username);
            }
            else
            {
                Debug.LogError("Kayıt hatası: " + task.Exception);
            }
        });
    }

    private void FinalizeSaveProcess(string username)
    {
        PlayerPrefs.SetInt("Login", 1);
 
        MainMenuUI.Instance.ClosePanel("SignUpPanel");
        MainMenuUI.Instance.ClosePanel("WelcomePanel");
        MainMenuUI.Instance.ClosePanel("LoadingPanel");
        MainMenuUI.Instance.OpenPanel("TeamSelectPanel"); 
        DatabaseManager.Instance.Initialize(userID);
          
    }


    public void Login()
    {

        MainMenuUI.Instance.OpenPanel("LoadingPanel");

        string enteredEmail = loginEmailInput.text;
        string enteredPassword = loginPasswordInput.text;

        if (string.IsNullOrEmpty(enteredEmail) || string.IsNullOrEmpty(enteredPassword))
        {
            MainMenuUI.Instance.OpenPanel("ErrorPanel", "Email ve şifre boş bırakılamaz!", "LoadingPanel");
            
            Debug.LogError("Email ve þifre boþ býrakýlamaz!"); //Error2
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(enteredEmail, enteredPassword).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                HandleLoginError(task.Exception);
            }
            else
            {
                FirebaseUser user = task.Result.User;
                userID = user.UserId;
                PlayerPrefs.SetInt("Login", 1);
                DatabaseManager.Instance.Initialize(userID);
                MainMenuUI.Instance.ClosePanel("WelcomePanel");
                MainMenuUI.Instance.ClosePanel("SignInPanel");
                MainMenuUI.Instance.ClosePanel("LoadingPanel");
                MainMenuUI.Instance.OpenPanel("AnaMenuPanel");
            }
        });
    }

    private void HandleAuthError(AggregateException exception)
    {
        foreach (var innerException in exception.Flatten().InnerExceptions)
        {
            if (innerException is FirebaseException firebaseEx)
            {
                switch (firebaseEx.ErrorCode)
                {
                    case (int)AuthError.EmailAlreadyInUse:
                        ShowErrorPanel("ErrorPanel", "Bu e-posta adresi zaten kullaniliyor.", "LoadingPanel");
                        Debug.LogError("Bu e-posta adresi zaten kullaniliyor.");
                        return;

                    case (int)AuthError.InvalidEmail:
                        ShowErrorPanel("ErrorPanel", "Email formatı yanlış.", "LoadingPanel");
                        Debug.LogError("Email formatı yanlış.");
                        return;

                    default:
                        ShowErrorPanel("ErrorPanel", "Bilinmeyen bir hata olustu.", "LoadingPanel");
                        Debug.LogError("Bilinmeyen bir hata olustu.");
                        return;
                }
            }
        }
        Debug.LogError("Kullanici kaydinda hata: " + exception);
    }

    private void HandleLoginError(AggregateException exception)
    {
        foreach (var innerException in exception.Flatten().InnerExceptions)
        {
            if (innerException is FirebaseException firebaseEx)
            {
                switch (firebaseEx.ErrorCode)
                {
                    case (int)AuthError.InvalidEmail:
                        ShowErrorPanel("ErrorPanel", "Email geçerli formatta değil!", "LoadingPanel");
                        return;

                    case (int)AuthError.WrongPassword:
                        ShowErrorPanel("ErrorPanel", "Şifre yanlış!", "LoadingPanel");
                        return;

                    case (int)AuthError.UserNotFound:
                        ShowErrorPanel("ErrorPanel", "Kullanıcı bulunamadı!", "LoadingPanel");
                        return;

                    default:
                        ShowErrorPanel("ErrorPanel", "Bir hata oluştu!", "LoadingPanel");
                        return;
                }
            }
        }
        Debug.LogError("Giris hatasi: " + exception);
    }


    private void ShowErrorPanel(string panelName, string message, string panelToClose)
    {
        MainMenuUI.Instance.OpenPanel(panelName, message, panelToClose);
    }
}
