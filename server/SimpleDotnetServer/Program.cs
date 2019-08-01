using System;
using System.Threading;
using Valve.Sockets;

namespace SimpleDotnetServer
{
    class Program
    {
        public static void RunAndBlock(Action<CancellationTokenSource> action)
        {
            var done = new ManualResetEventSlim(false);
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                AttachCtrlcSigtermShutdown(done, cancellationTokenSource);
                Console.WriteLine("Starting");

                action(cancellationTokenSource);
                Console.WriteLine("Application started. Press Ctrl+C to shut down.");
                done.Wait();
            }
        }

        private static void AttachCtrlcSigtermShutdown(ManualResetEventSlim manualResetEvent,
            CancellationTokenSource cancellationTokenSource)
        {
            void Shutdown()
            {
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    Console.WriteLine("Application is shutting down...");
                    cancellationTokenSource.Cancel();
                }

                manualResetEvent.Set();
            }

            AppDomain.CurrentDomain.ProcessExit += (sender, args) => Shutdown();
            Console.CancelKeyPress += (sender, args) =>
            {
                Shutdown();
                args.Cancel = true;
            };
        }


        static void Main(string[] args)
        {
            var exitEvent = new ManualResetEvent(false);
            RunAndBlock((c) =>
            {
                Library.Initialize();
                try
                {
                    Run(c);
                }
                finally
                {
                    Library.Deinitialize();
                }
            });
            Console.WriteLine("Done");
        }

        static void Run(CancellationTokenSource cts)
        {
            ushort port = 8080;
            NetworkingSockets server = new NetworkingSockets();
            Address address = new Address();

            address.SetAddress("::0", port);

            uint listenSocket = server.CreateListenSocket(ref address);

            StatusCallback status = (info, context) => {
                switch (info.connectionInfo.state)
                {
                    case ConnectionState.None:
                        break;

                    case ConnectionState.Connecting:
                        server.AcceptConnection(info.connection);
                        break;

                    case ConnectionState.Connected:
                        Console.WriteLine("Client connected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                        break;

                    case ConnectionState.ClosedByPeer:
                        server.CloseConnection(info.connection);
                        Console.WriteLine("Client disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                        break;
                }
            };

#if VALVESOCKETS_SPAN
            MessageCallback message = (in NetworkingMessage netMessage) => {
	            Console.WriteLine("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
            };
#else
            const int maxMessages = 20;

            NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];
#endif

            while (!cts.IsCancellationRequested)
            {
                server.DispatchCallback(status);

#if VALVESOCKETS_SPAN
		        server.ReceiveMessagesOnListenSocket(listenSocket, message, 20);
#else
                int netMessagesCount = server.ReceiveMessagesOnListenSocket(listenSocket, netMessages, maxMessages);

                if (netMessagesCount > 0)
                {
                    for (int i = 0; i < netMessagesCount; i++)
                    {
                        ref NetworkingMessage netMessage = ref netMessages[i];

                        Console.WriteLine("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);

                        netMessage.Destroy();
                    }
                }
#endif

                Thread.Sleep(15);
            }
        }
    }
}
