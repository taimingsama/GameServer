using System.Net;
using System.Net.Sockets;
using GameCommon.Protos;
using Google.Protobuf;

namespace GameCommon.Network;

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

    public void Send(byte[] data, int offset, int size)
    {
        if (_socket.Connected)
        {
            _socket.BeginSend(data, offset, size, SocketFlags.None, SocketOnSent, null);
        }
    }

    public void Send(Package package)
    {
        using var memoryStream = new MemoryStream();

        memoryStream.Write(BitConverter.GetBytes(package.CalculateSize()), 0, sizeof(int));
        package.WriteTo(memoryStream);

        var targetSendBytes = memoryStream.GetBuffer();

        Send(targetSendBytes, 0, targetSendBytes.Length);
    }

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

    private void SocketOnSent(IAsyncResult ar)
    {
        var sentLen = _socket.EndSend(ar);
    }
}