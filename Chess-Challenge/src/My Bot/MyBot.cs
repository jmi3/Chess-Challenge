using ChessChallenge.API;
using ChessChallenge.Application;
public class MyBot : IChessBot
{
   
    public Move Think(Board board, Timer timer)
    {
        Move[] AllMoves = board.GetLegalMoves();
        foreach (Move move in AllMoves)
        {
            if (MoveIsMate(board, move))
            {
                return move;
            }
            board.MakeMove(move);
            Move[] AllMoves2 = board.GetLegalMoves();
            bool leadsToLoss = false;
            foreach (Move move2 in AllMoves2)
            {
                if (MoveIsMate(board, move2))
                {
                    leadsToLoss = true;
                    break;
                }
            }
            if (leadsToLoss)
            {
                continue;
            }
            board.UndoMove(move);
        }
        return GetBestMoveOnMaterial(board, 2, 0).bestMove;
    }
    
    /*
    
    Evaluation section
    
    */

    bool MoveIsMate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    (Move bestMove, float bestEval) GetBestMoveOnMaterial(Board board, int depth, float currentBestEval)
    {
        float bestEval = 0;
        float eval;
        Move[] moves = board.GetLegalMoves();
        Move bestMove = moves[0];
        if (depth > 0)
        {

            foreach (Move move in moves)
            {
                board.MakeMove(move);
                eval = GetBestMoveOnMaterial(board, depth - 1, currentBestEval).bestEval;
                if (eval > bestEval)
                {
                    bestEval = eval;
                    bestMove = move;
                }
                board.UndoMove(move);
            }

        }
        else if (depth == 0)
        {
            foreach (Move move in board.GetLegalMoves())
            {
                board.MakeMove(move);
                eval = EvalMaterial(board, board.IsWhiteToMove);
                if (eval > bestEval)
                {
                    bestEval = eval;
                    bestMove = move;
                }
                board.UndoMove(move);
            }
        }
        return (bestMove, bestEval);
    }

    float EvalMaterial(Board board, bool white)
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
            (3 * N) + (3 * B) + 
            (1 * P);

        if (!white) { result *= -1; }

        return result;
    }


}