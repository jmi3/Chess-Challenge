using ChessChallenge.API;
using ChessChallenge.Application;
using System;

namespace WeightTables
{
    public class Compiler
    {

        public ulong[,] compiledWeights =
        {
        {8683547591809433600, 0, 0, 0, 0, 0},
        {6150110960405533511, 0, 0, 0, 0, 0},
        {8613882468073314150, 0, 0, 0, 0, 0},
        {4294967295, 0, 0, 0, 0, 0}
        };

        byte[,] weightsTable =
        {
            //{ a,  b,  c,  d,  e,  f,  g,  h}
        /*1*/  { 0,  0,  0,  0,  0,  0,  0,  0},
        /*2*/  { 7,  8,  8,  2,  2,  8,  8,  7},
        /*3*/  { 7,  4,  3,  5,  5,  3,  4,  7},
        /*4*/  { 5,  5,  5,  9,  9,  5,  5,  5},
        /*5*/  { 6,  6,  7,  9,  9,  7,  6,  6},
        /*6*/  { 7,  7,  8, 10, 10,  8,  7,  7},
        /*7*/  {15, 15, 15, 15, 15, 15, 15, 15},
        /*8*/  { 0,  0,  0,  0,  0,  0,  0,  0}
        };

        public void Compile()
        {
            //Only the first 4 bits of these numbers get encoded, (only 0 to 15) 

            for (int i = 0; i < 4; i++)
            {
                ulong result = 0;
                for (int row = 0; row < 2; row++)
                {
                    for (int col = 0; col < 8; col++)
                    {
                        result |= ((ulong)(weightsTable[row + 2 * i, col]) << (4 * col + 32 * row));

                    }
                }
                Console.WriteLine(result);
            }
        }

        public void Decompile()
        {

            Console.WriteLine("Pieces: Pawn = 1, Knight = 2, Bishop = 3, Rook = 4, Queen = 5, King = 6");

            for (int pieceType = 1; pieceType <= 6; pieceType++)
            {
                Console.WriteLine($"Weight Table for a {pieceType} is:");
                for (int Rank = 0; Rank < 8; Rank++)
                {
                    Console.Write("{");
                    for (int file = 0; file < 8; file++)
                    {
                        if (file != 0)
                        {
                            Console.Write(",");
                        }
                        Console.Write($"{SquareWeight(1, file, Rank)}");
                    }
                    Console.Write("},");
                    Console.WriteLine();

                }
            }

        }
        private int SquareWeight(int pieceType, int File, int Rank)
        {
            ulong encodedData = compiledWeights[(int)(Rank / 2), (int)pieceType - 1];
            encodedData = encodedData >> (4 * File + Rank % 2 * 32);

            return (int)encodedData & 15;

        }

    }
}