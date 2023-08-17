using ChessChallenge.API;
using ChessChallenge.Application;
using System;
using System.Collections.Generic;

namespace ChessChallenge.Example;
public class EvilBot : IChessBot
{
    // HonzaBot v2.0
    // Uses fail-hard Alpha-Beta pruning
    // Has very naive ordering (captures first)

    public Move Think(Board board, Timer timer)
    {
        Move move = GetTheMove(board, timer);

        return move;
    }


    public Move GetTheMove(Board board, Timer timer)
    {
        List<Move> legalMoves = OrderMoves(board);
        Move move = Move.NullMove;
        float bestEval;

        float eval;
        bool white = board.IsWhiteToMove;
        int positionsViewed = 0;
        int depth = 3;
        if (white)
        {
            bestEval = float.NegativeInfinity;
            for (int i = 0; i < legalMoves.Count; i++)
            {
                board.MakeMove(legalMoves[i]);
                (eval, positionsViewed) = OrderABSearch(board, depth, !white);
                board.UndoMove(legalMoves[i]);
                if (eval > bestEval)
                {
                    move = legalMoves[i];
                    bestEval = eval;
                }
            }
        }
        else
        {
            bestEval = float.PositiveInfinity;
            for (int i = 0; i < legalMoves.Count; i++)
            {
                board.MakeMove(legalMoves[i]);
                (eval, positionsViewed) = OrderABSearch(board, depth, !white);
                board.UndoMove(legalMoves[i]);
                if (eval < bestEval)
                {
                    move = legalMoves[i];
                    bestEval = eval;
                }
            }
        }
        Console.WriteLine($">>>> HBv2.0 Searched: {positionsViewed}");
        return move;
    }

    List<Move> OrderMoves(Board board)
    {
        Move[] captureMoves = board.GetLegalMoves(true);
        Move[] otherMoves = board.GetLegalMoves();
        List<Move> moves = new List<Move>();
        for (int i = 0; i < captureMoves.Length; i++)
        {
            moves.Add(captureMoves[i]);
        }

        for (int i = 0; i < otherMoves.Length; i++)
        {
            if (!moves.Contains(otherMoves[i]))
            {
                moves.Add(otherMoves[i]);
            }
        }

        return moves;
    }

    (float eval, int count) OrderABSearch(Board board, int depth, bool white, float min = float.NegativeInfinity, float max = float.PositiveInfinity)
    {
        if (depth == 0)
        {

            return (Eval(board, white), 1);
        }
        List<Move> moves = OrderMoves(board);
        float best, eval;
        int positionsViewed = 0, temp;
        if (white)
        {
            best = float.NegativeInfinity;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                (eval, temp) = OrderABSearch(board, depth - 1, false, min, max);
                best = Math.Max(eval, best);
                board.UndoMove(move);
                positionsViewed += temp;
                if (move.IsPromotion && move.PromotionPieceType != PieceType.Queen)
                {
                    eval -= 2;
                }
                if (best > max)
                {
                    break;
                }
                min = Math.Max(min, best);

            }

            return (best, positionsViewed);
        }
        else
        {
            best = float.PositiveInfinity;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                (eval, temp) = OrderABSearch(board, depth - 1, false, min, max);
                best = Math.Min(eval, best);
                board.UndoMove(move);
                if (move.IsPromotion && move.PromotionPieceType != PieceType.Queen)
                {
                    eval += 2;
                }
                positionsViewed += temp;
                if (best < min)
                {
                    break;
                }
                max = Math.Min(max, best);
            }
            return (best, positionsViewed);
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
        if (board.IsInCheckmate())
        {
            result += white ? 100 : -100;
        }

        return result;

    }




}


