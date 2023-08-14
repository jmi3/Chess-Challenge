using ChessChallenge.API;
using ChessChallenge.Application; //DELETE
using System;
using static System.Math;


public class SymmetricEvalBot : IChessBot
{
    private const int PLYDEPTH = 3;
    public Move Think(Board board, Timer timer)
    {
        ConsoleHelper.Log("Bot is thinking. Current eval: " + Eval(board, true).ToString(), false, ConsoleColor.Blue);
        Move[] moves = board.GetLegalMoves();

        Move my_move = GetBestMove(board, moves, 1);

        return my_move;
    }

    private Move GetBestMove(Board board, Move[] moves, int ply)
    {

        if (moves.Length <= 0) { return new Move(); }
        bool white = board.IsWhiteToMove;

        (Move move, float eval) result = Minimax(board, PLYDEPTH, -float.MaxValue, float.MaxValue, true, white);

        String current_side = white ? "White" : "Black";
        ConsoleHelper.Log(result.move.ToString() + " was the best move for " + current_side + " found with eval: " + Math.Abs(result.eval).ToString());
        return result.move;
    }

    //Rudimentary evaluation function as proposed here
    // https://www.chessprogramming.org/Evaluation
    private float Eval(Board board, bool white)
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

    //Aplha and beta to be set to float max vals accordingly
    private (Move move, float eval) Minimax(Board board, int depth, float alpha, float beta, bool maximising, bool white)
    {
        Move[] moves = board.GetLegalMoves(false);
        if (moves.Length <= 0)
        {
            // This is going to get much more confusing at greater depths.
            ConsoleHelper.Log($"Error accessing a move. No legal moves @ depth {depth}. Resolving...", false, ConsoleColor.Red);
            if (board.IsInCheckmate())
            {
                //Check ply, if odd then this is a good thing! If even then M1 :(
                if (depth % 2 == 0)
                {
                    ConsoleHelper.Log($"-> Opponent has M1", false, ConsoleColor.Red);
                    if (white) { return (new Move(), -float.MaxValue); } else { return (new Move(), float.MaxValue); }
                }
                else
                {
                    ConsoleHelper.Log($"-> Opponent is in checkmate!", false, ConsoleColor.Green);
                    if (white) { return (new Move(), float.MaxValue); } else { return (new Move(), -float.MaxValue); }
                }
            }
            else if (board.IsDraw())
            {
                //Returning a random with an eval of zero should make it the most favourable option if losing completely. (I think?)
                ConsoleHelper.Log($"-> Draw found", false, ConsoleColor.Yellow);
                return (new Move(), 0);
            }
            else
            {
                ConsoleHelper.Log($"-> Can't explain depth problem, consult Titan submersible.", true, ConsoleColor.Red);
            }

        }
        if (depth <= 0) return (moves[0], Eval(board, white));

        Move best_move = moves[0];
        float eval;

        if (maximising)
        {
            float max_eval = -float.MaxValue;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                eval = Minimax(board, depth - 1, alpha, beta, false, white).eval;
                board.UndoMove(move);
                if (eval > max_eval)
                {
                    max_eval = eval;
                    best_move = move;
                }
                alpha = Max(alpha, eval);
                if (beta <= alpha)
                {
                    break; //As seb once said: "Snip"
                }

            }
            return (best_move, max_eval);
        }
        else
        {
            float min_eval = float.MaxValue;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                eval = Minimax(board, depth - 1, alpha, beta, true, white).eval;
                board.UndoMove(move);
                if (eval < min_eval)
                {
                    min_eval = eval;
                    best_move = move;
                }
                beta = Min(beta, eval);
                if (beta <= alpha)
                {
                    break;
                }
            }
            return (best_move, min_eval);
        }

    }
}

/*
 * Documenting funny bugs
 * 7/28: Bot plays moves that sacrifice the most material possible (best eval for the other player).
*/
