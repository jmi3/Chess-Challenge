using ChessChallenge.API;

<<<<<<< Updated upstream
namespace Bots;

=======
>>>>>>> Stashed changes
public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        return moves[0];
    }
}