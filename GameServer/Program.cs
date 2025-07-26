using GameCommon.Network;
using GameCommon.Protos;
using GameServer.Network;

var netService = new NetService();

netService.Init(32510);

netService.Start();

MessageRouter.Instance.Subscribe<Vector3>((c, v) => { Console.WriteLine(v); });
MessageRouter.Instance.Subscribe<Vector3>((c, v) => { Console.WriteLine(v); });
MessageRouter.Instance.Subscribe<Vector3>((c, v) => { Console.WriteLine(v); });

Console.ReadKey();