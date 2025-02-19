using Google.Cloud.Firestore;

[FirestoreData]
public class User
{
    [FirestoreProperty]
    public string Id { get; set; }

    [FirestoreProperty]
    public string Username { get; set; }

    [FirestoreProperty]
    public string Email { get; set; }

    public User() { } // Construtor vazio necessário para Firestore
}
