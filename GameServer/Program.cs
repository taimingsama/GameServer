using GameCommon.Network;
using GameCommon.Protos;
using GameServer.Network;

var netService = new NetService();

netService.Init(32510);

netService.Start();

MessageRouter.Instance.Subscribe<Vector3>((c, v) => { Console.WriteLine(v); });
MessageRouter.Instance.Subscribe<Vector3>((c, v) => { Console.WriteLine(v); });
MessageRouter.Instance.Subscribe<Vector3>((c, v) => { Console.WriteLine(v); });

MessageRouter.Instance.Subscribe<UserRegisterRequest>((c, m) => { Console.WriteLine(m); });
MessageRouter.Instance.Subscribe<UserRegisterRequest>((c, m) => { Console.WriteLine(c); });

await MessageRouter.Instance.Start();

Console.ReadKey();

await MessageRouter.Instance.Stop();