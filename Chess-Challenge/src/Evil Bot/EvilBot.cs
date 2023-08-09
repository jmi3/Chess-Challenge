using ChessChallenge.API;
using System;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

        public Move Think(Board board, Timer timer)
        {
            Move[] allMoves = board.GetLegalMoves();

            // Pick a random move to play if nothing better is found
            Random rng = new();
            Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
            int highestValueCapture = 0;

            foreach (Move move in allMoves)
            {
                // Always play checkmate in one
                if (MoveIsCheckmate(board, move))
                {
                    moveToPlay = move;
                    break;
                }

                // Find highest value capture
                Piece capturedPiece = board.GetPiece(move.TargetSquare);
                int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

                if (capturedPieceValue > highestValueCapture)
                {
                    moveToPlay = move;
                    highestValueCapture = capturedPieceValue;
                }
            }

            return moveToPlay;
        }

        // Test if this move gives checkmate
        bool MoveIsCheckmate(Board board, Move move)
        {
            board.MakeMove(move);
            bool isMate = board.IsInCheckmate();
            board.UndoMove(move);
            return isMate;
        }
    }
}
/*using ChessChallenge.API;
using ChessChallenge.Application;
using System;

public class EvilBot : IChessBot
{

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        Random random = new Random();
        return moves[random.Next(moves.Length)];
    
        *//*Move move = GetBestMoveOnMaterial(board, 3).move;
        if (MoveIsValid(board, move) && !move.IsNull)
        {
            return move;
        }
        else
        {
            Move[] moves = board.GetLegalMoves();
            Random random = new Random();
            return moves[random.Next(moves.Length)];
        }*//*
    }

    *//*
    
    Evaluation section
    
    *//*
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
    bool MoveIsMate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    (Move move, float eval, bool wasMate) GetBestMoveOnMaterial(Board board, int depth)
    {
        (Move move, float eval, bool wasMate) bestResult = (Move.NullMove, float.NegativeInfinity, false);

        (Move move, float eval, bool wasMate) result;

        Move[] moves = board.GetLegalMoves();

        if (depth > 0)
        {
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                result = GetBestMoveOnMaterial(board, depth - 1);
                result.eval *= -1;
                result.move = move;
                board.UndoMove(move);
                if (result.eval > bestResult.eval)
                {
                    bestResult = result;
                }

            }
        }
        else if (depth == 0)
        {
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                result = (move, (-1) * Eval(board, board.IsWhiteToMove), board.IsInCheckmate());
                board.UndoMove(move);

                if (result.eval > bestResult.eval)
                {
                    bestResult = result;
                }
            }
        }
        return bestResult;
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


}*/