/*using ChessChallenge.API;
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
}*/
using ChessChallenge.API;
using ChessChallenge.Application;
using System;

namespace ChessChallenge.Example;
public class EvilBot: IChessBot
{

    public Move Think(Board board, Timer timer)
    {

        Move move = GetBestMoveOnMaterial(board, 3).move;
        bool draw = IsADraw(board, move);
        if (draw)
        {
            Console.WriteLine("making it a draw intentionally");
        }
        if (MoveIsMate(board, move))
        {
            Console.WriteLine("gonna mate that little piece of shit");
            return move;
        }
        if (MoveIsValid(board, move) && !move.IsNull)
        {
            if (draw == false)
            {
                return move;
            }
        }
        return randomMove(board);
    }
    Move randomMove(Board board)
    {
        Move[] moves = board.GetLegalMoves();
        Random random = new Random();
        return moves[random.Next(moves.Length)];
    }
    bool IsADraw(Board board, Move move)
    {
        board.MakeMove(move);
        bool isadraw = board.IsDraw();
        board.UndoMove(move);
        return isadraw;
    }

    /*
    
    Evaluation section
    
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
    bool MoveIsMate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    (Move move, double eval, bool wasMate) GetBestMoveOnMaterial(Board board, int depth)
    {
        (Move move, double eval, bool wasMate) bestResult = (Move.NullMove, double.NegativeInfinity, false);

        (Move move, double eval, bool wasMate) result;

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
        int K = board.GetPieceList(PieceType.King, true).Count - board.GetPieceList(PieceType.King, false).Count;

        //Multiply the material differences by their respective weights.
        float result = (9 * Q) +
            (5 * R) +
            (3 * N) + (3 * B) +
            (1 * P) + (12 * K);

        return result;
    }
    double DistanceFromCentre(Square square)
    {
        return Math.Sqrt(Math.Pow(square.File - 3.5f, 2f) + Math.Pow(square.Rank - 3.5f, 2f));
    }
    double EvalPiecePositions(Board board)
    {
        double result = 0;
        ulong piecesBitboard = board.AllPiecesBitboard;
        int numberOfPieces = BitboardHelper.GetNumberOfSetBits(piecesBitboard);
        while (piecesBitboard > 0)
        {
            int isWhite = 1;
            Square currentSquare = new Square(BitboardHelper.ClearAndGetIndexOfLSB(ref piecesBitboard));
            Piece currentPiece = board.GetPiece(currentSquare);
            if (!currentPiece.IsWhite)
            {
                isWhite = -1;
            }
            if (currentPiece.IsKing)
            {
                result += DistanceFromCentre(currentSquare) * isWhite * 100 / board.PlyCount;
            }
            if (currentPiece.IsKnight)
            {
                result += DistanceFromCentre(currentSquare) * isWhite;
            }

        }
        return result;
    }
    public double Eval(Board board, bool white)
    {
        double result = EvalMaterial(board, white);


        if (!white)
        {
            result *= -1;
        }
        return result;
    }


}