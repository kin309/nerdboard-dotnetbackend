using Google.Cloud.Firestore;

public class Message
{
    public string? SenderId { get; set; }

    public string? Content { get; set; }

    public Timestamp SentAt { get; set; } // Use Timestamp do Firestore para datas

    public static Message GetFirestoreMessage(FirestoreMessage firestoreMessage)
    {
        Message message = new Message();
        message.SenderId = firestoreMessage.SenderId;
        message.Content = firestoreMessage.Content;
        message.SentAt = firestoreMessage.SentAt;
        return message;
    }
}
