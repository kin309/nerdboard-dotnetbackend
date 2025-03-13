using Google.Cloud.Firestore;

[FirestoreData]
public class FirestoreUser
{
    [FirestoreProperty]
    public string? Id { get; set; }

    [FirestoreProperty]
    public string? Username { get; set; }

    [FirestoreProperty]
    public string? Email { get; set; }

    public FirestoreUser() { } // Construtor vazio necessário para Firestore
}
