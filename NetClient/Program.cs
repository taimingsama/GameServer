using System.Net;
using System.Net.Sockets;
using System.Text;
using GameCommon.Network;
using GameCommon.Protos;
using Google.Protobuf;

var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

var ipEndPoint = new IPEndPoint(IPAddress.Loopback, 32510);
await socket.ConnectAsync(ipEndPoint);

Console.WriteLine("已连接到服务器：" + socket.RemoteEndPoint);

// var v = new Vector3 { X = 100, Y = 200, Z = 300 };
// await SendMessage(v.ToByteArray(), socket);

var package = new Package
{
    Request = new Request
    {
        UserRegisterRequest = new UserRegisterRequest
        {
            Username = "TaiMing",
            Password = "123456",
        }
    }
};

await SendMessage(package.ToByteArray(), socket);

Console.ReadKey();

static async Task SendMessage(byte[] messageBody, Socket socket)
{
    var totalLen = messageBody.Length + 4; // 消息体长度 + 消息总长度字段长度

    // 创建字节缓冲区
    var messsageByteBuffer = ByteBuffer.Allocate(totalLen, true);

    // 写入消息体总长度
    messsageByteBuffer.WriteBytes(BitConverter.GetBytes(totalLen - 4)); // 减去消息总长度字段本身的4字节（小端序）
    // 写入消息体
    messsageByteBuffer.WriteBytes(messageBody);

    await socket.SendAsync(messsageByteBuffer.ToArray(), SocketFlags.None);
}