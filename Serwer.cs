using System;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// Summary description for Class1
/// </summary>
public class Server
{
    internal class StateObject
    {
        internal byte[] sBuffer;
        internal Socket sSocket;
        internal StateObject(int size, Socket sock)
        {
            sBuffer = new byte[size];
            sSocket = sock;
        }

    }

    static void Main()
    {
        IPEndPoint ipEndpoint = new IPEndPoint(IPAddress.IPv6Any, 1900);
        Socket listenSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        listenSocket.Bind(ipEndpoint);
        listenSocket.Listen(1);
        // Zamiast blokowac podaje metode do wywolania w przypadku
        // polaczenia
        IAsyncResult asyncAccept = listenSocket.BeginAccept(
          new AsyncCallback(Server.acceptCallback),
          listenSocket);

        Console.Write("Trwa laczenie.");
        if (writeDot(asyncAccept) == true)
        {
            Thread.Sleep(3000);
        }
    }

    public static void acceptCallback(IAsyncResult asyncAccept)
    {
        Socket listenSocket = (Socket)asyncAccept.AsyncState;
        Socket serverSocket =
          listenSocket.EndAccept(asyncAccept);

        if (serverSocket.Connected == false)
        {
            Console.WriteLine(".serwer nie polaczony.");
            return;
        }
        else Console.WriteLine(".serwer polaczony.");

        listenSocket.Close();

        StateObject stateObject =
          new StateObject(16, serverSocket);

        // Przekazujemy StateObject, bo potrzeba zarowno gniazda,
        // jak i bufora
        IAsyncResult asyncReceive =
          serverSocket.BeginReceive(
            stateObject.sBuffer,
            0,
            stateObject.sBuffer.Length,
            SocketFlags.None,
            new AsyncCallback(receiveCallback),
            stateObject);

        Console.Write("Odbieranie danych.");
        writeDot(asyncReceive);
    }

    public static void receiveCallback(IAsyncResult asyncReceive)
    {
        StateObject stateObject =
          (StateObject)asyncReceive.AsyncState;
        int bytesReceived =
          stateObject.sSocket.EndReceive(asyncReceive);

        Console.WriteLine(
          ".Otrzymano {0} bajtow: {1}",
          bytesReceived.ToString(),
          Encoding.ASCII.GetString(stateObject.sBuffer));

        byte[] sendBuffer =
          Encoding.ASCII.GetBytes("Goodbye");
        IAsyncResult asyncSend =
          stateObject.sSocket.BeginSend(
            sendBuffer,
            0,
            sendBuffer.Length,
            SocketFlags.None,
            new AsyncCallback(sendCallback),
            stateObject.sSocket);

        Console.Write("Wysylanie odpowiedzi.");
        writeDot(asyncSend);
    }

    public static void sendCallback(IAsyncResult asyncSend)
    {
        Socket serverSocket = (Socket)asyncSend.AsyncState;
        int bytesSent = serverSocket.EndSend(asyncSend);
        Console.WriteLine(
          ".Wyslano {0} bajtow.{1}{1}Zamykanie gniazd.",
          bytesSent.ToString(),
          Environment.NewLine);

        serverSocket.Shutdown(SocketShutdown.Both);
        serverSocket.Close();
    }

    internal static bool writeDot(IAsyncResult ar)
    {
        int i = 0;
        while (ar.IsCompleted == false)
        {
            if (i++ > 40)
            {
                Console.WriteLine("Przekroczony czas oczekiwania.");
                return false;
            }
            Console.Write(".");
            Thread.Sleep(500);
        }
        return true;
    }
}