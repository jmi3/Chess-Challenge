using ChessChallenge.API;
using ChessChallenge.Application;
using System;
using System.Collections.Generic;
namespace ChessChallenge.Example;
public class EvilBot : IChessBot
{
    // public Dictionary<ulong, Tuple<Move, float>> searchedPositions = new Dictionary<ulong, Tuple<Move, float>>();

    public Move Think(Board board, Timer timer)
    {
        Console.WriteLine(board.IsWhiteToMove);
        /*
        Weights go like this: 0: Material, 1: Controlled squares, 2: Number of possible capture moves, 3: Capture move, 4: Castle move, 5: Promotion move
        6: Moving king or any rook when Castling is possible, 7: dont go to places where you will be attacked
        */
        Random rnd = new Random();
        List<float> Weighths = new List<float> { 9f, 1f, 2f, 9f, 5f, 5f, -6f, -10f };
        Move moveToDo = FindMove(board, 3, Weighths, float.PositiveInfinity, timer).bestMove;
        if (moveToDo == Move.NullMove || !MoveIsValid(board, moveToDo))
        {
            moveToDo = board.GetLegalMoves()[rnd.Next(board.GetLegalMoves().Length)];
            Console.WriteLine("Move was None or Invalid, chosing random" + moveToDo);
        }
        Console.WriteLine(board.IsWhiteToMove);
        return moveToDo;
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


    float CurrentValue(Board board)
    {
        int Q = board.GetPieceList(PieceType.Queen, true).Count - board.GetPieceList(PieceType.Queen, false).Count;
        int B = board.GetPieceList(PieceType.Bishop, true).Count - board.GetPieceList(PieceType.Bishop, false).Count;
        int N = board.GetPieceList(PieceType.Knight, true).Count - board.GetPieceList(PieceType.Knight, false).Count;
        int R = board.GetPieceList(PieceType.Rook, true).Count - board.GetPieceList(PieceType.Rook, false).Count;
        int K = board.GetPieceList(PieceType.King, true).Count - board.GetPieceList(PieceType.King, false).Count;

        int value = (12 * K + Q * 9 + R * 5 + (B + N) * 3 + board.GetPieceList(PieceType.Pawn, true).Count - board.GetPieceList(PieceType.Pawn, false).Count);
        if (!board.IsWhiteToMove) { value *= (-1); }
        return value;
    }
    ulong GetAttacks(PieceType pieceType, Square square, Board board, bool isWhite)
    {
        switch (pieceType)
        {
            case PieceType.Rook: return BitboardHelper.GetSliderAttacks(PieceType.Rook, square, board);
            case PieceType.Bishop: return BitboardHelper.GetSliderAttacks(PieceType.Bishop, square, board);
            case PieceType.Queen: return BitboardHelper.GetSliderAttacks(PieceType.Queen, square, board);
            case PieceType.Knight: return BitboardHelper.GetKnightAttacks(square);
            case PieceType.King: return BitboardHelper.GetKingAttacks(square);
            case PieceType.Pawn: return BitboardHelper.GetPawnAttacks(square, isWhite);
            default: return 0;
        }
    }
    // Returns (number of attacked squares, my value that is currently being attacked)
    (int, int) AttackedSquares(Board board, bool isWhite)
    {
        int valueGettingAttacked = 0;
        ulong piecesBitboard;
        ulong attackedBitboard = 0;
        if (isWhite)
        {
            piecesBitboard = board.WhitePiecesBitboard;
        }
        else
        {
            piecesBitboard = board.BlackPiecesBitboard;
        }
        while (piecesBitboard != 0)
        {
            Square currentSquare = new Square(BitboardHelper.ClearAndGetIndexOfLSB(ref piecesBitboard));
            Piece currentPiece = board.GetPiece(currentSquare);
            if (board.SquareIsAttackedByOpponent(currentSquare))
            {
                valueGettingAttacked += PieceValue(currentPiece.PieceType);
            }
            attackedBitboard |= GetAttacks(currentPiece.PieceType, currentSquare, board, isWhite);

        }
        return (BitboardHelper.GetNumberOfSetBits(attackedBitboard), valueGettingAttacked);
    }

    float BoardScore(Board board, List<float> weights)
    {

        int captureCount = 0;
        while (captureCount < board.GetLegalMoves(true).Length)
        {
            captureCount++;
        }
        return weights[0] * CurrentValue(board) + weights[1] * AttackedSquares(board, board.IsWhiteToMove).Item1 + weights[2] * captureCount;
    }
    float MoveScore(Board board, List<float> weights, Move move)
    {

        float currentBoardScore = BoardScore(board, weights);
        board.MakeMove(move);
        if (!board.IsDraw()) { board.UndoMove(move); return 0; }
        if (board.IsInCheckmate()) { board.UndoMove(move); return float.PositiveInfinity; }
        float moveScore = (-1) * BoardScore(board, weights) - currentBoardScore;
        board.UndoMove(move);

        if (move.IsCapture)
        {
            moveScore += weights[3] * (12 + PieceValue(move.CapturePieceType) - PieceValue(move.MovePieceType));
        }
        if (move.IsCastles)
        {
            moveScore += weights[4];
        }
        if (move.IsPromotion)
        {
            moveScore += weights[5] * PieceValue(move.PromotionPieceType);
        }
        if ((move.MovePieceType == PieceType.King || move.MovePieceType == PieceType.Rook) && (board.HasKingsideCastleRight(board.IsWhiteToMove) || board.HasQueensideCastleRight(board.IsWhiteToMove)))
        {
            moveScore += weights[6];
        }
        if (board.SquareIsAttackedByOpponent(move.TargetSquare))
        {
            moveScore += weights[7] * PieceValue(move.MovePieceType);
        }
        return moveScore;
    }
    int PieceValue(PieceType pieceType)
    {
        switch (pieceType)
        {
            case PieceType.King: return 12;
            case PieceType.Pawn: return 1;
            case PieceType.Bishop: return 3;
            case PieceType.Knight: return 3;
            case PieceType.Rook: return 5;
            case PieceType.Queen: return 9;
            default: return 0;
        }
    }

    (Move bestMove, float bestScore) FindMove(Board board, int depth, List<float> weights, float upperLayerBest, Timer timer)
    {
        Move bestMove = Move.NullMove;
        float bestScore = float.NegativeInfinity;
        /*   ulong key = board.ZobristKey;
           if (searchedPositions.ContainsKey(key))
           {
               return (searchedPositions[key].Item1, searchedPositions[key].Item2);
           }
           else */
        {
            List<Move> movesOrder = new List<Move>(board.GetLegalMoves(true));
            foreach (Move move in board.GetLegalMoves())
            {
                if (!movesOrder.Contains(move))
                {
                    movesOrder.Add(move);
                }
            }
            foreach (Move move in movesOrder)
            {
                /*if (timer.MillisecondsElapsedThisTurn > 5000 || timer.MillisecondsRemaining < 1000)
                    break;*/
                float currentScore;
                if (depth == 0)
                {
                    currentScore = MoveScore(board, weights, move);
                }
                else
                {
                    board.MakeMove(move);
                    currentScore = (-1) * FindMove(board, depth - 1, weights, -bestScore, timer).bestScore;
                    board.UndoMove(move);
                }
                /*                if (currentScore > upperLayerBest && movesOrder.IndexOf(move) > 0)
                                    break;*/
                if (currentScore > bestScore)
                {
                    bestScore = currentScore;
                    bestMove = move;
                }
            }
            /* if (!searchedPositions.ContainsKey(key))
                searchedPositions.Add(key, new Tuple<Move, float>(bestMove, bestScore)); */
        }
        return (bestMove, bestScore);

    }

}