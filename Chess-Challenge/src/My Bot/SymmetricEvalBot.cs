using ChessChallenge.API;
using ChessChallenge.Application; //DELETE
using System;

namespace Bots;

public class SymmetricEvalBot : IChessBot
{
    private const int PLYDEPTH = 2;
    public Move Think(Board board, Timer timer)
    {
        ConsoleHelper.Log("NEW MOVE", false, ConsoleColor.Blue);
        Move[] moves = board.GetLegalMoves();

        Move my_move = GetBestMove(board, moves, 1);

        return my_move;
    }

    private Move GetBestMove(Board board, Move[] moves, int ply)
    {
        //FOR EVERY MOVE check the affect it would have on the board. (This obviously needs to be optimised).
        float best_eval = 0;
        if(moves.Length <= 0) { return new Move(); }
        Move best_move = moves[0];
        bool white = board.IsWhiteToMove;

        foreach (Move m in moves)
        {
            board.MakeMove(m);
            float e = Eval(board, board.IsWhiteToMove, ply, m);
            if (!white) { e *= -1; }
            if (ply == 2) { ConsoleHelper.Log(m.ToString() + "->" + e.ToString()); }

            if (e > best_eval)
            {
                best_eval = e;
                best_move = m;
            }

            board.UndoMove(m);
        }

        String current_side = board.IsWhiteToMove ? "White" : "Black";
        ConsoleHelper.Log(best_move.ToString() + " was the best move for " + current_side + " found with eval: " + Math.Abs(best_eval).ToString());
        return best_move;
    }

    //Rudimentary evaluation function as proposed here
    // https://www.chessprogramming.org/Evaluation
    private float Eval(Board board, bool white, int ply, Move prev)
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

        if (ply < PLYDEPTH)
        {
            ply += 1;
            Move opp_best_move = GetBestMove(board, board.GetLegalMoves(false), ply);
            String current_side = board.IsWhiteToMove ? "White" : "Black";
            board.MakeMove(opp_best_move);
            float eval_delta = Eval(board, !white, ply, opp_best_move);
            result += eval_delta;
            ConsoleHelper.Log("I think that " + current_side + "'s best move is: " + opp_best_move.ToString() + " if I play " + prev.ToString());
            ConsoleHelper.Log("Eval delta: " + eval_delta.ToString() + " => " + result.ToString(), false,ConsoleColor.Red);
            board.UndoMove(opp_best_move);
        }

        return result;
    }
}

/*
 * Documenting funny bugs
 * 7/28: Bot plays moves that sacrifice the most material possible (best eval for the other player).
*/
