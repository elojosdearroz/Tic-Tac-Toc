using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TresEnRaya
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[,] board = new string[3, 3];
        private bool isXTurn = true;
        private GameClient _gameClient;
        private int _playerId; // Puedes usar 1 o 2 para identificar jugador
        private bool _isMyTurn = false;

        public MainWindow()
        {
            InitializeComponent();
            InicializarJuego();

            _gameClient = new GameClient();
            _gameClient.OnMessageReceived += GameClient_OnMessageReceived;

            // Conectarse al servidor (ip y puerto deben ser los correctos)
            _gameClient.Connect("200.0.0.126", 1234);

            // Aquí define el jugador y si empieza primero o segundo

            _isMyTurn = (_playerId == 1);
            StatusText.Text = _isMyTurn ? "Tu turno (X)" : "Esperando turno del oponente...";
        }

        private void InicializarJuego()
        {
            board = new string[3, 3];
            isXTurn = true;
            StatusText.Text = "¡Empieza jugando! Tu turno (X)";
            LimpiarBotones();
        }

        private void LimpiarBotones()
        {
            foreach (var child in GameBoard.Children)
            {
                if (child is Button btn && btn.Tag != null)
                {
                    btn.Content = "";
                    btn.IsEnabled = true;
                }
            }
        }



        private void BotonTablero_Click(object sender, RoutedEventArgs e)
        {
            if (!_isMyTurn) return; // Solo jugar si es tu turno

            Button btn = (Button)sender;
            string[] coordenadas = btn.Tag.ToString().Split(',');
            int fila = int.Parse(coordenadas[0]);
            int columna = int.Parse(coordenadas[1]);

            // Enviar la acción al servidor
            _gameClient.SendAction("play", fila, columna, _playerId);


        }
        private void GameClient_OnMessageReceived(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var parts = message.Split('|');

                if (parts[0] == "CONNECT")
                {
                    _playerId = int.Parse(parts[1]);
                    _isMyTurn = (_playerId == 1); // Jugador 1 empieza
                    StatusText.Text = _isMyTurn ? "Tu turno (X)" : "Esperando turno del oponente...";
                    return;
                }

                if (parts.Length == 4 && parts[0] == "play")
                {
                    int fila = int.Parse(parts[1]);
                    int columna = int.Parse(parts[2]);
                    int playerId = int.Parse(parts[3]);

                    string jugador = (playerId == 1) ? "X" : "O";
                    board[fila, columna] = jugador;

                    foreach (var btn in FindVisualChildren<Button>(GameBoard))
                    {
                        if (btn.Tag != null && btn.Tag.ToString() == $"{fila},{columna}")
                        {
                            btn.Content = jugador;
                            btn.IsEnabled = false;
                            break;
                        }
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
            });
        }





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
            if ((board[0, 0] == jugador && board[1, 1] == jugador && board[2, 2] == jugador) ||
                (board[0, 2] == jugador && board[1, 1] == jugador && board[2, 0] == jugador))
                return true;

            return false;
        }

        private bool TableroLleno()
        {
            foreach (var valor in board)
            {
                if (string.IsNullOrEmpty(valor))
                    return false;
            }
            return true;
        }

        #region DesactivarTablero
        //Conjunto de metodos para la desactivacion del tablero
        //Desacticvar el tablero
        private void DesactivarTablero()
        {
            foreach (var btn in FindVisualChildren<Button>(GameBoard))
            {
                btn.IsEnabled = false;
            }
        }
        //Metodo recursivo para desactivar el tablero una vez haya un ganador

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T t)
                    {
                        yield return t;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        #endregion

    }
}
