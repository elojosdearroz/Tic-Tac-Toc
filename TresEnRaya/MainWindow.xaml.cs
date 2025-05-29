using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TresEnRaya
{
    public partial class MainWindow : Window
    {
        private string[,] board = new string[3, 3];
        private bool isXTurn = true;
        private GameClient _gameClient;
        private int _playerId;
        private bool _isMyTurn = false;
        private string modoJuego = "";
        private TicTacToeAI ai;

        public MainWindow()
        {
            InitializeComponent();
            InicializarJuego();
        }

        #region Inicialización y Configuración de Juego
        private void InicializarJuego()
        {
            board = new string[3, 3];
            isXTurn = true;
            StatusText.Text = "¡Empieza jugando! Tu turno (X)";
            LimpiarBotones();
        }

        private void LimpiarBotones()
        {
            foreach (var btn in FindVisualChildren<Button>(GameBoard).Where(b => b.Tag != null))
            {
                btn.Content = "";
                btn.IsEnabled = true;
            }
        }
        #endregion

        #region Manejo de Modos de Juego
        private void vsMaquina_Click(object sender, RoutedEventArgs e)
        {
            modoJuego = "vsMaquina";
            VsPlayerRadio.Visibility = Visibility.Collapsed;
            IniciarJuegoContraIA();
        }

        private void vsJugador_Click(object sender, RoutedEventArgs e)
        {
            modoJuego = "vsJugador";
            VsMachineRadio.Visibility = Visibility.Collapsed;
            NetworkPanel.Visibility = Visibility.Visible;
        }

        private void IniciarJuegoContraIA()
        {
            InicializarJuego();
            ai = new TicTacToeAI();
            ai.Reset();
            StatusText.Text = "¡Empieza jugando! Tu turno (X)";
        }

        public void IniciarJuegoEnLinea(int playerId)
        {
            _gameClient = new GameClient();
            _gameClient.OnMessageReceived += GameClient_OnMessageReceived;
            _gameClient.Connect("200.0.0.126", 1234);

            _playerId = playerId;
            _isMyTurn = (playerId == 1);
            StatusText.Text = _isMyTurn ? "Tu turno (X)" : "Esperando turno del oponente...";
        }
        #endregion

        #region Lógica del Juego
        private void BotonTablero_Click(object sender, RoutedEventArgs e)
        {
            if (modoJuego == "vsMaquina")
            {
                ProcesarMovimientoIA((Button)sender);
            }
            else if (modoJuego == "vsJugador" && _isMyTurn)
            {
                ProcesarMovimientoMultijugador((Button)sender);
            }
        }

        private void ProcesarMovimientoIA(Button btn)
        {
            var (fila, columna) = ObtenerCoordenadas(btn);

            if (!string.IsNullOrEmpty(board[fila, columna])) return;

            // Movimiento del jugador
            RealizarMovimiento(btn, fila, columna, "X");

            if (VerificarGanador("X")) return;
            if (TableroLleno()) return;

            // Movimiento de la IA
            MoverIA();
        }

        private void MoverIA()
        {
            char[] aiBoard = new char[9];
            for (int i = 0; i < 9; i++)
            {
                int f = i / 3, c = i % 3;
                aiBoard[i] = string.IsNullOrEmpty(board[f, c]) ? '-' : board[f, c][0];
            }

            Array.Copy(aiBoard, ai.Board, 9);
            int aiMove = ai.MakeBotMove();

            if (aiMove != -1)
            {
                int f = aiMove / 3, c = aiMove % 3;
                var btn = FindVisualChildren<Button>(GameBoard)
                    .FirstOrDefault(b => b.Tag.ToString() == $"{f},{c}");

                if (btn != null)
                {
                    RealizarMovimiento(btn, f, c, "O");
                }
            }
        }

        private void ProcesarMovimientoMultijugador(Button btn)
        {
            var (fila, columna) = ObtenerCoordenadas(btn);
            _gameClient.SendAction("play", fila, columna, _playerId);
        }

        private (int fila, int columna) ObtenerCoordenadas(Button btn)
        {
            string[] coordenadas = btn.Tag.ToString().Split(',');
            return (int.Parse(coordenadas[0]), int.Parse(coordenadas[1]));
        }

        private void RealizarMovimiento(Button btn, int fila, int columna, string jugador)
        {
            board[fila, columna] = jugador;
            btn.Content = jugador;
            btn.IsEnabled = false;

            if (VerificarGanador(jugador))
            {
                StatusText.Text = jugador == "X" ? "¡Ganaste!" : "¡La IA gana!";
                DesactivarTablero();
            }
            else if (TableroLleno())
            {
                StatusText.Text = "¡Empate!";
                DesactivarTablero();
            }
        }
        #endregion

        #region Lógica Multijugador
        private void GameClient_OnMessageReceived(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var parts = message.Split('|');
                if (parts.Length == 4 && parts[0] == "play")
                {
                    ProcesarMensajeJuego(parts);
                }
            });
        }

        private void ProcesarMensajeJuego(string[] parts)
        {
            int fila = int.Parse(parts[1]);
            int columna = int.Parse(parts[2]);
            int playerId = int.Parse(parts[3]);
            string jugador = (playerId == 1) ? "X" : "O";

            board[fila, columna] = jugador;

            var btn = FindVisualChildren<Button>(GameBoard)
                .FirstOrDefault(b => b.Tag.ToString() == $"{fila},{columna}");

            if (btn != null)
            {
                btn.Content = jugador;
                btn.IsEnabled = false;
            }

            if (VerificarGanador(jugador))
            {
                StatusText.Text = $"¡Jugador {jugador} gana!";
                DesactivarTablero();
                _isMyTurn = false;
            }
            else if (TableroLleno())
            {
                StatusText.Text = "¡Empate!";
                _isMyTurn = false;
            }
            else
            {
                _isMyTurn = (playerId != _playerId);
                StatusText.Text = _isMyTurn ? "Tu turno" : "Turno del oponente";
            }
        }
        #endregion

        #region Utilidades del Juego
        private bool VerificarGanador(string jugador)
        {
            // Filas y columnas
            for (int i = 0; i < 3; i++)
            {
                if ((board[i, 0] == jugador && board[i, 1] == jugador && board[i, 2] == jugador) ||
                    (board[0, i] == jugador && board[1, i] == jugador && board[2, i] == jugador))
                    return true;
            }

            // Diagonales
            return (board[0, 0] == jugador && board[1, 1] == jugador && board[2, 2] == jugador) ||
                   (board[0, 2] == jugador && board[1, 1] == jugador && board[2, 0] == jugador);
        }

        private bool TableroLleno() => !board.Cast<string>().Any(string.IsNullOrEmpty);

        private void DesactivarTablero()
        {
            foreach (var btn in FindVisualChildren<Button>(GameBoard))
                btn.IsEnabled = false;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T t) yield return t;
                foreach (var childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }
        #endregion

        #region Eventos de UI
        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            InicializarJuego();
            modoJuego = "";
            VsMachineRadio.Visibility = Visibility.Visible;
            VsMachineRadio.IsChecked = false;
            VsPlayerRadio.Visibility = Visibility.Visible;
            VsPlayerRadio.IsChecked = false;
            NetworkPanel.Visibility = Visibility.Collapsed;
            if (ai != null) ai.Reset();
        }

        private void HostButton_Click(object sender, RoutedEventArgs e)
        {
            IniciarJuegoEnLinea(1);
            JoinButton.IsEnabled = HostButton.IsEnabled = false;
        }

        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            IniciarJuegoEnLinea(2);
            JoinButton.IsEnabled = HostButton.IsEnabled = false;
        }
        #endregion
    }
}