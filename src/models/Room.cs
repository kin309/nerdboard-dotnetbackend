    public class Room
    {
        public string? RoomId { get; set; }

        public string? RoomName { get; set; }

        public string? CreatedBy { get; set; }

        public Dictionary<string, User> Users { get; set; } = new Dictionary<string, User>();

        public List<Message> Messages { get; set; } = new List<Message>();

        public static Room GetRoomFromFirestore(FirestoreRoom firestoreRoom){
            Room room = new Room();
            room.RoomId = firestoreRoom.RoomId;
            room.RoomName = firestoreRoom.RoomName; 
            room.CreatedBy = firestoreRoom.CreatedBy;
            foreach (var user in firestoreRoom.Users){
                room.Users.Add(user.Key, User.GetFirestoreUser(user.Value));
            }
            foreach (var message in firestoreRoom.Messages){
                room.Messages.Add(Message.GetFirestoreMessage(message));
            }
            return room;
        }
    }