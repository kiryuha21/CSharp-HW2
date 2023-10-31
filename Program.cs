using System.Net;
using System.Net.Sockets;
using System.Text;

var server = new HttpServer(49410, true);
server.Start();

public class HttpServer
{
    private readonly TcpListener _listener;
    private readonly bool _useThreadPool;
    private bool _isRunning = false;

    public HttpServer(int port, bool useThreadPool)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _useThreadPool = useThreadPool;

        int MaxThreadsCount = Environment.ProcessorCount * 4;
        ThreadPool.SetMaxThreads(MaxThreadsCount, MaxThreadsCount);
        ThreadPool.SetMinThreads(2, 2);
    }

    ~HttpServer()
    {
        if (_isRunning)
        {
            _listener.Stop();
        }
    }

    public void Start()
    {
        _listener.Start();
        _isRunning = true;

        Console.WriteLine("Server started.");

        while (true)
        {
            TcpClient client = _listener.AcceptTcpClient();

            if (_useThreadPool)
            {
                ThreadPool.QueueUserWorkItem(ProcessClient, client);
            }
            else
            {
                ProcessClient(client);
            }
        }
    }

    private void ProcessClient(object obj)
    {
        TcpClient client = obj as TcpClient;

        if (client == null)
            return;

        using (NetworkStream stream = client.GetStream())
        {
            byte[] request = new byte[1024];
            int bytesRead = stream.Read(request, 0, request.Length);
            string message = Encoding.ASCII.GetString(request, 0, bytesRead);
            Console.WriteLine("Received message: " + message);

            string response = File.ReadAllText("response.html");
            byte[] buffer = Encoding.ASCII.GetBytes(response);

            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }

        client.Close();
    }
}

