using Google.Cloud.Firestore;

[FirestoreData]
public class FirestoreMessage
{
    [FirestoreProperty]
    public string? SenderId { get; set; }

    [FirestoreProperty]
    public string? Content { get; set; }

    [FirestoreProperty]
    public Timestamp SentAt { get; set; } // Use Timestamp do Firestore para datas

    public FirestoreMessage() { } // Construtor vazio necessário para Firestore
}
