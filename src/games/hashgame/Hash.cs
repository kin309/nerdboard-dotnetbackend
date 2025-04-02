using System;
using System.Collections.Generic;

public class Hash
{
    private List<List<HashTile>> _hash = new List<List<HashTile>>();

    public Hash()
    {
        BuildHash();
    }

    private void BuildHash()
    {
        for (int i = 0; i < 3; i++)
        {
            _hash.Add(new List<HashTile>());
            for (int j = 0; j < 3; j++)
            {
                _hash[i].Add(new HashTile(i, j));
            }
        }
        Console.WriteLine($"Final hash: {_hash}");
    }

    public bool CheckCoordinate(int[] coordinate, string symbol)
    {
        if (LineCheck(coordinate[0], symbol))
        {
            return true;
        }

        if (ColumnCheck(coordinate[1], symbol))
        {
            return true;
        }

        if (coordinate[0] == coordinate[1] && MainDiagonalCheck(symbol))
        {
            return true;
        }

        if (coordinate[0] + coordinate[1] == 2 && SecondaryDiagonalCheck(symbol))
        {
            return true;
        }

        return false;
    }

    public bool LineCheck(int line, string symbol)
    {
        int count = 0;
        for (int j = 0; j < 3; j++)
        {
            string actualSymbol = _hash[line][j].GetSymbol();
            if (actualSymbol == symbol)
            {
                count++;
            }
            else
            {
                return false;
            }
        }

        return count == 3;
    }

    public bool ColumnCheck(int column, string symbol)
    {
        int count = 0;
        for (int i = 0; i < 3; i++)
        {
            string actualSymbol = _hash[i][column].GetSymbol();
            if (actualSymbol == symbol)
            {
                count++;
            }
            else
            {
                return false;
            }
        }

        return count == 3;
    }

    public bool MainDiagonalCheck(string symbol)
    {
        int count = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (i == j)
                {
                    string actualSymbol = _hash[i][j].GetSymbol();
                    if (actualSymbol == symbol)
                    {
                        count++;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        return count == 3;
    }

    public bool SecondaryDiagonalCheck(string symbol)
    {
        int count = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (i + j == 2)
                {
                    string actualSymbol = _hash[i][j].GetSymbol();
                    if (actualSymbol == symbol)
                    {
                        count++;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        return count == 3;
    }

    public HashTile GetTile(int[] coordinates)
    {
        return _hash[coordinates[0]][coordinates[1]];
    }

    public List<List<HashTile>> GetGrid()
    {
        return _hash;
    }
}