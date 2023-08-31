using ChessChallenge.API;
using ChessChallenge.Application;
using System;

namespace ChessChallenge.Example;
public class NIWTFWD_v2_6 : IChessBot
{
    private int _searched = 0;
    // Set the depth you want the bot to evaluate
    private int _depth = 7;     
    private int _max_depth = int.MaxValue;     
    private int _transposition_depth = 0;     
    
    private Board board;
    private int _maximizing;
    private Move _bestMove;
    private bool reduceDepth = true;
    public struct Transposition
    {
        public ulong zobristHash;
        public Move move;
        public int evaluation;
        public sbyte depth;
        public byte flag;
    };

    Transposition[] TPTable;

    public NIWTFWD_v2_6()
    {
        TPTable = new Transposition[0x800000];
    }

    public Move Think(Board board, Timer timer)
    {
        //if (!reduceDepth && timer.MillisecondsRemaining < timer.GameStartTimeMilliseconds / 4 && timer.OpponentMillisecondsRemaining > timer.MillisecondsRemaining)
        
        this.board = board;
        _maximizing = board.IsWhiteToMove ? 1 : -1;
        _searched = 0;
        
        int alpha = int.MinValue / 3, beta = int.MaxValue / 3;
        
        DateTime t = DateTime.Now;
        _transposition_depth = 0;
        int result = TPTOrderABNegaMax(_depth, _maximizing, alpha, beta);
        ref Transposition tran = ref TPTable[board.ZobristKey & 0x7FFFFF];
        
        ConsoleHelper.Log($"Playing {board.IsWhiteToMove}, making move {_bestMove} with eval {tran.evaluation} material {EvalMaterial()}", false, ConsoleColor.White);
        ConsoleHelper.Log($"TransposedABNega searched {_searched} in {(int)(DateTime.Now - t).TotalMilliseconds} ms", false, ConsoleColor.Blue);
        if (reduceDepth)
        {
            _max_depth = 256;
            reduceDepth = false;
            _depth = _depth - 2;
        }
        return _bestMove;
    }
    
    // Negamax algorithm with alpha-beta pruning
    int TPTOrderABNegaMax(int depth, int maximizing, int alpha, int beta)
    {
        int startAlpha = alpha;
        Move bestMove = Move.NullMove;
        ref Transposition transposition = ref TPTable[board.ZobristKey & 0x7FFFFF];
        if (transposition.zobristHash == board.ZobristKey && transposition.depth >= depth)
        {
            ref int TPTeval = ref transposition.evaluation;
            //If we have an "exact" score (a < score < beta) just use that
            if (transposition.flag == 1)
            {
                if (_max_depth >= _transposition_depth)
                {
                    ConsoleHelper.Log($"Jsem v chainu: {_transposition_depth}", false, ConsoleColor.Yellow);
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
                if (board.PlyCount > 34)
                {
                    return 0;
                }
                else
                {
                    //Pokud evaluujeme z pohledu bileho,
                    //tak predpokladame, ze cerny nas chce navest do remizy, tedy
                    //ze je to pro nej vyhra
                    return maximizing * (-2000);
                }
            }
            else if (board.IsInCheckmate())
            {
                //Pokud evaluujeme z pohledu bileho,
                //tak udelal finishing move cerny, a tedy vyhral

                return maximizing * (1000000 - 100 * (_depth - depth));
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
        transposition.depth = (sbyte) depth;
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
    int AttackedSqares()
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
    public int Eval()
    {
        return EvalMaterial() + AttackedSqares() * 5 + (int)PiecesPositionEval();
    }

}

