using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var move = Minimax(board, botPlayer).Item1;
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

        // Minimax y funciones privadas:
        private Tuple<int, int> Minimax(char[] state, char player)
        {
            char opponent = player == 'X' ? 'O' : 'X';
            List<int> actions = state.Select((v, i) => new { v, i }).Where(x => x.v == '-').Select(x => x.i).ToList();

            if (CheckStaticWin(state, botPlayer)) return Tuple.Create(-1, 10);
            if (CheckStaticWin(state, humanPlayer)) return Tuple.Create(-1, -10);
            if (actions.Count == 0) return Tuple.Create(-1, 0);

            Tuple<int, int> bestMove = null;

            foreach (int action in actions)
            {
                char[] newState = (char[])state.Clone();
                newState[action] = player;
                int score = Minimax(newState, opponent).Item2;

                score = player == botPlayer ? score : -score;

                if (bestMove == null || score > bestMove.Item2)
                {
                    bestMove = Tuple.Create(action, score);
                }
            }

            return bestMove;
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
