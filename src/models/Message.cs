using Google.Cloud.Firestore;

[FirestoreData]
public class Message
{
    [FirestoreProperty]
    public string SenderId { get; set; }

    [FirestoreProperty]
    public string Content { get; set; }

    [FirestoreProperty]
    public Timestamp SentAt { get; set; } // Use Timestamp do Firestore para datas

    public Message() { } // Construtor vazio necessário para Firestore
}
