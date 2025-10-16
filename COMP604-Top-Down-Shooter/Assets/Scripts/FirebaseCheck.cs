using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseCheck : MonoBehaviour
{
    FirebaseAuth auth;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                Debug.Log("Firebase connected to project successfully!");

                // Initialize Auth
                auth = FirebaseAuth.DefaultInstance;

                // Run a test to create a user account
                CreateTestUser();
            }
            else
            {
                Debug.LogError("Firebase failed to initialize: " + status);
            }
        });
    }

    void CreateTestUser()
    {
        string email = "testuser@example.com";    // use a fake but valid format
        string password = "Password123";

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Sign-up failed: " + task.Exception);
            }
            else
            {
                Debug.Log("User created successfully: " + task.Result.User.Email);
            }
        });
    }
}
