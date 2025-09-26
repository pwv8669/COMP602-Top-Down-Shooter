using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject loginPrefab;
    public GameObject signupPrefab;

    private GameObject currentUI;

    void Awake()
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
        ShowLogin();
    }

    public void ShowLogin()
    {
        // swap between sign-up and log-in
        ClearCurrent();
        Canvas canvas = FindObjectOfType<Canvas>();
        currentUI = Instantiate(loginPrefab, canvas.transform, false);
        currentUI.name = loginPrefab.name;
        currentUI.transform.SetAsLastSibling();
    }

    public void ShowSignup()
    {
        // Swap between login and sign-up
        ClearCurrent();
        Canvas canvas = FindObjectOfType<Canvas>();
        currentUI = Instantiate(signupPrefab, canvas.transform, false);
        currentUI.name = signupPrefab.name;
        currentUI.transform.SetAsLastSibling();
    }

    private void ClearCurrent()
    {
        if (currentUI != null)
        {
            Destroy(currentUI);
            currentUI = null;
        }
    }
}
