    public class Room
    {
        public string? RoomId { get; set; }

        public string? RoomName { get; set; }

        public string? CreatedBy { get; set; }

        public Dictionary<string, User> Users { get; set; } = new Dictionary<string, User>();

        public static Room GetRoomFromFirestore(FirestoreRoom firestoreRoom){
            Room room = new Room();
            room.RoomId = firestoreRoom.RoomId;
            room.RoomName = firestoreRoom.RoomName; 
            room.CreatedBy = firestoreRoom.CreatedBy;
            room.Users = new Dictionary<string,User>();
            return room;
        }
    }