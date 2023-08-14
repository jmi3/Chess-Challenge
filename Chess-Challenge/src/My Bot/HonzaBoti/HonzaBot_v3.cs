using ChessChallenge.API;
using ChessChallenge.Application;
using System;
using System.Collections.Generic;

namespace ChessChallenge.Example;
public class HonzaBot_v3 : IChessBot
{

    public Move Think(Board board, Timer timer)
    {
        Console.WriteLine($">>>> Current eval: {Eval(board, board.IsWhiteToMove)}");
        Move move = GetTheMove(board, timer);
        
        return move;
    }

    public Move GetTheMove(Board board, Timer timer)
    {
        
        float bestEval;

        float eval, max = float.PositiveInfinity, min = float.NegativeInfinity;
        
        bool white = board.IsWhiteToMove;
        
        int positionsViewed = 0;
        
        int depth = 4;
        
        (Move move, eval, positionsViewed) = OrderABSearch(board, depth, white, white, min, max);

        Console.WriteLine($">>>> Searched: {positionsViewed}");
        return move;
    }

    List<Move> OrderMoves(Board board)
    {
        Move[] otherMoves = board.GetLegalMoves();
        List<Move> moves = new List<Move>(board.GetLegalMoves(true));
        
        for (int i = 0; i < otherMoves.Length; i++)
        {
            if (!moves.Contains(otherMoves[i]))
            {
                moves.Add(otherMoves[i]);
            }
        }

        return moves; 
    }

    (Move bestMove, float eval, int count) OrderABSearch(Board board, int depth, bool white, bool maximizing, float min = float.NegativeInfinity, float max = float.PositiveInfinity)
    {

        List<Move> moves = OrderMoves(board);

        if (moves.Count == 0)
        {

            if (board.IsDraw())
            {
                return (Move.NullMove, 0, 1);
            }

            if (board.IsInCheckmate())
            {
                if (maximizing) 
                {
                    return (Move.NullMove, float.MinValue, 1);
                }
                else
                {
                    return (Move.NullMove, float.MaxValue, 1);
                }
            }
    
        }
        
        if (depth == 0)
        {
            return (Move.NullMove, Eval(board, white), 1);
        }
        Move bestMove = Move.NullMove, tempMove;
        float bestEval, eval;
        int positionsViewed = 0, temp = 0;
        if (maximizing)
        {
            bestEval = float.MinValue;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                (tempMove, eval, temp) = OrderABSearch(board, depth - 1, white, false, min, max);
                board.UndoMove(move);
                
                positionsViewed += temp;

                if (eval > bestEval) 
                {
                    bestEval = eval;
                    bestMove = move;
                }
                
                if (min > bestEval)
                {
                    break;
                }

                min = Math.Max(min, eval);

            }

            return (bestMove, bestEval, positionsViewed);
        }
        else
        {
            bestEval = float.MaxValue;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                (tempMove, eval, temp) = OrderABSearch(board, depth - 1, white, true, min, max);
                board.UndoMove(move);
                
                positionsViewed += temp;
                if (eval < bestEval)
                {
                    bestEval = eval;
                    bestMove = move;  
                }

                if (max < bestEval)
                {
                    break;
                }
                max = Math.Min(max, bestEval);
            }
            return (bestMove, bestEval, positionsViewed);
        }
    }




    float EvalMaterial(Board board, bool white)
    {
        int P = board.GetPieceList(PieceType.Pawn, true).Count - board.GetPieceList(PieceType.Pawn, false).Count;
        int N = board.GetPieceList(PieceType.Knight, true).Count - board.GetPieceList(PieceType.Knight, false).Count;
        int B = board.GetPieceList(PieceType.Bishop, true).Count - board.GetPieceList(PieceType.Bishop, false).Count;
        int R = board.GetPieceList(PieceType.Rook, true).Count - board.GetPieceList(PieceType.Rook, false).Count;
        int Q = board.GetPieceList(PieceType.Queen, true).Count - board.GetPieceList(PieceType.Queen, false).Count;
        int K = board.GetPieceList(PieceType.King, true).Count - board.GetPieceList(PieceType.King, false).Count;

        //Multiply the material differences by their respective weights.
        float result =
            (9 * Q) + (5 * R) +
            (3 * N) + (3 * B) +
            (1 * P) + (12 * K);

        return result;
    }
    public float Eval(Board board, bool white)
    {
        if (board.IsDraw())
        {
            return 0;
        }
        float result = EvalMaterial(board, white);
        if (board.IsInCheckmate()) {
            result += white ? 100 : -100;
        }

        return result;

    }


  

}


