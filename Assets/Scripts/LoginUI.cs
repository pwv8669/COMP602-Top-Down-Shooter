using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    [Header("Feedback Text")]
    public TextMeshProUGUI attemptFieldText;

    [Header("Buttons")]
    public Button loginButton;
    public Button goToSignupButton;

    void Start()
    {
        attemptFieldText.gameObject.SetActive(false);

        loginButton.onClick.AddListener(OnLoginPressed);
        goToSignupButton.onClick.AddListener(() => UIManager.Instance.ShowSignup());
    }

    void OnLoginPressed()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            // Incomplete Login
            attemptFieldText.text = "Please enter both username and password.";
            attemptFieldText.gameObject.SetActive(true);
            return;
        }

        
        if (AccountManager.Instance.Login(username, password))
        {
            // Successful Login
            Destroy(gameObject);
        }
        else
        {
            // Unsuccessful Login
            attemptFieldText.text = "Incorrect username or password.";
            attemptFieldText.gameObject.SetActive(true);
            passwordInput.text = string.Empty;
        }
    }
}
