﻿using ChessSharp.Pieces;
using ChessSharp.SquareData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessSharp
{
    /// <summary>
    /// A static class containing helper methods.
    /// </summary>
    public static class ChessUtilities
    {
        internal static Player RevertPlayer(Player player)
        {
            return player == Player.White ? Player.Black : Player.White;
        }

        internal static GameState GetGameState(GameBoard board)
        {
            Player opponent = board.WhoseTurn();
            Player lastPlayer = RevertPlayer(opponent);
            bool hasValidMoves = GetValidMoves(board).Count > 0;
            bool isInCheck = IsPlayerInCheck(opponent, board);

            if (isInCheck && !hasValidMoves)
            {
                return lastPlayer == Player.White ? GameState.WhiteWinner : GameState.BlackWinner;
            }

            if (!hasValidMoves)
            {
                return GameState.Stalemate;
            }

            if (isInCheck)
            {
                return opponent == Player.White ? GameState.WhiteInCheck : GameState.BlackInCheck;
            }

            return IsInsufficientMaterial(board.Board) ? GameState.Draw : GameState.NotCompleted;
        }

        /* TODO: Still not sure where to implement it, but I may need methods:
           TODO: bool CanClaimDraw + bool ClaimDraw + OfferDraw
        */

        internal static bool IsInsufficientMaterial(Piece[,] board)
        {
            Piece[] pieces = board.Cast<Piece>().ToArray();
            var whitePieces = pieces.Select((p, i) => new { Piece = p, SquareColor = (i % 8 + i / 8) % 2 }).Where(p => p.Piece != null && p.Piece.Owner == Player.White).ToArray();
            var blackPieces = pieces.Select((p, i) => new { Piece = p, SquareColor = (i % 8 + i / 8) % 2 }).Where(p => p.Piece != null && p.Piece.Owner == Player.Black).ToArray();

            if (whitePieces.Length == 1 && blackPieces.Length == 1) // King vs King
            {
                return true;
            }


            if (whitePieces.Length == 1 && blackPieces.Length == 2 &&
                blackPieces.Any(p => p.Piece is Bishop ||
                                     p.Piece is Knight)) // White King vs black king and (Bishop|Knight)
            {
                return true;
            }

            if (whitePieces.Length == 2 && blackPieces.Length == 1 &&
                whitePieces.Any(p => p.Piece is Bishop ||
                                     p.Piece is Knight)) // Black King vs white king and (Bishop|Knight)
            {
                return true;
            }

            if (whitePieces.Length == 2 && blackPieces.Length == 2) // King and bishop vs king and bishop
            {
                var whiteBishop = whitePieces.First(p => p.Piece is Bishop);
                var blackBishop = blackPieces.First(p => p.Piece is Bishop);
                return whiteBishop != null && blackBishop != null &&
                       whiteBishop.SquareColor == blackBishop.SquareColor;
            }
            return false;
        }

        /// <summary>Gets the valid moves of the given <see cref="GameBoard"/>.</summary>
        /// <param name="board">The <see cref="GameBoard"/> that you want to get its valid moves.</param>
        /// <returns>Returns a list of the valid moves.</returns>
        public static List<Move> GetValidMoves(GameBoard board)
        {
            var player = board.WhoseTurn();
            var validMoves = new List<Move>();
            Square[] squares = new[]
            {
                "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8",
                "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8",
                "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8",
                "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8",
                "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8",
                "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8",
                "G1", "G2", "G3", "G4", "G5", "G6", "G7", "G8",
                "H1", "H2", "H3", "H4", "H5", "H6", "H7", "H8",
            }.Select(Square.Parse).ToArray();

            var playerOwnedSquares = squares.Where(sq => board[sq] != null &&
                                                         board[sq].Owner == player).ToArray();
            var nonPlayerOwnedSquares = squares.Where(sq => board[sq] == null ||
                                                            board[sq].Owner != player).ToArray();

            foreach (var playerOwnedSquare in playerOwnedSquares)
            {
                validMoves.AddRange(nonPlayerOwnedSquares
                    .Select(nonPlayerOwnedSquare => new Move(playerOwnedSquare, nonPlayerOwnedSquare, player))
                    .Where(move => GameBoard.IsValidMove(move, board)));
            }

            return validMoves;
        }

        /// <summary>Gets the valid moves of the given <see cref="GameBoard"/> that has a specific given source <see cref="Square"/>.</summary>
        /// <param name="source">The source <see cref="Square"/> that you're looking for its valid moves.</param>
        /// <param name="board">The <see cref="GameBoard"/> that you want to get its valid moves from the specified square.</param>
        /// <returns>Returns a list of the valid moves that has the given source square.</returns>
        /// 
        public static List<Move> GetValidMovesOfSourceSquare(Square source, GameBoard board)
        {
            var validMoves = new List<Move>();
            var piece = board[source];
            if (piece == null)
            {
                return validMoves;
            }

            Square[] squares = new[]
            {
                "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8",
                "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8",
                "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8",
                "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8",
                "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8",
                "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8",
                "G1", "G2", "G3", "G4", "G5", "G6", "G7", "G8",
                "H1", "H2", "H3", "H4", "H5", "H6", "H7", "H8",
            }.Select(Square.Parse).ToArray();

            var player = piece.Owner;
            var nonPlayerOwnedSquares = squares.Where(sq => board[sq] == null ||
                                                            board[sq].Owner != player).ToArray();


            validMoves.AddRange(nonPlayerOwnedSquares
                .Select(nonPlayerOwnedSquare => new Move(source, nonPlayerOwnedSquare, player, PawnPromotion.Queen)) // If promoteTo is null, valid pawn promotion will cause exception. Need to implement this better and cleaner in the future.
                .Where(move => GameBoard.IsValidMove(move, board)));
            return validMoves;
        }

        internal static bool IsPlayerInCheck(Player player, GameBoard board)
        {
            Square[] squares = new[]
            {
                "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8",
                "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8",
                "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8",
                "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8",
                "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8",
                "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8",
                "G1", "G2", "G3", "G4", "G5", "G6", "G7", "G8",
                "H1", "H2", "H3", "H4", "H5", "H6", "H7", "H8",
            }.Select(Square.Parse).ToArray();

            var opponentOwnedSquares = squares.Where(sq => board[sq] != null &&
                                                           board[sq].Owner != player);
            var playerKingSquare = squares.First(sq => new King(player).Equals(board[sq]));

            return (from opponentOwnedSquare in opponentOwnedSquares
                    let piece = board[opponentOwnedSquare]
                    let move = new Move(opponentOwnedSquare, playerKingSquare, RevertPlayer(player), PawnPromotion.Queen) // Added PawnPromotion in the Move because omitting it causes a bug when King in its rank is in a check by a pawn.
                    where piece.IsValidGameMove(move, board)
                    select piece).Any();
        }

        internal static bool PlayerWillBeInCheck(Move move, GameBoard board)
        {
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            var boardClone = GameBoard.Clone(board); // Make the move on this board to keep original board as is.
            Piece piece = boardClone[move.Source];
            boardClone.Board[(int)move.Source.Rank, (int)move.Source.File] = null;
            boardClone.Board[(int)move.Destination.Rank, (int)move.Destination.File] = piece;

            return IsPlayerInCheck(move.Player, boardClone);
        }

        internal static bool IsTherePieceInBetween(Square square1, Square square2, Piece[,] board)
        {
            var xStep = 0;
            var yStep = 0;

            if (square2.File > square1.File)
            {
                xStep = 1;
            }
            if (square2.Rank > square1.Rank)
            {
                yStep = 1;
            }

            if (square2.File < square1.File)
            {
                xStep = -1;
            }
            if (square2.Rank < square1.Rank)
            {
                yStep = -1;
            }

            var source = new Square(square1.File, square1.Rank);
            var destination = new Square(square2.File, square2.Rank);
            Rank rank = source.Rank;
            File file = source.File;
            while (true)
            {
                rank += yStep;
                file += xStep;
                if (rank == destination.Rank && file == destination.File)
                {
                    return false;
                }

                if (board[(int)rank, (int)file] != null)
                {
                    return true;
                }
            }

        }
    }
}