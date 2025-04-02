using System;
using System.Collections.Generic;

public class TicTacToeTurnManager
{
    private List<TicTacToePlayer> _players;
    private TicTacToePlayer? _actualPlayer = null;
    private int _actualIndex;

    public TicTacToeTurnManager(List<TicTacToePlayer> players)
    {
        _players = players;
        _actualIndex = RandomizeTurn();
        _actualPlayer = players[_actualIndex];

        _players[0].Symbol = "x";
        _players[1].Symbol = "o";
    }

    public int RandomizeTurn()
    {
        Random rand = new Random();
        return rand.Next(0, 2); // Returns 0 or 1
    }

    public void ChangeTurn()
    {
        _actualIndex = (_actualIndex == 0) ? 1 : 0;
        _actualPlayer = _players[_actualIndex];
    }

    public TicTacToePlayer? GetActualPlayer()
    {
        return _actualPlayer;
    }
}