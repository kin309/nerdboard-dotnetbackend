using System;
using System.Collections.Generic;

public class GameRule<T>
{
    private T _game;
    private bool _ended = false;
    private List<Action<TicTacToePlayer>> _conditionMet = new List<Action<TicTacToePlayer>>();

    public GameRule(T game)
    {
        _game = game;
    }

    public bool Ended
    {
        get { return _ended; }
    }

    public void Win(TicTacToePlayer player)
    {
        if (!_ended)
        {
            foreach (var callback in _conditionMet)
            {
                callback(player);
            }
            _ended = true;
        }
    }

    public void AddConditionMetListener(Action<TicTacToePlayer> callback)
    {
        _conditionMet.Add(callback);
    }

    public void CallEvent(TicTacToePlayer player)
    {
        foreach (var callback in _conditionMet)
        {
            callback(player);
        }
    }
}