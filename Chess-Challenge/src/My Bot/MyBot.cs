using ChessChallenge.API;
using ChessChallenge.Application;
using System;

namespace ChessChallenge.Example;
public class MyBot : IChessBot
{

    public Move Think(Board board, Timer timer)
    {

        Move move = GetBestMove(board, 3);
        if (MoveIsValid(board, move) && !move.IsNull)
        {

            bool draw = IsADraw(board, move);
            if (draw)
            {
                Console.WriteLine("Not making it a draw intentionally");
                return randomMove(board);
            }
            if (MoveIsMate(board, move))
            {
                Console.WriteLine("Gonna mate that little piece of shit");
                return move;
            }
            if (MoveIsValid(board, move) && !move.IsNull)
            {
                return move;
            }
            return move;
        }
        return randomMove(board);
    }


    /*
     * Picks a random move from all possible moves.
    */
    Move randomMove(Board board)
    {
        Move[] moves = board.GetLegalMoves();
        Random random = new Random();
        return moves[random.Next(moves.Length)];
    }

 /*   
     * Determines, whether the move leads to already seen position or not.Useles...? @-@
  */   
    bool IsADraw(Board board, Move move)
    {
        board.MakeMove(move);
        bool isadraw = board.IsDraw();
        board.UndoMove(move);
        return isadraw;
    }

/*

    Evaluation section
    
    

* Determines, whether the move is valid.
*/
bool MoveIsValid(Board board, Move move)
    {

        try
        {
            board.MakeMove(move);
            board.UndoMove(move);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

/*
* Determines, whether the move is mate.

*/
bool MoveIsMate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    Move GetBestMove(Board board, int depth, float prevBestEval = float.NegativeInfinity)
    {
        float bestEval = float.NegativeInfinity;

        float eval;
        Move resultMove;
        Move[] moves = board.GetLegalMoves();
        return randomMove(board);
    }

    float[] InDepthEval(Board board, Move[] moves, int depth, float currBest = float.NegativeInfinity)
    {

        float[] movesEval = new float[moves.Length];
        Move move;
        if (depth > 0)
        {
            for (int i = 0; i < moves.Length; i++)
            {
                move = moves[i];
                board.MakeMove(move);
                if (board.IsInCheckmate())
                {
                    board.UndoMove(move);
                    movesEval[i] = float.PositiveInfinity;
                }

                else if (board.IsInCheck())
                {
                    depth++;
                }

                else
                {
                    movesEval[i] = InDepthEval(board, board.GetLegalMoves(), depth - 1);

                    movesEval[i] *= -1;

                    board.UndoMove(move);
                    if (movesEval[i] < currBest)
                    {
                        movesEval[i] = float.NaN;
                    }
                }


            }
        }
        else if (depth == 0)
        {
            for (int i = 0; i < moves.Length; i++)
            {
                move = moves[i];
                board.MakeMove(move);
                movesEval[i] = (-1) * Eval(board, board.IsWhiteToMove);
                if (board.IsInCheckmate())
                {
                    movesEval[i] = float.NegativeInfinity;
                    board.UndoMove(move);
                }
                board.UndoMove(move);

                if (movesEval[i] < currBest && MoveIsValid(board, move))
                {
                    movesEval[i] = float.NaN;
                }
            }
        }
        return movesEval;
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
        float result = (9 * Q) +
            (5 * R) +
            (3 * N) + (3 * B) +
            (1 * P) + (12 * K);

        return result;
    }
    public float Eval(Board board, bool white)
    {
        float result = EvalMaterial(board, white);


        if (!white)
        {
            result *= -1;
        }
        return result;
    }


}

