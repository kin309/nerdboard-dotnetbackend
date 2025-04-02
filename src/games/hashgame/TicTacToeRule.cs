using System;

public class TicTacToeRule : GameRule<TicTacToeLogic>
{
    private TicTacToeLogic _hashGame;

    public TicTacToeRule(TicTacToeLogic hashGame) : base(hashGame)
    {
        _hashGame = hashGame;
    }

    public void CheckWinner(TicTacToePlayer player, int[] coordinates)
    {
        Hash hash = _hashGame.GetHash();
        bool gameWin = hash.CheckCoordinate(
            [coordinates[0], coordinates[1]],
            player.Symbol
        );

        if (gameWin)
        {
            Win(player);
        }
    }
}