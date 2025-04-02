using System;
using System.Collections.Generic;

public class TicTacToeLogic
{
    private Hash _hash = new Hash();
    private TicTacToeTurnManager _turnManager;
    private List<TicTacToePlayer> _players;
    private TicTacToeRule _rule;
    
    private List<Action<TicTacToeState>> _playerPlayCallbacks = new List<Action<TicTacToeState>>();
    
    private TicTacToePlayer? _winner = null;

    public TicTacToeLogic(List<TicTacToePlayer> players)
    {
        _players = players;
        _turnManager = new TicTacToeTurnManager(_players);
        _rule = new TicTacToeRule(this);
        _rule.AddConditionMetListener(OnPlayerWin);
    }

    public void Play(int[] coordinates)
    {
        string? playerSymbol = _turnManager.GetActualPlayer()?.Symbol;
        if (string.IsNullOrEmpty(playerSymbol)) return;
        
        HashTile playTile = _hash.GetTile(coordinates);

        if (string.IsNullOrEmpty(playTile.GetSymbol()))
        {
            playTile.SetSymbol(playerSymbol);
        }

        TicTacToePlayer? player = _turnManager.GetActualPlayer();

        if (player != null)
        {
            _rule.CheckWinner(player, coordinates);
        }
        else
        {
            Console.Error.WriteLine("Player is null or undefined.");
        }

        _turnManager.ChangeTurn();
        CallEvent(GetGameState());
    }

    public Hash GetHash()
    {
        return _hash;
    }

    public void OnPlayerWin(TicTacToePlayer player)
    {
        _winner = player;
    }

    public void AddListener(Action<TicTacToeState> callback)
    {
        _playerPlayCallbacks.Add(callback);
    }

    public void CallEvent(TicTacToeState gameState)
    {
        foreach (var callback in _playerPlayCallbacks)
        {
            callback(gameState);
        }
    }

    public TicTacToeRule GetRule()
    {
        return _rule;
    }

    public bool IsPlayerTurn(TicTacToePlayer player)
    {
        return player == _turnManager.GetActualPlayer();
    }

    public List<TicTacToePlayer> GetPlayers()
    {
        return _players;
    }

    public TicTacToeState GetGameState()
    {
        return new TicTacToeState
        {
            PlayerTurn = _turnManager.GetActualPlayer()?.Id,
            Grid = _hash.GetGrid(),
            PlayerWin = _winner,
            Draw = IsTie(_hash.GetGrid(), _winner),
            Players = _players
        };
    }

    public bool HasEnded()
    {
        return _rule.Ended;
    }

    private static bool IsTie(List<List<HashTile>> grid, TicTacToePlayer? winner)
    {
        return winner == null && grid.TrueForAll(row => row.TrueForAll(tile => !string.IsNullOrEmpty(tile.GetSymbol())));
    }
}

public class TicTacToeState
{
    public string? PlayerTurn { get; set; }
    public List<List<HashTile>>? Grid { get; set; }
    public TicTacToePlayer? PlayerWin { get; set; }
    public bool Draw { get; set; }
    public List<TicTacToePlayer>? Players { get; set; }
}