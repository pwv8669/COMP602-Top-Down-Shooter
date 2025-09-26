using UnityEngine;
using System.Collections.Generic;

public class AccountManager : MonoBehaviour
{
    public static AccountManager Instance { get; private set; }

    private Database db;
    public Account currentUser;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            db = SaveSystem.Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // C = Create (Account)
    public bool Signup(string username, string password)
    {
        foreach (var acc in db.accounts)
        {
            if (acc.username == username)
                return false; // username not unique
        }

        Account newAcc = new Account { username = username, password = password };
        db.accounts.Add(newAcc);
        currentUser = newAcc;
        SaveSystem.Save(db);
        return true;
    }

    // R = Read (Login)
    public bool Login(string username, string password)
    {
        foreach (var acc in db.accounts)
        {
            if (acc.username == username && acc.password == password)
            {
                currentUser = acc;
                return true;
            }
        }
        return false;
    }

    // U = Update (Password)
    public void UpdatePassword(string newPassword)
    {
        if (currentUser != null)
        {
            currentUser.password = newPassword;
            SaveSystem.Save(db);
        }
    }

    // D = Delete (Account)
    public void DeleteAccount(string username)
    {
        db.accounts.RemoveAll(acc => acc.username == username);
        if (currentUser != null && currentUser.username == username)
        {
            currentUser = null;
        }
        SaveSystem.Save(db);
    }

    // Always available: Save current state
    public void SaveProgress()
    {
        SaveSystem.Save(db);
    }

    public List<Account> GetAllAccounts()
    {
        return new List<Account>(db.accounts);
    }
}
