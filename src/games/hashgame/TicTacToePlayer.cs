public class TicTacToePlayer
{
    public string Id { get; private set; }
    public string Name { get; set; }
    public string Symbol { get; set; }

    public TicTacToePlayer(string name, string id)
    {
        Name = name;
        Id = id;
        Symbol = string.Empty;
    }
}