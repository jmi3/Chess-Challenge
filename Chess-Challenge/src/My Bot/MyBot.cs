using ChessChallenge.API;
using ChessChallenge.Application;
using System;

namespace ChessChallenge.Example;
public class MyBot : IChessBot
{

    public Move Think(Board board, Timer timer)
    {

        Move[] legalMoves = board.GetLegalMoves();
        float bestEval = float.NegativeInfinity;
        Move move = Move.NullMove;
        float eval;
        bool white = board.IsWhiteToMove;

        for (int i = 0; i < legalMoves.Length; i++)
        {
            eval = GPTInDepthEval(board, 4, float.NegativeInfinity, float.PositiveInfinity, white);
            if (eval > bestEval)
            {
                move = legalMoves[i];
                bestEval = eval;
            }
        }
        Console.WriteLine($"MyBot: Found a {move} with evaluation {bestEval}");
        
        bool draw = IsADraw(board, move);

        if (draw)
        {
            Console.WriteLine("Not making it a draw intentionally");
            return randomMove(board);
        }
        else if (MoveIsValid(board, move) && !move.IsNull)
        {
            if (MoveIsMate(board, move))
            {
                Console.WriteLine("Gonna mate that little piece of shit");
            }
            Console.WriteLine($"MyBot: Making a {move} with evaluation {bestEval}");
            return move;

        }
        Console.WriteLine($"Move was null: {move.IsNull}");

        return randomMove(board);
    }


    
     //Picks a random move from all possible moves.

    Move randomMove(Board board)
    {
        Move[] moves = board.GetLegalMoves();
        Random random = new Random();
        return moves[random.Next(moves.Length)];
    }

    
    //Determines, whether the move leads to already seen position or not.Useles...? @-@
     
    bool IsADraw(Board board, Move move)
    {
        board.MakeMove(move);
        bool isadraw = board.IsDraw();
        board.UndoMove(move);
        return isadraw;
    }



    //Evaluation section


    //Determines, whether the move is valid.



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


    //Determines, whether the move is mate.


    bool MoveIsMate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    float InDepthEval(Board board, int depth, bool white, float currBest = float.NegativeInfinity)
    {
        Move[] moves = board.GetLegalMoves();
        float bestEval = float.NegativeInfinity;
        float tempEval;

        if (depth <= 0)
        {
            for (int i = 0; i < moves.Length; i++)
            {
                // Evaluate each move
                board.MakeMove(moves[i]);
                tempEval = -Eval(board, !white);
                board.UndoMove(moves[i]);

                // Now if the current best is smaller than what we found, we just quit the branch xD  

                if (currBest > tempEval)
                {
                    return float.NaN;
                }
                else 
                {
                    // Decide, whether the move is better than any of the previous evaluated
                    if (bestEval < tempEval) 
                    {
                        bestEval = tempEval;
                    }
                
                }

            }

        }
        else
        {
            for (int i = 0; i < moves.Length; i++)
            {
                // Evaluate each move
                board.MakeMove(moves[i]);
                tempEval = -InDepthEval(board, depth-1, !white, bestEval);
                board.UndoMove(moves[i]);

                if (float.IsNaN(tempEval))
                {
                    return bestEval;
                }
                // Now if the current best is smaller than what we found, we just quit the branch xD  
                else if (currBest > tempEval)
                {
                    return tempEval;
                }
                else
                {
                    // Decide, whether the move is better than any of the previous evaluated
                    if (bestEval < tempEval)
                    {
                        bestEval = tempEval;
                    }

                }
            }
        }
        return bestEval;


    }

    float GPTInDepthEval(Board board, int depth, float alpha, float beta, bool maximizingPlayer)

    {
        if (depth <= 0)
        {
            return Eval(board, maximizingPlayer);  // Replace with your evaluation function
        }

        Move[] moves = board.GetLegalMoves();

        if (maximizingPlayer)
        {
            float bestEval = float.NegativeInfinity;

            foreach (Move move in moves)
            {
                board.MakeMove(move);
                float tempEval = GPTInDepthEval(board, depth - 1, alpha, beta, !maximizingPlayer);
                board.UndoMove(move);

                bestEval = Math.Max(bestEval, tempEval);
                alpha = Math.Max(alpha, bestEval);

                if (alpha >= beta)
                {
                    break;  // Prune the branch
                }
            }

            return bestEval;
        }
        else
        {
            float bestEval = float.PositiveInfinity;

            foreach (Move move in moves)
            {
                board.MakeMove(move);
                float tempEval = GPTInDepthEval(board, depth - 1, alpha, beta, !maximizingPlayer);
                board.UndoMove(move);

                bestEval = Math.Min(bestEval, tempEval);
                beta = Math.Min(beta, bestEval);

                if (alpha >= beta)
                {
                    break;  // Prune the branch
                }
            }

            return bestEval;
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
        float result = EvalMaterial(board, white);
     
        if (!white) { result *= -1; }
     
        return result;

    }


  

}


