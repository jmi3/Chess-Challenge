using ChessChallenge.API;
using ChessChallenge.Application; //DELETE
using System;

public class MMBot : IChessBot
{
    private float MMSearch(Board board, int depth, bool maximising) {
        if (depth == 0) {
            return Eval(board);
        }
        else if (board.IsInCheckmate()) {
            return maximising ? float.NegativeInfinity : float.PositiveInfinity;
        }

        Move[] moves = board.GetLegalMoves();   // TODO: sort the moves list
        float currBest = maximising ? float.NegativeInfinity : float.PositiveInfinity;

        foreach (Move m in moves) {
            board.MakeMove(m);
            float eval = MMSearch(board, depth - 1, !maximising);
            if (maximising && eval > currBest) {
                currBest = eval;
            }
            else if (!maximising && eval < currBest) {
                currBest = eval;
            }

            board.UndoMove(m);
        }
        return currBest;
    }

    public Move Think(Board board, Timer timer)
    {
        bool maximising = board.IsWhiteToMove;
        int searchDepth = 2;

        // THESE SHOULD REALLY HAVE CHANGED AFTER SEARCH!!
        Move bestMove = Move.NullMove;
        float bestScore = float.NaN;

        foreach(Move m in board.GetLegalMoves())
        {
            board.MakeMove(m);
            float eval = MMSearch(board, searchDepth, !maximising);

            // we always want to pick at least 1 move
            if (float.IsNaN(bestScore)) {
                bestScore = eval;
                bestMove = m;
            }

            if (maximising && eval > bestScore) {
                bestScore = eval;
                bestMove = m;
            }
            else if (!maximising && eval < bestScore) {
                bestScore = eval;
                bestMove = m;
            }
            board.UndoMove(m);
        }

        ConsoleHelper.Log(bestMove.ToString() + " was the best move found with eval: " + bestScore.ToString());

        return bestMove;
    }

    //Rudimentary evaluation function as proposed here
    // https://www.chessprogramming.org/Evaluation
    private float Eval(Board board)
    {
        int P = board.GetPieceList(PieceType.Pawn, true).Count - board.GetPieceList(PieceType.Pawn, false).Count;
        int N = board.GetPieceList(PieceType.Knight, true).Count - board.GetPieceList(PieceType.Knight, false).Count;
        int B = board.GetPieceList(PieceType.Bishop, true).Count - board.GetPieceList(PieceType.Bishop, false).Count;
        int R = board.GetPieceList(PieceType.Rook, true).Count - board.GetPieceList(PieceType.Rook, false).Count;
        int Q = board.GetPieceList(PieceType.Queen, true).Count - board.GetPieceList(PieceType.Queen, false).Count;

        //Multiply the material differences by their respective weights.
        float result = (9 * Q) + 
            (5 * R) + 
            (3 * (B + N)) + 
            (1 * P);

        if(!board.IsWhiteToMove)
        {
            result *= -1; //Adjusting result for when playing the black pieces.
        }

        return result;
    }
}
