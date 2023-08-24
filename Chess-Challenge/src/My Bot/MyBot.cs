﻿using ChessChallenge.API;
using ChessChallenge.Application;
using System;
using System.Collections.Generic;

namespace ChessChallenge.Example;
public class MyBot : IChessBot
{
    private int _searched = 0;
    private int _depth = 5; // Set the depth you want the bot to evaluate
    private Board board;
    private bool _white;

    public Move Think(Board board, Timer timer)
    {
        
        this.board = board;
        _white = board.IsWhiteToMove;
        Move[] moves = OrderMoves();
        Move bestMove = new Move();
        
        double temp = 0;
        _searched = 0;
        double alpha = double.NegativeInfinity, beta = double.PositiveInfinity;
        double result = double.NegativeInfinity;

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            _searched++;
            temp = -OrderABNegaMax(_depth-1, board.IsWhiteToMove,
                                    -beta, -alpha);
            board.UndoMove(move);
            
            if(temp > result)
            {
                result = temp;
                bestMove = move;
            }
            alpha = Math.Max(result, alpha);
            if (alpha >= beta)
            {
                break;
            }
        }

        ConsoleHelper.Log($"Playing {board.IsWhiteToMove}, making move {bestMove} with eval found {result}, last temp {temp}", false, ConsoleColor.White);
        ConsoleHelper.Log($"Searched {_searched}", false, ConsoleColor.Red);

        return bestMove;
    }

    double OrderABNegaMax(int depth, bool maximizing, double alpha, double beta)
    {
        Move[] moves = OrderMoves();
        if (depth <= 0 || board.IsDraw() || board.IsInCheckmate())
        {
            if (board.IsDraw())
            {
                if (board.PlyCount > 34)
                {
                    return 0;
                }
                else
                {
                    //Pokud evaluujeme z pohledu bileho,
                    //tak predpokladame, ze cerny nas chce navest do remizy, tedy
                    //ze je to pro nej vyhra
                    if (maximizing)
                    {
                        return 2000;
                    }
                    else
                    {
                        return -2000;
                    }
                }
            }
            else if (board.IsInCheckmate())
            {
                //Pokud evaluujeme z pohledu bileho,
                //tak udelal finishing move cerny, a tedy vyhral

                return maximizing ? - 10000000 + 100 * (_depth - depth) : 10000000 - 100 * (_depth - depth);
            }
            return maximizing ? Eval() : -Eval();
        }

        double result = double.NegativeInfinity;

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            _searched++;
            result = Math.Max(
                result,
                -OrderABNegaMax(depth - 1, !maximizing,
                                -beta, -alpha)
                );
            board.UndoMove(move);
            alpha = Math.Max(result, alpha);
            if (alpha >= beta)
            {
                break;
            }
        }
        return result;


    }
    Move[] OrderMoves()
    {
        Move[] otherMoves = board.GetLegalMoves();
        Move[] moves = new Move[otherMoves.Length];
        int captures = board.GetLegalMoves(true).Length;
        int seen_captures = 0;

        for (int i = 0; i < otherMoves.Length; i++)
        {
            if (otherMoves[i].IsCapture)
            {
                moves[seen_captures] = otherMoves[i];
                seen_captures++;
            }
            else
            {
                moves[captures + i - seen_captures] = otherMoves[i];
            }
        }

        return moves;
    }
    int EvalMaterial()
    {
        int P = board.GetPieceList(PieceType.Pawn, true).Count - board.GetPieceList(PieceType.Pawn, false).Count;
        int N = board.GetPieceList(PieceType.Knight, true).Count - board.GetPieceList(PieceType.Knight, false).Count;
        int B = board.GetPieceList(PieceType.Bishop, true).Count - board.GetPieceList(PieceType.Bishop, false).Count;
        int R = board.GetPieceList(PieceType.Rook, true).Count - board.GetPieceList(PieceType.Rook, false).Count;
        int Q = board.GetPieceList(PieceType.Queen, true).Count - board.GetPieceList(PieceType.Queen, false).Count;
        int K = board.GetPieceList(PieceType.King, true).Count - board.GetPieceList(PieceType.King, false).Count;

        //Multiply the material differences by their respective weights.
        int result = (900 * Q) +
            (500 * R) +
            (300 * N) + (300 * B) +
            (100 * P) + (3100 * K);

        return result;
    }
    double AttackedSqares()
    {
        int result = 0;
        ulong piecesBitboard = board.AllPiecesBitboard;
        while (piecesBitboard > 0)
        {
            int white = 1;
            Square currentSquare = new Square(BitboardHelper.ClearAndGetIndexOfLSB(ref piecesBitboard));
            Piece currentPiece = board.GetPiece(currentSquare);
            if (!currentPiece.IsWhite)
            {
                white = -1;
            }
            result += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPieceAttacks(currentPiece.PieceType, currentSquare, board, currentPiece.IsWhite)) * white;
        }
        return result;
    }
    double PiecesPositionEval()
    {
        double result = 0;
        ulong piecesBitboard = board.AllPiecesBitboard;
        while (piecesBitboard > 0)
        {
            int white = 1;
            Square currentSquare = new Square(BitboardHelper.ClearAndGetIndexOfLSB(ref piecesBitboard));
            Piece currentPiece = board.GetPiece(currentSquare);
            if (!currentPiece.IsWhite)
            {
                white = -1;
            }
            if (currentPiece.IsKing)
            {
                result += Math.Sqrt((currentSquare.File - 3.5) * (currentSquare.File - 3.5) + (currentSquare.Rank - 3.5) * (currentSquare.Rank - 3.5)) * white;
            }
        }
        return result;
    }
    public double Eval()
    {
        return EvalMaterial() + AttackedSqares() * 5 + PiecesPositionEval();
    }

}

