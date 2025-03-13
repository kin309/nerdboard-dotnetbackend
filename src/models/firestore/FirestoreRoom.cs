using Google.Cloud.Firestore;

[FirestoreData]
public class FirestoreRoom
{
    [FirestoreProperty]
    public string? RoomId { get; set; }

    [FirestoreProperty]
    public string? RoomName { get; set; }

    [FirestoreProperty]
    public string? CreatedBy { get; set; }

    [FirestoreProperty]
    public Dictionary<string, FirestoreUser> Users { get; set; } = new Dictionary<string, FirestoreUser>();

    [FirestoreProperty]
    public List<FirestoreMessage> Messages { get; set; } = new List<FirestoreMessage>();

    public FirestoreRoom() { } // Construtor sem parâmetros necessário para desserialização
}
