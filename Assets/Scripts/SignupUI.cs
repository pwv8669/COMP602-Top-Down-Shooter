using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SignupUI : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;

    [Header("Feedback Text")]
    public TextMeshProUGUI attemptFieldText;

    [Header("Buttons")]
    public Button signupButton;
    public Button backToLoginButton;

    void Start()
    {
        attemptFieldText.gameObject.SetActive(false);

        signupButton.onClick.AddListener(OnSignupPressed);
        backToLoginButton.onClick.AddListener(() => UIManager.Instance.ShowLogin());
    }

    void OnSignupPressed()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();
        string confirmPassword = confirmPasswordInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            // Incomplete sign-up
            attemptFieldText.text = "Please enter a username and password.";
            attemptFieldText.gameObject.SetActive(true);
            return;
        }

        if (password != confirmPassword)
        {
            // Invalid password
            attemptFieldText.text = "Passwords do not match.";
            attemptFieldText.gameObject.SetActive(true);
            return;
        }

        if (AccountManager.Instance.Signup(username, password))
        {
            // Successful account creation
            attemptFieldText.text = "Account created successfully!";
            attemptFieldText.gameObject.SetActive(true);
            UIManager.Instance.ShowLogin();
            Destroy(gameObject);
        }
        else
        {
            // Existing username
            attemptFieldText.text = "Username already exists.";
            attemptFieldText.gameObject.SetActive(true);
        }
    }
}
