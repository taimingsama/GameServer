using System.Net;
using System.Net.Sockets;

namespace GameServer.Network
{
    /// <summary>
    /// 负责监听TCP网络端口，异步接收Socket连接
    /// </summary>
    public class TcpSocketListener
    {
        public event EventHandler<Socket>? SocketConnected; //客户端接入事件

        private readonly IPEndPoint _endPoint;
        private Socket? _serverSocket; //服务端监听对象

        public TcpSocketListener(string host, int port)
        {
            _endPoint = new IPEndPoint(IPAddress.Parse(host), port);
        }

        public void Start()
        {
            if (IsRunning) return;

            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(_endPoint);
            _serverSocket.Listen();
            Console.WriteLine("开始监听于端点：" + _serverSocket.LocalEndPoint);

            var args = new SocketAsyncEventArgs();
            args.Completed += OnAccept; //当有人连入的时候
            _serverSocket.AcceptAsync(args);
        }

        private void OnAccept(object? sender, SocketAsyncEventArgs e)
        {
            if (_serverSocket == null) return;

            //真的有人连进来
            if (e.SocketError == SocketError.Success)
            {
                var client = e.AcceptSocket; //连入的人

                if (client != null)
                {
                    SocketConnected?.Invoke(this, client);
                }
            }

            //继续接收下一位
            e.AcceptSocket = null;
            _serverSocket.AcceptAsync(e);
        }

        public bool IsRunning => _serverSocket != null;

        public void Stop()
        {
            if (_serverSocket == null) return;

            _serverSocket.Close();
            _serverSocket = null;
        }
    }
}