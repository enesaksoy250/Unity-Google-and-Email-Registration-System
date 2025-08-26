using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Firestore;
using Google;
using UnityEngine;


public class LoginWithGoogle : MonoBehaviour
{
    public static LoginWithGoogle instance;

    public string GoogleWebApi = "670816235399-gt0palda6cpg0eaciqegs94rgtffcciq.apps.googleusercontent.com";

    FirebaseAuth auth;

    FirebaseUser user;

    private bool isGoogleSignInInitialized = false;

    public bool authInitialized = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
     
    }

    public void Initialize()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    public void LoginWithGoogleAndCheckDatabase()
    {
        if (!isGoogleSignInInitialized)
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = GoogleWebApi,
                RequestEmail = true,
                RequestProfile = true
            };
            isGoogleSignInInitialized = true;
        }

        GoogleSignIn.DefaultInstance.SignOut();


        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Google Sign-In failed.");
                MainMenuUI.Instance.OpenPanel("ErrorPanel");
                return;
            }

            else if (task.IsCanceled)
            {
                Debug.LogError("Google Sign-In canceled.");
                return;
            }

            MainMenuUI.Instance.OpenPanel("LoadingPanel");

            GoogleSignInUser googleUser = task.Result;
            Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null); 

            string username = googleUser.DisplayName;

            auth = FirebaseAuth.DefaultInstance;

            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
            {
                if (authTask.IsCanceled)
                {
                    Debug.Log("Google Sign-In was canceled by the user.");
                    MainMenuUI.Instance.ClosePanel("LoadingPanel");
                    MainMenuUI.Instance.OpenPanel("ErrorPanel", "Google Sign-In was canceled by the user.");
                    return;
                }

                if (authTask.IsFaulted)
                {
                    Debug.LogError($"Google Sign-In failed with error: {task.Exception}");
                    MainMenuUI.Instance.ClosePanel("LoadingPanel");
                    MainMenuUI.Instance.OpenPanel("ErrorPanel", $"Google Sign-In failed with error: {task.Exception}");
                    return;
                }

                user = auth.CurrentUser;
                string userId = user.UserId;

                FirebaseFirestore firestoreDb = FirebaseFirestore.DefaultInstance;
         
                firestoreDb.Collection("users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(dataTask =>
                {
                    if (dataTask.IsFaulted)
                    {
                        Debug.LogError("Database check failed.");
                        MainMenuUI.Instance.ClosePanel("LoadingPanel");
                        MainMenuUI.Instance.OpenPanel("ErrorPanel", "Database check failed.");
                    }
                    else if (dataTask.Result.Exists)
                    {
                        Debug.Log("Kullanıcı kaydı bulundu.");
                        MainMenuUI.Instance.ClosePanel("LoadingPanel");
                        MainMenuUI.Instance.OpenPanel("ErrorPanel", "Kullanıcı kaydı bulundu.");
                        DatabaseManager.Instance.Initialize(userId);                      
                    }
                    else
                    {
                        Debug.Log("Kayıt bulunamadı. Panel açılıyor...");
                        MainMenuUI.Instance.ClosePanel("LoadingPanel");
                        MainMenuUI.Instance.OpenPanel("ErrorPanel", "Eski kullanıcı kaydı bulunamadı yeni kayıt oluşturuluyor.");
                        DatabaseManager.Instance.Initialize(userId);
                        AuthenticationManager.Instance.WriteNewUser(username,userId);
                    
                    }
                });
            });
        });
    }

   

}