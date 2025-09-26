using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string savePath = Application.persistentDataPath + "/database.json";

    // Save modified database to JSON. Partial risk of corruption if an issue is made, but it's generally safe at this point.
    public static void Save(Database db)
    {
        string json = JsonUtility.ToJson(db, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Saved to: " + savePath);
    }

    // Load all accounts from JSON, non-accessible.
    public static Database Load()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<Database>(json);
        }
        else
        {
            return new Database();
        }
    }
}
