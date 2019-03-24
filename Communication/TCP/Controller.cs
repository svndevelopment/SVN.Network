﻿using SVN.Network.Communication.Message;
using SVN.Network.Properties;
using SVN.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace SVN.Network.Communication.TCP
{
    public class Controller
    {
        public bool IsRunning { get; private set; }
        private List<Connection> Connections { get; } = new List<Connection>();
        public Action<int, IMessage> HandleMessage { get; set; } = (clientId, message) => { };
        public Action<string> LogEvent { get; set; } = message => { };
        public Action<Exception> LogException { get; set; } = exception => { };
        public Action<int, string> LogConnectionEvent { get; set; } = (clientId, message) => { };
        public Action<int, string> LogConnectionTransfer { get; set; } = (clientId, message) => { };
        public Action<int, Exception> LogConnectionException { get; set; } = (clientId, exception) => { };

        protected int ConnectionsCount
        {
            get => this.Connections.Count;
        }

        private TimeSpan Sleeptime
        {
            get => TimeSpan.FromMilliseconds(int.Parse(Settings.ThreadSleeptime));
        }

        protected Controller()
        {
        }

        protected void Start(TcpClient tcpClient)
        {
            this.IsRunning = true;

            lock (this.Connections)
            {
                this.Connections.Add(new Connection(this, tcpClient));
            }

            TaskContainer.Run(this.Observer);
        }

        protected void Stop()
        {
            this.IsRunning = false;

            while (this.Connections.Any())
            {
                var connection = default(Connection);

                lock (this.Connections)
                {
                    connection = this.Connections.FirstOrDefault();
                }

                if (connection is null)
                {
                    Thread.Sleep(this.Sleeptime);
                    continue;
                }

                lock (this.Connections)
                {
                    this.Connections.Remove(connection);
                }

                Thread.Sleep(this.Sleeptime);
            }
        }

        protected void Send(IMessage message)
        {
            lock (this.Connections)
            {
                foreach (var connection in this.Connections.Where(x => x.IsRunning))
                {
                    connection.SendObject(message);
                }
            }
        }

        protected void Send(int clientId, IMessage message)
        {
            lock (this.Connections)
            {
                foreach (var connection in this.Connections.Where(x => x.Id == clientId && x.IsRunning))
                {
                    connection.SendObject(message);
                }
            }
        }

        private void Observer()
        {
            while (this.IsRunning)
            {
                var connection = default(Connection);

                lock (this.Connections)
                {
                    connection = this.Connections.FirstOrDefault(x => !x.IsRunning);
                }

                if (connection is null)
                {
                    Thread.Sleep(this.Sleeptime);
                    continue;
                }

                lock (this.Connections)
                {
                    this.Connections.Remove(connection);
                }

                Thread.Sleep(this.Sleeptime);
            }
        }
    }
}