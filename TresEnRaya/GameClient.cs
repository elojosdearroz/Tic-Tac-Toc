﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TresEnRaya
{
    public class GameClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        public event Action<string> OnMessageReceived;

        public void Connect(string ip, int port)
        {
            _client = new TcpClient();
            _client.Connect(ip, port);
            _stream = _client.GetStream();
            Console.WriteLine("Conectado al servidor.");

            // Hilo para escuchar mensajes
            new Thread(() => ListenForMessages()).Start();
        }

        public void SendAction(string action, int fila, int columna, int playerId)
        {
            string message = $"{action}|{fila}|{columna}|{playerId}";
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            _stream.Write(buffer, 0, buffer.Length);
        }

        public void ListenForMessages()
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                OnMessageReceived?.Invoke(message);
            }
        }
    }
}
