using System.Linq;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public FirebaseAuth Auth { get; private set; }
    public FirebaseFirestore Firestore { get; private set; }
    public FirebaseUser CurrentUser => Auth.CurrentUser;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread((Task<DependencyStatus> task) =>
        {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                Auth = FirebaseAuth.DefaultInstance;
                Firestore = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firebase initialized.");
            }
            else
            {
                Debug.LogError("Could not resolve Firebase dependencies: " + status);
            }
        });
    }

    public async void SignUp(string email, string password, string username, Action<bool, string> callback)
    {
        Debug.Log("SignUp() called. Auth=" + (Auth != null) + ", Firestore=" + (Firestore != null));

        // Force username to lowercase for consistency.
        username = username.Trim().ToLower();

        try
        {
            // Check if username already exists
            QuerySnapshot checkSnapshot = await Firestore.Collection("players")
                .WhereEqualTo("username", username)
                .GetSnapshotAsync();

            if (checkSnapshot != null && checkSnapshot.Count > 0)
            {
                callback(false, "Username already exists.");
                return;
            }

            // If username is unique, create Firebase Auth user
            Debug.Log("Creating Firebase Auth user for " + email);
            var authResult = await Auth.CreateUserWithEmailAndPasswordAsync(email, password);
            var user = authResult.User;

            Debug.Log("User created in Auth: " + user.UserId);

            // Wait briefly to ensure Auth session syncs with Firestore
            await Task.Delay(1000);

            var playerData = new PlayerData
            {
                email = email,
                username = username,
                wins = 0,
                losses = 0,
                score = 0,
                totalSessionTime = 0,
                lastUpdated = DateTime.UtcNow.ToString("o")
            };

            // Save player data to Firestore
            Debug.Log("About to write player data to Firestore: " + user.UserId);

            await Firestore.Collection("players")
                .Document(user.UserId)
                .SetAsync(playerData);

            Debug.Log("Firestore write SUCCESS for userId: " + user.UserId);
            callback(true, null);
        }
        catch (Exception e)
        {
            Debug.LogError("SignUp failed: " + e);
            callback(false, e.Message);
        }
    }

    private void Login(string email, string password, Action<bool, string> callback)
    {
        Auth.SignInWithEmailAndPasswordAsync(email, password)
        .ContinueWithOnMainThread((Task<Firebase.Auth.AuthResult> task) =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                callback(true, null);
            }
            else
            {
                callback(false, task.Exception?.Message);
            }
        });
    }

    private void UsernameLogin(string username, string password, Action<bool, string> callback)
    {
        Firestore.Collection("players").WhereEqualTo("username", username).GetSnapshotAsync()
        .ContinueWithOnMainThread((Task<QuerySnapshot> task) =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                var snapshot = task.Result;
                if (snapshot.Count == 1)
                {
                    var doc = snapshot.Documents.First();
                    string email = doc.GetValue<string>("email");
                    Login(email, password, callback);
                }
                else if (snapshot.Count == 0)
                {
                    callback(false, "No account found with that username.");
                }
                else
                {
                    callback(false, "Duplicate usernames found.");
                }
            }
            else
            {
                callback(false, task.Exception?.Message);
            }
        });
    }

    // LOGIN (username or email + password)
    public void LoginUserOrEmail(string userOrEmail, string password, Action<bool, string> callback)
    {
        string TrimmedInput = userOrEmail.Trim();

        if (userOrEmail.Contains("@") && userOrEmail.Contains("."))
        {
            Login(TrimmedInput, password, callback);
        }
        else
        {
            UsernameLogin(TrimmedInput.ToLower(), password, callback);
        }
    }

    // LOAD PLAYER DATA FROM ACCOUNT
    public void LoadPlayerData(Action<PlayerData, string> callback)
    {
        if (CurrentUser == null)
        {
            callback(null, "No user logged in.");
            return;
        }
        Firestore.Collection("players").Document(CurrentUser.UserId).GetSnapshotAsync()
        .ContinueWithOnMainThread((Task<DocumentSnapshot> task) =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                var doc = task.Result;
                if (doc.Exists)
                {
                    PlayerData data = doc.ConvertTo<PlayerData>();
                    callback(data, null);
                }
                else
                {
                    callback(null, "Player data does not exist.");
                }
            }
            else
            {
                callback(null, task.Exception?.Message);
            }
        });
    }

    // SAVE PLAYER DATA TO ACCOUNT
    public void SavePlayerData(PlayerData data, Action<bool, string> callback)
    {
        if (CurrentUser == null)
        {
            callback(false, "No user logged in.");
            return;
        }
        data.lastUpdated = DateTime.UtcNow.ToString("o");
        Firestore.Collection("players").Document(CurrentUser.UserId).SetAsync(data)
        .ContinueWithOnMainThread((Task setTask) =>
        {
            if (setTask.IsCompleted && !setTask.IsFaulted)
                callback(true, null);
            else
                callback(false, setTask.Exception?.Message);
        });
    }

    // SIGN OUT
    public void SignOut()
    {
        Auth.SignOut();
    }
}
