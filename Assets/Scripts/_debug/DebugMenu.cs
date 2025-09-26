using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class DebugMenu : MonoBehaviour
{
    public Transform entriesParent;            // Content with VerticalLayoutGroup
    public GameObject accountEntryPrefab;      // Prefab for account entry (should have TMP_Text)
    public Button refreshButton;               // Button to refresh account list

    void Start()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(PopulateAccounts);

        PopulateAccounts();
    }

    void OnEnable()
    {
        PopulateAccounts();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            gameObject.SetActive(false);
    }

    public void PopulateAccounts()
    {
        if (entriesParent == null || accountEntryPrefab == null || AccountManager.Instance == null)
        {
            Debug.LogWarning("DebugMenu: Missing references. Assign everything in the Inspector.");
            return;
        }

        foreach (Transform child in entriesParent)
            Destroy(child.gameObject);

        List<Account> accounts = AccountManager.Instance.GetAllAccounts();
        if (accounts == null)
        {
            Debug.LogWarning("DebugMenu: AccountManager.Instance.GetAllAccounts() returned null.");
            return;
        }

        foreach (Account acc in accounts)
        {
            GameObject entry = Instantiate(accountEntryPrefab, entriesParent);
            entry.name = acc.username;

            TMP_Text text = entry.GetComponentInChildren<TMP_Text>();
            if (text != null)
                text.text = acc.username;
        }
    }
}
