using ChessChallenge.API;
using ChessChallenge.Application;
public class MyBot : IChessBot
{
   
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
        return AllMoves[0];
    }
    
    /*
    
    Evaluation section
    
    */

    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }


    public float EvalMaterial(Board board, bool white)
    {
        int P = board.GetPieceList(PieceType.Pawn, true).Count - board.GetPieceList(PieceType.Pawn, false).Count;
        int N = board.GetPieceList(PieceType.Knight, true).Count - board.GetPieceList(PieceType.Knight, false).Count;
        int B = board.GetPieceList(PieceType.Bishop, true).Count - board.GetPieceList(PieceType.Bishop, false).Count;
        int R = board.GetPieceList(PieceType.Rook, true).Count - board.GetPieceList(PieceType.Rook, false).Count;
        int Q = board.GetPieceList(PieceType.Queen, true).Count - board.GetPieceList(PieceType.Queen, false).Count;
        //int K = board.GetPieceList(PieceType.King, true).Count - board.GetPieceList(PieceType.King, false).Count;

        //Multiply the material differences by their respective weights.
        float result = (9 * Q) +
            (5 * R) +
            (3 * N) + (3.2f * B) + //N.B. the higher weighting for bishops!
            (1 * P);

        if (!white) { result *= -1; }

        return result;
    }


}