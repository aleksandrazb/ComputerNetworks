using System;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class Client
{

    class StateObject
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
        string[] argHostName = new string[5];
        argHostName[0] = "localhost";


        if (argHostName.Length > 0)
        {
            IPAddress ipAddress =
              //Dns.Resolve(argHostName[0]).AddressList[0]; (przestarzaĹ‚e)
              Dns.GetHostEntry(argHostName[0]).AddressList[0];

            IPEndPoint ipEndpoint =
              new IPEndPoint(ipAddress, 1900);

            Socket clientSocket = new Socket(
              AddressFamily.InterNetworkV6,
              SocketType.Stream,
              ProtocolType.Tcp);

            IAsyncResult asyncConnect = clientSocket.BeginConnect(
              ipEndpoint,
              new AsyncCallback(connectCallback),
              clientSocket);

            Console.Write("Lacze sie.");
            if (writeDot(asyncConnect) == true)
            {
                Thread.Sleep(3000);
            }
        }
        Console.ReadLine();
    }

    public static void connectCallback(IAsyncResult asyncConnect)
    {
        Socket clientSocket =
          (Socket)asyncConnect.AsyncState;
        clientSocket.EndConnect(asyncConnect);

        if (clientSocket.Connected == false)
        {
            Console.WriteLine(".klient nie polaczony.");
            return;
        }
        else Console.WriteLine(".klient polaczony.");

        byte[] sendBuffer = Encoding.ASCII.GetBytes("Hello");
        IAsyncResult asyncSend = clientSocket.BeginSend(
          sendBuffer,
          0,
          sendBuffer.Length,
          SocketFlags.None,
          new AsyncCallback(sendCallback),
          clientSocket);

        Console.Write("Wysylanie danych.");
        writeDot(asyncSend);
    }

    public static void sendCallback(IAsyncResult asyncSend)
    {
        Socket clientSocket = (Socket)asyncSend.AsyncState;
        int bytesSent = clientSocket.EndSend(asyncSend);
        Console.WriteLine(
          ".Wyslano {0} bajtow.",
          bytesSent.ToString());

        StateObject stateObject =
          new StateObject(16, clientSocket);

        IAsyncResult asyncReceive =
          clientSocket.BeginReceive(
            stateObject.sBuffer,
            0,
            stateObject.sBuffer.Length,
            SocketFlags.None,
            new AsyncCallback(receiveCallback),
            stateObject);

        Console.Write("Odbieranie odpowiedzi serwera.");
        writeDot(asyncReceive);
    }

    public static void receiveCallback(IAsyncResult asyncReceive)
    {
        StateObject stateObject =
         (StateObject)asyncReceive.AsyncState;

        int bytesReceived =
          stateObject.sSocket.EndReceive(asyncReceive);

        Console.WriteLine(
          ".Otrzymano {0} bajtow: {1}{2}{2}Zamykanie polaczenia.",
          bytesReceived.ToString(),
          Encoding.ASCII.GetString(stateObject.sBuffer),
          Environment.NewLine);

        stateObject.sSocket.Shutdown(SocketShutdown.Both);
        stateObject.sSocket.Close();
    }

    internal static bool writeDot(IAsyncResult ar)
    {
        int i = 0;
        while (ar.IsCompleted == false)
        {
            if (i++ > 20)
            {
                Console.WriteLine("Przekroczono czas polaczenia z serwerem.");
                return false;
            }
            Console.Write(".");
            Thread.Sleep(100);
        }
        return true;
    }
}