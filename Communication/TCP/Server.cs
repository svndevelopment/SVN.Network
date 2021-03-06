﻿using SVN.Tasks;
using System;
using System.Net;
using System.Net.Sockets;

namespace SVN.Network.Communication.TCP
{
    public class Server : Controller
    {
        public static Server Instance { get; } = new Server();

        public new bool IsRunning { get; private set; }
        private TcpListener TcpListener { get; set; }

        public Server()
        {
        }

        public void Start(int port = 10000)
        {
            if (!this.IsRunning)
            {
                try
                {
                    this.IsRunning = true;

                    this.TcpListener = new TcpListener(IPAddress.Any, port);
                    this.TcpListener.Start();

                    base.Start();
                    TaskContainer.Run(() => this.Listener(port));
                    base.OnInitializationSuccess("localhost", port);
                }
                catch (Exception e)
                {
                    base.OnInitializationFailed("localhost", port, e);
                    this.Stop();
                }
            }
        }

        public new void Stop()
        {
            if (this.IsRunning)
            {
                this.IsRunning = false;

                this.TcpListener.Stop();
                base.Stop();
            }
        }

        public new void Send(int clientId, object message)
        {
            base.Send(clientId, message);
        }

        public new void SendToOthers(int clientId, object message)
        {
            base.SendToOthers(clientId, message);
        }

        public new void SendToAll(object message)
        {
            base.SendToAll(message);
        }

        private void Listener(int port)
        {
            while (this.IsRunning)
            {
                try
                {
                    base.StartConnection(this.TcpListener.AcceptTcpClient());
                }
                catch (SocketException)
                {
                    this.Stop();
                }
                catch (Exception e)
                {
                    base.OnUnhandledException(e);
                    this.Stop();
                }
            }
        }
    }
}