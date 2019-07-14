using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MyRpc;
using MyRpc.Impl;
using MyRpc.Model;

namespace RPC
{
    public interface IChatService
    {
        string Ping();
        IChatSession Login(string username, IMessageHandler handler);
    }

    public interface IChatSession
    {
        void SendMessage(string text);
    }

    public interface IMessageHandler
    {
        void OnMessage(string username, string text);
    }
    class ClientMessageHandler : IMessageHandler
    {
        void IMessageHandler.OnMessage(string username, string text)
        {
            Console.WriteLine($"[{username}] {text}");
        }
    }

    class Program
    {
        static readonly IRpcProtocol<IPEndPoint, byte[], object> _binaryTcpProtocol = Rpc.TcpTransport.MakeProtocol(Rpc.BinarySerializer);
        static readonly IRpcServiceHost<object, IChatService> _host = Rpc.GenericHost.Helper().ForService<IChatService>();

        static void DoServer()
        {
            Console.WriteLine("\nServer mode");
            Console.WriteLine("----------------------------");
            var svc = new ChatServiceImpl();

            var activeSessions = new LinkedList<IRpcChannel<IChatService>>();

            using (var listener = _host.Listen(_binaryTcpProtocol, new IPEndPoint(IPAddress.Any, 12345)))
            {
                listener.Start();

                Action<IRpcChannelAcceptContext<IPEndPoint, IChatService>> acceptHandler = null;
                acceptHandler = ctx =>
                {
                    var sessionSvcStub = new ChatServiceStub(svc);
                    var channel = ctx.Confirm(sessionSvcStub);

                    lock (activeSessions)
                    {
                        channel.OnClosed += () =>
                        {
                            lock (activeSessions)
                                activeSessions.Remove(channel);

                            sessionSvcStub.CleanupSession();
                        };

                        activeSessions.AddLast(channel);
                    }

                    channel.Start();

                    listener.AcceptChannelAsync(acceptHandler);
                };

                listener.AcceptChannelAsync(acceptHandler);

                Console.ReadLine();
            }
        }

        static void DoClient()
        {
            Console.WriteLine("\nClient mode");
            Console.WriteLine("---------------------------------");
            using (var cnn = _host.Connect(_binaryTcpProtocol, new IPEndPoint(IPAddress.Loopback, 12345)))
            {
                Console.WriteLine("Login in progress");

                var sw = new Stopwatch();
                for (int i = 0; i < 20; i++)
                {
                    sw.Start();
                    Console.WriteLine(cnn.Service.Ping());
                    sw.Stop();
                    Console.WriteLine(i);
                }
                Console.WriteLine(new TimeSpan(sw.ElapsedTicks / 20));


                var session = cnn.Service.Login(Console.ReadLine(), new ClientMessageHandler());

                Console.WriteLine("Login success");

                for (; ; )
                    session.SendMessage(Console.ReadLine());
            }
        }

        static void Main(string[] args)
        {
            if (Console.ReadKey().Key == ConsoleKey.S)
            {
                DoServer();
            }
            else
            {
                DoClient();
            }
        }
    }
}
