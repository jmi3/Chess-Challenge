using ChessChallenge.API;
using ChessChallenge.Application; //DELETE
using System;
// for List
using System.Collections.Generic;
// for ToList
using System.Linq;

namespace Bots;

public class AlphaBetaBot : IChessBot
{
    // Simple alpha-beta search
    private float AlphaBetaSearch( Board board
                                 , int depth
                                 , float alpha
                                 , float beta
                                 , bool maximising
                                 ) {
        // If we're out of depth or game over, stop
        if (depth == 0 || board.IsInCheckmate()) {
            return Eval(board);
        }


        // Order the moves by their ranking, taking care that we're looking
        // ahead here, so the `MEval` sign needs to be opposite

        List<Move> moves = board.GetLegalMoves().ToList().OrderByDescending(m => (maximising ? 1 : -1) * MEval(m)).ToList();
        float bestEval = maximising ? float.NegativeInfinity : float.PositiveInfinity;

        foreach (Move nextMove in moves) {
            // make the move
            board.MakeMove(nextMove);

            // recurse on the new state with 1 less depth, and !­­maximising
            float currEval;
            currEval = AlphaBetaSearch(board, depth - 1, -alpha, -beta, !maximising);

            // keep the new evaluation iff it is better
            if (maximising) {
                bestEval = currEval > bestEval ? currEval : bestEval;
                // keep track of global maximum
                alpha = alpha > currEval ? alpha : currEval;
                if (beta <= alpha) {
                    board.UndoMove(nextMove);
                    break;
                }
            }
            else {
                bestEval = currEval < bestEval ? currEval : bestEval;
                // keep track of global minimum
                beta = beta < currEval ? beta : currEval;
                if (beta <= alpha) {
                    board.UndoMove(nextMove);
                    break;
                }
            }

            // remember to undo our move
            board.UndoMove(nextMove);
        }

        // return the evaluation we found
        return bestEval;
    }

    public float AlphaBetaSearch2(Board board, int depth, float alpha, float beta, bool maximising) {
        if (depth == 0 || board.IsInCheckmate()) {
            return Eval(board);
        }

        // Ranked list of moves
        List<Move> moves = board.GetLegalMoves().ToList().OrderByDescending(m => (maximising ? 1 : -1) * MEval(m)).ToList();

        // Starting point
        float localAlpha = alpha;
        float localBeta = beta;
        float bestScore = float.NaN;

        if (maximising) {
            bestScore = float.NegativeInfinity;
            foreach (Move futureMove in moves) {
                // set up hypothetical state
                board.MakeMove(futureMove);

                float eval = AlphaBetaSearch2(board, depth - 1, localAlpha, localBeta, !maximising);
                bestScore = eval > bestScore ? eval : bestScore;
                if (bestScore > localBeta) {
                    // remember to restore previous state
                    board.UndoMove(futureMove);
                    break;
                }
                // keep track of search maximum
                localAlpha = bestScore > localAlpha ? bestScore : localAlpha;
                // remember to restore previous state
                board.UndoMove(futureMove);
            }
            return bestScore;
        }
        else {
            bestScore = float.PositiveInfinity;
            foreach(Move futureMove in moves) {
                // set up hypothetical state
                board.MakeMove(futureMove);

                float eval = AlphaBetaSearch2(board, depth - 1, localAlpha, localBeta, !maximising);
                bestScore = eval < bestScore ? eval : bestScore;
                if (bestScore < localAlpha) {
                    // remember to restore previous state
                    board.UndoMove(futureMove);
                    break;
                }
                // keep track of search minimum
                localBeta = bestScore < localBeta ? bestScore : localBeta;
                // remember to restore previous state
                board.UndoMove(futureMove);
            }
        }

        return bestScore;
    }

    public Move Think(Board board, Timer timer)
    {
        // maximising depends on which side we're playing
        bool maximising = board.IsWhiteToMove;

        // Depth for Alpha-Beta exploration
        int searchDepth = 3;

        // Ranked list of moves
        List<Move> moves = board.GetLegalMoves().ToList().OrderByDescending(m => (maximising ? 1 : -1) * MEval(m)).ToList();

        // Init with nonsense (makes debugging obvious (I hope))
        Move bestMove = Move.NullMove;
        float bestEval = float.NaN;

        // search the moves
        foreach (Move currMove in moves) {
            board.MakeMove(currMove);
            // we've made our move, so start by eval.ng opponent's pos.n
            float currEval = AlphaBetaSearch2( board
                                             , searchDepth
                                             , float.NegativeInfinity
                                             , float.PositiveInfinity
                                             , maximising
            );

            // We need to start somewhere
            if (float.IsNaN(bestEval)) {
                bestEval = currEval;
                bestMove = currMove;
            }

            // Rest proceeds as expected
            if (maximising && currEval > bestEval) {
                bestEval = currEval;
                bestMove = currMove;
            }
            else if (currEval < bestEval) {
                bestEval = currEval;
                bestMove = currMove;
            }

            // remember to undo
            board.UndoMove(currMove);
        }

        ConsoleHelper.Log("AlphaBetaBot chose " + bestMove.ToString() + " with eval.n: " + bestEval.ToString());
        return bestMove;
    }

    // Simple move ranking function.
    // TODO: Make this better.
    private int MEval(Move move) {
        return 1 + (move.IsCapture ? 8 : 0) + (move.IsPromotion ? 4 : 0) + (move.IsCastles ? 2 : 0);
    }

    // Rudimentary evaluation function as proposed here
    // https://www.chessprogramming.org/Evaluation
    private float Eval(Board board)
    {
        // if we've won, that's the best possible outcome
        if (board.IsInCheckmate()) {
            return board.IsWhiteToMove ? float.NegativeInfinity : float.PositiveInfinity;
        }

        int P = board.GetPieceList(PieceType.Pawn, true).Count - board.GetPieceList(PieceType.Pawn, false).Count;
        int N = board.GetPieceList(PieceType.Knight, true).Count - board.GetPieceList(PieceType.Knight, false).Count;
        int B = board.GetPieceList(PieceType.Bishop, true).Count - board.GetPieceList(PieceType.Bishop, false).Count;
        int R = board.GetPieceList(PieceType.Rook, true).Count - board.GetPieceList(PieceType.Rook, false).Count;
        int Q = board.GetPieceList(PieceType.Queen, true).Count - board.GetPieceList(PieceType.Queen, false).Count;

        //Multiply the material differences by their respective weights.
        float result = (9 * Q) + 
                       (5 * R) + 
                       (3 * (B + N)) + 
                       (1 * P);

        if(!board.IsWhiteToMove)
        {
            result *= -1; //Adjusting result for when playing the black pieces.
        }

        return result;
    }
}
