public class HashTile
{
    private int[] _coordinates;
    private string _symbol;

    public HashTile(int i, int j)
    {
        _coordinates = [i, j];
        _symbol = "";
    }

    public int[] GetCoordinates()
    {
        return _coordinates;
    }

    public string GetSymbol()
    {
        return _symbol;
    }

    public void SetSymbol(string symbol)
    {
        _symbol = symbol;
    }
}