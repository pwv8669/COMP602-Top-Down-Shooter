using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Handles login and sign-up UI events and connects them to AccountManager.
/// Attach this to a Canvas or a controller GameObject.
/// </summary>
public class AccountUI : MonoBehaviour
{
    [Header("Login UI")]
    public TMP_InputField loginUsernameField;
    public TMP_InputField loginPasswordField;
    public GameObject loginContainer;

    [Header("Sign Up UI")]
    public TMP_InputField signupEmailField;
    public TMP_InputField signupUsernameField;
    public TMP_InputField signupPasswordField;
    public TMP_InputField signupConfirmPasswordField;
    public GameObject signUpContainer;

    /// <summary>
    /// Ensures a FirebaseManager instance exists in the scene at startup.
    /// Creates one if none is found so Firebase is initialized before any UI calls.
    /// </summary>
    void Start()
    {
        if (FirebaseManager.Instance == null)
        {
            GameObject manager = GameObject.Find("FirebaseManager");
            if (manager == null)
            {
                manager = new GameObject("FirebaseManager");
                manager.AddComponent<FirebaseManager>();
            }
        }
    }

    /// <summary>
    /// Called by the Login button. Checks credentials with AccountManager.
    /// </summary>
    public void OnLoginSubmit()
    {
        if (loginUsernameField == null || loginPasswordField == null)
        {
            Debug.LogError("Login fields are not assigned on this instance.");
            return;
        }

        string userOrEmail = loginUsernameField.text.Trim();
        string password = loginPasswordField.text;

        if (string.IsNullOrEmpty(userOrEmail))
        {
            Debug.Log("Enter your username or email.");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            Debug.Log("Enter your password.");
            return;
        }

        if (FirebaseManager.Instance == null)
        {
            Debug.LogError("FirebaseManager.Instance is null. Make sure FirebaseManager is loaded in this scene.");
            return;
        }

        FirebaseManager.Instance.LoginUserOrEmail(userOrEmail, password, (success, error) =>
        {
            if (success)
            {
                Debug.Log("Login successful!");
                // TODO: move to next scene or enable main game UI
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                Debug.Log("Invalid username or password." + (string.IsNullOrEmpty(error) ? "" : $" ({error})"));
            }
        });
    }


    /// <summary>
    /// Called by the Sign Up button. Creates a new account with AccountManager.
    /// </summary>
    public void OnSignUpSubmit()
    {
        if (signupEmailField == null || signupUsernameField == null ||
            signupPasswordField == null || signupConfirmPasswordField == null)
        {
            Debug.LogError("One or more sign-up input fields are not assigned on this instance.");
            return;
        }

        if (FirebaseManager.Instance == null)
        {
            Debug.LogError("FirebaseManager.Instance is null. Make sure FirebaseManager is loaded in this scene.");
            return;
        }

        string email = signupEmailField.text.Trim();
        string username = signupUsernameField.text.Trim();
        string password = signupPasswordField.text;
        string confirm = signupConfirmPasswordField.text;

        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        {
            Debug.Log("Enter a valid email address.");
            return;
        }

        if (username.Length < 3 || username.Length > 16)
        {
            Debug.Log("Username must be between 3 and 16 characters.");
            return;
        }

        if (password != confirm)
        {
            Debug.Log("Passwords do not match.");
            return;
        }

        bool hasLetter = false;
        bool hasDigit = false;

        for (int i = 0; i < password.Length; i++)
        {
            char c = password[i];
            if (char.IsLetter(c)) hasLetter = true;
            if (char.IsDigit(c)) hasDigit = true;
            if (hasLetter && hasDigit) break;
        }

        if (password.Length < 8 || !hasLetter || !hasDigit)
        {
            Debug.Log("Password must be at least 8 characters and include both letters and numbers.");
            return;
        }

        FirebaseManager.Instance.SignUp(email, password, username, (success, error) =>
        {
            if (success)
            {
                Debug.Log("Account created! You can now log in.");
                ShowLogin();
            }
            else
            {
                Debug.Log("Username already exists or signup failed." + (string.IsNullOrEmpty(error) ? "" : $" ({error})"));
            }
        });
    }

    /// <summary>
    /// Called by the "Don't have an account?" button on the login UI.
    /// Hides login and shows sign-up.
    /// </summary>
    public void ShowSignUp()
    {
        if (loginContainer != null)
        {
            loginContainer.SetActive(false);
        }

        if (signUpContainer != null)
        {
            signUpContainer.SetActive(true);
        }
    }

    /// <summary>
    /// Called by the "<<" button on the sign-up UI.
    /// Hides sign-up and shows login.
    /// </summary>
    public void ShowLogin()
    {
        if (signUpContainer != null)
        {
            signUpContainer.SetActive(false);
        }

        if (loginContainer != null)
        {
            loginContainer.SetActive(true);
        }
    }
}
