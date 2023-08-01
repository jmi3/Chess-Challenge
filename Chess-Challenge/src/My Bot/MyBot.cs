using ChessChallenge.API;
using ChessChallenge.Application;
public class MyBot : IChessBot
{
    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }
    public Move Think(Board b, Timer timer)
    {
        Move[] AllMoves = b.GetLegalMoves();
        foreach (Move move in AllMoves)
        {
            if (MoveIsCheckmate(b, move))
            {
                return move;
            }
            b.MakeMove(move);
            Move[] AllMoves2 = b.GetLegalMoves();
            bool leadsToLoss = false;
            foreach (Move move2 in AllMoves2)
            {
                if (MoveIsCheckmate(b, move2))
                {
                    leadsToLoss = true;
                    break;
                }
            }
            if (leadsToLoss)
            {
                continue;
            }
            b.UndoMove(move);
        }
    }
}