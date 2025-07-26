using System.Net.Sockets;
using GameCommon.Network;
using GameCommon.Protos;

namespace GameServer.Network;

public class NetService
{
    private TcpSocketListener? _listener;

    public NetService()
    {
    }

    public void Init(int port)
    {
        _listener = new TcpSocketListener("0.0.0.0", port);

        _listener.SocketConnected += ListenerOnClientConnected;
    }

    public void Start()
    {
        CheckedListener.Start();
    }

    private TcpSocketListener CheckedListener
    {
        get
        {
            if (_listener == null)
            {
                throw new InvalidOperationException("网络服务未初始化，请先调用 Init 方法。");
            }

            return _listener;
        }
    }

    private void ListenerOnClientConnected(object? sender, Socket socket)
    {
        _ = new NetConnection(socket, ConnectionOnDataReceived, ConnectionOnDisconnected);
    }

    private void ConnectionOnDataReceived(NetConnection receiver, byte[] data)
    {
        var v = Vector3.Parser.ParseFrom(data);

        Console.WriteLine(v);
    }

    private void ConnectionOnDisconnected(NetConnection connection)
    {
        Console.WriteLine("客户端断开连接，端点: " + connection.RemoteEndPoint);
    }
}