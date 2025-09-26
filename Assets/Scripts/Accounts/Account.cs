using System;

[Serializable]
public class Account
{
    public string username;
    public string password; // no hashing yet
    public Stats stats = new Stats();
}
