using MyRpc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyRpc.Impl.Transport
{
    public class RpcTransportListener : IRpcTransportListener<IPEndPoint, byte[]>
    {
        //public event Action<Exception> OnError = delegate { };

        public IPEndPoint LocalEndPoint { get; }
        // private SocketAsyncEventArgs _acceptAsyncArgs;
        private TcpListener _listener;

        public RpcTransportListener(IPEndPoint localEndPoint)
        {
            this._listener = new TcpListener(localEndPoint); ;
            // _acceptAsyncArgs = new SocketAsyncEventArgs();
            // _acceptAsyncArgs.Completed += this.AcceptCompleted;
            this.LocalEndPoint = localEndPoint;
        }

        public async Task<IRpcTransportAcceptContext<IPEndPoint, byte[]>> AcceptAsync()
        {
            var sck = await _listener.AcceptSocketAsync();
            return new RpcTransportAcceptContext(sck);


            //var acceptArgs = new SocketAsyncEventArgs();
            //acceptArgs.Completed += (sender, ea) =>
            //{
            //    var sck = ea.AcceptSocket;

            //    var acceptContext = new RpcTransportAcceptContext(sck);
            //    onAccepted(acceptContext);
            //};

            // _listenSocket.AcceptAsync(acceptArgs);
        }

        public void Dispose()
        {
            _listener.Stop();
        }

        public void Start()
        {
            _listener.Start(10);
        }

        //private void AcceptAsync(SocketAsyncEventArgs e)
        //{
        //    bool willRaiseEvent = _listenSocket.AcceptAsync(e);
        //    if (!willRaiseEvent)
        //        this.AcceptCompleted(_listenSocket, e);
        //}

        //private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        //{
        //    if (e.SocketError == SocketError.Success)
        //    {
        //        //TODO: do smth (but what?)
        //    }
        //    e.AcceptSocket = null;
        //    this.AcceptAsync(_acceptAsyncArgs);
        //}

    }
}
