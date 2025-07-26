using System.Net;
using System.Net.Sockets;
using GameCommon.Network;
using GameCommon.Protos;

namespace GameServer.Network;

public class NetConnection
{
    public delegate void DataReceivedHandler(NetConnection receiver, byte[] data);

    public delegate void DisconnectedHandler(NetConnection connection);

    private readonly Socket _socket;
    private readonly DataReceivedHandler _dataReceived;
    private readonly DisconnectedHandler _disconnected;

    public NetConnection(Socket socket,
        DataReceivedHandler dataReceived,
        DisconnectedHandler disconnected)
    {
        _socket = socket;
        _dataReceived = dataReceived;
        _disconnected = disconnected;

        var decoder = new LengthFieldDecoder(socket, 1024, 0,
            sizeof(int), 0, sizeof(int));
        decoder.DataReceived += DecoderOnDataReceived;
        decoder.Disconnected += DecoderOnDisconnected;
        decoder.Start();
    }

    public EndPoint RemoteEndPoint => _socket.RemoteEndPoint!;

    public void Close()
    {
        try
        {
            _socket.Shutdown(SocketShutdown.Both);
        }
        catch
        {
            // ignored
        }

        _socket.Close();
    }

    private void DecoderOnDataReceived(byte[] data)
    {
        // 处理接收到的数据
        
        _dataReceived(this, data);
    }

    private void DecoderOnDisconnected(Socket soc)
    {
        _disconnected(this);
    }
}