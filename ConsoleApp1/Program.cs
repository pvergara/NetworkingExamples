using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ConsoleApp1
{
    internal class Program
    {

        private bool connectedClients = true;
        private readonly object l = new object();
        private Socket socket;
        private static string path = Environment.GetEnvironmentVariable("UserProfile")+"\\lerele.bin";
        private static IList<User> usersReader;
        private Dictionary<Socket,StreamWriter> Dictionary = new Dictionary<Socket, StreamWriter>();

        static void Main(string[] args)
        {
            //IList<User> users = new List<User>()
            //{
            //    new(name: "YHVH", password: "1YHVH1"),
            //    new(name: "pablo", password: "ApabloA"),
            //    new(name : "admin", password : "EadminE"),
            //    new(name: "jose", password: "yjosey"),
            //    new(name : "ruben", password : "YrubenY")
            //};

            
            //WriteUsersToFile(users);

            Program.usersReader = GetUsersFromFile(path);

            foreach (var user in usersReader)
                Console.WriteLine(user);

            Program program = new Program();
            program.Init();
        }

        private static void WriteUsersToFile(IList<User> users)
        {
            using (BinaryWriter writer = new BinaryWriter(new FileStream(
                       path,
                       FileMode.OpenOrCreate)))
            {
                foreach (var user in users)
                {
                    writer.Write(user.Name);
                    writer.Write(user.Password);
                    writer.Write(user.lastConnection);
                }

            }
        }

        private static IList<User> GetUsersFromFile(string path)
        {
            IList<User> users;
            IList<User> usersReader = new List<User>();
            using (BinaryReader reader = new BinaryReader(new FileStream(path , FileMode.Open)))
            {
                long readed = 0;
                while(reader.PeekChar()!=-1)
                {
                    User aux = new User();
                    aux.Name = reader.ReadString();
                    aux.Password = reader.ReadString();
                    aux.lastConnection =  reader.ReadString();
                    usersReader.Add(aux);
                }
            }

            return usersReader;
        }

        public void Init()
        {
            IPEndPoint ie = new IPEndPoint(IPAddress.Any, port: 31416);
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(ie);
                socket.Listen(backlog: 999);
                Console.WriteLine("Connected in port:" + 31416);
                while (connectedClients)
                {
                    Socket clSocket = socket.Accept();
                    Thread thread = new Thread((() => ClientThread(clSocket)));
                    thread.Start();
                }    
                
                
            }
        }

        private void ClientThread(Socket clSocket)
        {

            string msg = "welcome to the server, give me your username and password";
            string? userMsg = "";
            using (NetworkStream networkStream = new NetworkStream(clSocket))
            using (StreamReader streamReader = new StreamReader(networkStream))
            using (StreamWriter streamWriter = new StreamWriter(networkStream))
            {
                AddMyStreamWriterToDictionaryDependingOn(clSocket,streamWriter);
                streamWriter.WriteLine(msg);
                streamWriter.Flush();
                streamWriter.WriteLine("user pls: ");
                streamWriter.Flush();
                string? username= streamReader.ReadLine();
                streamWriter.WriteLine("password pls: ");
                streamWriter.Flush();
                string? password = streamReader.ReadLine();
                bool userFound = false;
                User userAux = null;
                lock (l)
                {
                    foreach (User user in Program.usersReader)
                    {
                        if (user.Name == username && user.Password == password)
                        {
                            userFound = true;
                            userAux = user;
                            break;
                        }
                    }
                }
                
                if (userFound)
                {
                    streamWriter.WriteLine("Hey Hello!!!");
                    streamWriter.Flush();
                    lock (l)
                    {
                        Program.usersReader.First(u => u.Name == userAux.Name && u.Password == userAux.Password)
                                .lastConnection = $@"{DateTime.Now:F}";
                        WriteUsersToFile(Program.usersReader);
                    }
                    while (userMsg != null)
                    {
                        try
                        {
                            userMsg = streamReader.ReadLine();
                            //        streamWriter.WriteLine(userMsg);
                            //        streamWriter.Flush();
                        }
                        catch (Exception e)
                        {
                            userMsg = null;
                            throw;
                        }
                    }
                }
                else
                {
                    lock (l)
                    {
                        foreach (var keyValue in this.Dictionary)
                        {
                            if (keyValue.Key != clSocket)
                            {
                                StreamWriter streamWriterAux = keyValue.Value;
                                streamWriterAux.WriteLine("Goodbye (to romance)!!!");
                                streamWriterAux.Flush();
                            }
                        }
                        
                    }

                }



                lock (l)
                {
                    connectedClients = false;
                }
                this.DeleteMyStreamWriterToDictionaryDependingOn(clSocket);
                clSocket.Close();
            }
        }

        private void DeleteMyStreamWriterToDictionaryDependingOn(Socket clSocket)
        {
            lock (l)
            {
                this.Dictionary.Remove(clSocket);
            }
        }

        private void AddMyStreamWriterToDictionaryDependingOn(Socket clSocket, StreamWriter streamWriter)
        {
            lock (l)
            {
                this.Dictionary.Add(clSocket,streamWriter);
            }
        }
    }
}
