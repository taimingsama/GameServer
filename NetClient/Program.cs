using System.Net;
using System.Net.Sockets;
using System.Text;
using GameCommon.Network;
using GameCommon.Protos;
using Google.Protobuf;

var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

var ipEndPoint = new IPEndPoint(IPAddress.Loopback, 32510);
await socket.ConnectAsync(ipEndPoint);

var connection = new NetConnection(socket,
    (receiver, data) => Console.WriteLine("收到消息"),
    connection => Console.WriteLine("连接已断开"));

Console.WriteLine("已连接到服务器：" + socket.RemoteEndPoint);

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

connection.Send(package);

Console.ReadKey();

connection.Close();
