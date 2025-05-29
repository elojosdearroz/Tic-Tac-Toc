using System;
using System.Collections.Generic;
using System.Linq;

namespace TresEnRaya
{
    public class TicTacToeAI
    {
        private char[] board = Enumerable.Repeat('-', 9).ToArray();
        private char humanPlayer;
        private char botPlayer;

        public TicTacToeAI(char human = 'X', char bot = 'O')
        {
            humanPlayer = human;
            botPlayer = bot;
        }

        public char[] Board => board;

        public bool MakeHumanMove(int pos)
        {
            if (pos < 0 || pos >= 9 || board[pos] != '-') return false;
            board[pos] = humanPlayer;
            return true;
        }

        public int MakeBotMove()
        {
            // Primero verifica si puede ganar en el siguiente movimiento
            int winningMove = GetWinningMove(botPlayer);
            if (winningMove != -1)
            {
                board[winningMove] = botPlayer;
                return winningMove;
            }

            // Luego verifica si el humano puede ganar en el siguiente movimiento para bloquear
            int blockingMove = GetWinningMove(humanPlayer);
            if (blockingMove != -1)
            {
                board[blockingMove] = botPlayer;
                return blockingMove;
            }

            // Si no, usa Minimax con poda alfa-beta
            var move = Minimax(board, botPlayer, int.MinValue, int.MaxValue).Item1;
            if (move != -1)
                board[move] = botPlayer;
            return move;
        }

        public bool CheckWinner(char player)
        {
            int[,] wins = {
                {0,1,2}, {3,4,5}, {6,7,8},
                {0,3,6}, {1,4,7}, {2,5,8},
                {0,4,8}, {2,4,6}
            };

            for (int i = 0; i < wins.GetLength(0); i++)
            {
                if (board[wins[i, 0]] == player && board[wins[i, 1]] == player && board[wins[i, 2]] == player)
                    return true;
            }
            return false;
        }

        public bool IsDraw()
        {
            return !board.Contains('-');
        }

        public void Reset()
        {
            board = Enumerable.Repeat('-', 9).ToArray();
        }

        // Funciones privadas mejoradas:
        private int GetWinningMove(char player)
        {
            int[,] wins = {
                {0,1,2}, {3,4,5}, {6,7,8},
                {0,3,6}, {1,4,7}, {2,5,8},
                {0,4,8}, {2,4,6}
            };

            for (int i = 0; i < wins.GetLength(0); i++)
            {
                int a = wins[i, 0], b = wins[i, 1], c = wins[i, 2];
                if (board[a] == player && board[b] == player && board[c] == '-') return c;
                if (board[a] == player && board[c] == player && board[b] == '-') return b;
                if (board[b] == player && board[c] == player && board[a] == '-') return a;
            }
            return -1;
        }

        private Tuple<int, int> Minimax(char[] state, char player, int alpha, int beta)
        {
            char opponent = player == 'X' ? 'O' : 'X';
            List<int> availableMoves = GetAvailableMovesOrdered(state);

            if (CheckStaticWin(state, botPlayer)) return Tuple.Create(-1, 10);
            if (CheckStaticWin(state, humanPlayer)) return Tuple.Create(-1, -10);
            if (availableMoves.Count == 0) return Tuple.Create(-1, 0);

            Tuple<int, int> bestMove = null;

            foreach (int move in availableMoves)
            {
                char[] newState = (char[])state.Clone();
                newState[move] = player;
                int score = Minimax(newState, opponent, alpha, beta).Item2;

                if (player == botPlayer)
                {
                    if (bestMove == null || score > bestMove.Item2)
                    {
                        bestMove = Tuple.Create(move, score);
                        alpha = Math.Max(alpha, score);
                    }
                }
                else
                {
                    if (bestMove == null || score < bestMove.Item2)
                    {
                        bestMove = Tuple.Create(move, score);
                        beta = Math.Min(beta, score);
                    }
                }

                if (beta <= alpha)
                    break;
            }

            return bestMove;
        }

        private List<int> GetAvailableMovesOrdered(char[] state)
        {
            // Prioriza el centro y luego las esquinas para mejores resultados
            int[] movePriority = { 4, 0, 2, 6, 8, 1, 3, 5, 7 };
            return movePriority.Where(pos => state[pos] == '-').ToList();
        }

        private bool CheckStaticWin(char[] state, char player)
        {
            int[,] wins = {
                {0,1,2}, {3,4,5}, {6,7,8},
                {0,3,6}, {1,4,7}, {2,5,8},
                {0,4,8}, {2,4,6}
            };

            for (int i = 0; i < wins.GetLength(0); i++)
            {
                if (state[wins[i, 0]] == player && state[wins[i, 1]] == player && state[wins[i, 2]] == player)
                    return true;
            }
            return false;
        }
    }
}