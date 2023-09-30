using ChessChallenge.API;
using ChessChallenge.Application;
using System;
using System.Collections.Generic;


namespace ChessChallenge.Example;
public class NIWTFWD_v5_6 : IChessBot
{
    //actually NIWTFWD_v5_6-

    private int _searched = 0;
    // Set the depth you want the bot to evaluate
    private int _depth = 5;
    private int _max_depth = 512;
    private int _transposition_depth = 0;
    private readonly ulong[,] _positionalWeights = 
    {
        {8683547591809433600, 1388309562400055824, 3915129336154501941, 384307146728703573, 2781419601154818592, 10986381248880408745},
        {6150110960405533511, 2623195334416164162, 3784827174736258659, 384307145385268560, 7393108935958960484, 7224376637290534231},
        {8613882468073314150, 2618391568115529282, 3771313077301437779, 384307145385268560, 2781422900838242664, 4841373122423685940},
        {4294967295, 81665195684152369, 230584288054363203, 8608480568053246632, 308641985531897444, 3611889320058683955}
    };

    private Board board;
    private int _maximizing;
    private Move _bestMove;
    private int reduceDepth = 0;
    public struct Transposition
    {
        public ulong zobristHash;
        public Move move;
        public int evaluation;
        public sbyte depth;
        public byte flag;
    };

    Transposition[] m_TPTable;

    public NIWTFWD_v5_6()
    {
        m_TPTable = new Transposition[0x800000];
    }

    public Move Think(Board board, Timer timer)
    {

        if (reduceDepth == 2 && timer.MillisecondsRemaining < 8000 && timer.OpponentMillisecondsRemaining > timer.MillisecondsRemaining)
        {
            reduceDepth++;
            _max_depth = 128;
            _depth--;
        }
        this.board = board;
        _maximizing = board.IsWhiteToMove ? 1 : -1;
        _searched = 0;

        int alpha = -80000000, beta = 80000000;

        DateTime t = DateTime.Now;
        _transposition_depth = 0;
        int result = TPTOrderABNegaMax(_depth, _maximizing, alpha, beta);
        ref Transposition tran = ref m_TPTable[board.ZobristKey & 0x7FFFFF];
        ConsoleHelper.Log($"Playing {board.IsWhiteToMove} making move {_bestMove} with eval {tran.evaluation} material {Eval()}", false, ConsoleColor.White);
        ConsoleHelper.Log($"TransposedABNega searched {_searched} in {(int)(DateTime.Now - t).TotalMilliseconds} ms", false, ConsoleColor.Blue);
        if (reduceDepth==0)
        {
            _max_depth = 256;
            reduceDepth++;
        }
        return _bestMove;
    }
    /*
     
     Recursion
     
     */

    // Negamax algorithm with alpha-beta pruning
    int TPTOrderABNegaMax(int depth, int maximizing, int alpha, int beta)
    {
        int startAlpha = alpha;
        Move bestMove = Move.NullMove;
        ref Transposition transposition = ref m_TPTable[board.ZobristKey & 0x7FFFFF];
        if (transposition.zobristHash == board.ZobristKey && transposition.depth >= depth)
        {
            ref int TPTeval = ref transposition.evaluation;
            //If we have an "exact" score (a < score < beta) just use that
            if (transposition.flag == 1)
            {
                if (_max_depth >= _transposition_depth)
                {
                    //ConsoleHelper.Log($"Jsem v chainu: {_transposition_depth}", false, ConsoleColor.Yellow);
                    _transposition_depth++;
                    Move move = transposition.move;
                    board.MakeMove(move);
                    TPTeval = Math.Max(TPTeval, -TPTOrderABNegaMax(depth, -maximizing, -beta, -alpha));
                    board.UndoMove(move);
                }

                if (_depth == depth)
                {
                    _bestMove = transposition.move;
                }
                return TPTeval;

            }
            else if (transposition.flag == 2)
            {

                alpha = Math.Max(alpha, TPTeval);
            }
            else if (transposition.flag == 3)
            {
                beta = Math.Min(beta, TPTeval);
            }
            if (alpha >= beta)
            {
                if (_depth == depth)
                {
                    _bestMove = transposition.move;
                }
                return TPTeval;
            }
        }
        if (depth <= 0 || board.IsDraw() || board.IsInCheckmate())
        {
            if (board.IsDraw())
            {
                if (board.PlyCount > 28)
                {
                    return 0;
                }
                else
                {
                    //Pokud evaluujeme z pohledu bileho,
                    //tak predpokladame, ze cerny nas chce navest do remizy, tedy
                    //ze je to pro nej vyhra
                    return 2000;
                }
            }
            else if (board.IsInCheckmate())
            {
                //Pokud evaluujeme z pohledu bileho,
                //tak udelal finishing move cerny, a tedy vyhral

                return board.PlyCount - 5000000;
            }
            return maximizing * Eval();
        }

        Move[] moves = OrderMoves();

        int result = int.MinValue / 2;
        int temp;
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            _searched++;
            temp = -TPTOrderABNegaMax(depth - 1, -maximizing, -beta, -alpha);
            board.UndoMove(move);

            if (temp > result)
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

        transposition.evaluation = result;
        transposition.zobristHash = board.ZobristKey;
        transposition.move = bestMove;

        if (result <= startAlpha)
        {
            transposition.flag = 3;
        }
        else if (result >= beta)
        {
            transposition.flag = 2;
        }
        else
        {
            transposition.flag = 1;
        }
        transposition.depth = (sbyte)depth;
        if (_depth == depth)
        {
            _bestMove = bestMove;
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

    /*void OrderMoves(ref Move[] moves)
    {
        int[] sortmoves = new int[moves.Length];
        ref Transposition transposition = ref m_TPTable[board.ZobristKey & 0x7FFFFF];
        for (int i = 0; i < moves.Length; i++)
        {
            Move move = moves[i];
            board.MakeMove(move);
            sortmoves[i] = 0;
            if (move.Equals(transposition.move))
            {
                sortmoves[i] = 1000;
            }
            else if (board.IsDraw())
            {
                sortmoves[i] = 10;
            }
            else if (move.IsCapture || move.IsCastles || board.IsInCheck())
            {
                sortmoves[i] = 100;
            }
            board.UndoMove(move);
        }
        Array.Sort(sortmoves, moves);
    }*/

    /* 
    Evaluation
    */



    int Eval()
    {
        int result = 0;
        ulong piecesBitboard = board.AllPiecesBitboard;
        while (piecesBitboard > 0)
        {
            int white = 1;
            int blackShift = 0;
            Square currentSquare = new Square(BitboardHelper.ClearAndGetIndexOfLSB(ref piecesBitboard));
            Piece currentPiece = board.GetPiece(currentSquare);
            if (!currentPiece.IsWhite)
            {
                white = -1;
                blackShift = 7;
            }
            
            result += 
                (((int)(_positionalWeights[(int)(white * (currentSquare.Rank - blackShift) / 2), (int)currentPiece.PieceType - 1] >> (4 * currentSquare.File + (white * (currentSquare.Rank - blackShift)) % 2 * 32)) & 15)
                + BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPieceAttacks(currentPiece.PieceType, currentSquare, board, currentPiece.IsWhite))
                + PieceTypeValue(currentPiece.PieceType)) * white;

        }
        return result * 5;
    }
    int PieceTypeValue(PieceType pieceType)
    {
        switch ((int)pieceType) {
            case 1: return 20;
            case 2: return 60;
            case 3: return 60;
            case 4: return 100;
            case 5: return 180;
            default: return 620;
        }
    }
}

