public class User{
    public string? Id { get; set; }
    public string? ContextId { get; set; }
    public string? Username { get; set; }

    public static User GetFirestoreUser(FirestoreUser firestoreUser){
        User user = new User();
        user.Id = firestoreUser.Id;
        user.Username = firestoreUser.Username;
        return user;
    }
}
