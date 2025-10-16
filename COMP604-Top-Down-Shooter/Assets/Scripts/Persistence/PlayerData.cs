using Firebase.Firestore;

[FirestoreData]
public class PlayerData
{
    [FirestoreProperty] public string email { get; set; }
    [FirestoreProperty] public string username { get; set; }
    [FirestoreProperty] public int wins { get; set; }
    [FirestoreProperty] public int losses { get; set; }
    [FirestoreProperty] public int score { get; set; }
    [FirestoreProperty] public long totalSessionTime { get; set; }
    [FirestoreProperty] public string lastUpdated { get; set; }
}
