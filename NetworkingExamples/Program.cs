using System.Net;
using System.Net.Sockets;

namespace NetworkingExamples
{
    internal class Program
    {
        private bool connectedClients = true;
        private readonly object l = new object();
        private Socket socket;
        List<String> users = new List<String>(){"YHVH","pablo","admin","jose","ruben"};

        static void Main(string[] args)
        {
            ushort[] ports = new ushort[5] {135, 49665, 49666, 31416, 5000 };
            Program procedure = new Program();
            Console.WriteLine(procedure.FreePort(ports));
            procedure.Init();
        }


        public int FreePort(ushort[] ports)
        {
            ushort port = 0;
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                ProtocolType.Tcp);
            int i = 0;
            while (i < ports.Length)
            {
                port = ports[i];
                IPEndPoint ie = new IPEndPoint(IPAddress.Any, port);
                try
                {
                    socket.Bind(ie);
                    socket.Close();
                    return port;
                }
                catch(SocketException e) when(e.ErrorCode ==
                                              (int)SocketError.AddressAlreadyInUse)
                {
                    i++;
                }
            }
            return -1;
        }

        public void Init()
        {
            IPEndPoint ie = new IPEndPoint(IPAddress.Any, port: 31416);
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(ie);
                socket.Listen(backlog: 999);
                Console.WriteLine("Connected in port:" + 31416);
                lock (l)
                {
                    while (connectedClients)
                    {
                        Socket clSocket = socket.Accept();
                        Thread thread = new Thread((() => ClientThread(clSocket)));
                        thread.Start();
                    }    
                }
                
                
            }
        }

        private void ClientThread(Socket clSocket)
        {
            string msg = "welcome to the server";
            string? userMsg = "";
            using (NetworkStream networkStream = new NetworkStream(clSocket))
            using (StreamReader streamReader = new StreamReader(networkStream))
            using (StreamWriter streamWriter = new StreamWriter(networkStream))
            {
                Console.WriteLine(msg + "say something");
                streamWriter.Write(msg);
                streamWriter.Flush();
                while (userMsg != null)
                {
                    try
                    {
                        userMsg = streamReader.ReadLine();
                        streamWriter.WriteLine(userMsg);
                        streamWriter.Flush();
                    }
                    catch (Exception e)
                    {
                        userMsg = null;
                        throw;
                    }
                }

                lock (l)
                {
                    connectedClients = false;
                }
                
            }
        }
    }

    
}