using Google.Cloud.Firestore;

[FirestoreData]
public class Room
{
    [FirestoreProperty]
    public string RoomId { get; set; }

    [FirestoreProperty]
    public string RoomName { get; set; }

    [FirestoreProperty]
    public string CreatedBy { get; set; }

    [FirestoreProperty]
    public Dictionary<string, User> Users { get; set; } = new Dictionary<string, User>();

    [FirestoreProperty]
    public List<Message> Messages { get; set; } = new List<Message>();

    public Room() { } // Construtor sem parâmetros necessário para desserialização
}
