using GameServer.Network;

var netService = new NetService();

netService.Init(32510);

netService.Start();

Console.ReadKey();