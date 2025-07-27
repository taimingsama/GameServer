using System.Collections.Concurrent;
using GameCommon.Protos;
using Google.Protobuf;

namespace GameCommon.Network;

public class MessageRouter : Singleton<MessageRouter>
{
    private record PackageUnit(NetConnection Connection, Package Package);

    public delegate void ProcessMessageHandler<in TMessage>(NetConnection connection, TMessage message)
        where TMessage : IMessage;

    private readonly ConcurrentQueue<PackageUnit> _packageUnitQueue = new();
    private readonly Dictionary<Type, Delegate?> _messageTypeToHandler = new();
    private int _threadCount;
    private int _workerCount;
    private bool _isRunning;
    private AutoResetEvent _existMessageEvent = new(false);

    public async Task Start(int targetThreadCount = 8)
    {
        _threadCount = Math.Clamp(targetThreadCount, 1, 256);

        _isRunning = true;

        for (var i = 0; i < _threadCount; i++)
        {
            Task.Factory.StartNew(MessageWork, TaskCreationOptions.LongRunning);
        }

        while (_workerCount < _threadCount)
        {
            await Task.Delay(100);
        }

        Console.WriteLine($"All MessageWork Started");
    }

    public async Task Stop()
    {
        _isRunning = false;
        _packageUnitQueue.Clear();

        while (_workerCount > 0)
        {
            _existMessageEvent.Set();
            await Task.Delay(10);
        }

        Console.WriteLine($"All MessageWork End");
    }

    public void AddMessage(NetConnection connection, Package message)
    {
        _packageUnitQueue.Enqueue(new PackageUnit(connection, message));

        _existMessageEvent.Set();

        // Console.WriteLine(_messageUnitQueue.Count);
    }

    public void Subscribe<TMessage>(ProcessMessageHandler<TMessage> processMessageHandler) where TMessage : IMessage
    {
        var messageType = typeof(TMessage);

        if (_messageTypeToHandler.ContainsKey(messageType))
        {
            _messageTypeToHandler[messageType] =
                Delegate.Combine(_messageTypeToHandler[messageType], processMessageHandler);
        }
        else
        {
            _messageTypeToHandler.Add(messageType, processMessageHandler);
        }

        // Console.WriteLine($"{messageType}委托长度 : {_messageTypeToHandler[messageType]?.GetInvocationList().Length ?? 0}");
    }

    public void Unsubscribe<TMessage>(ProcessMessageHandler<TMessage> processMessageHandler) where TMessage : IMessage
    {
        var messageType = typeof(TMessage);

        if (_messageTypeToHandler.ContainsKey(messageType))
        {
            _messageTypeToHandler[messageType] =
                Delegate.Remove(_messageTypeToHandler[messageType], processMessageHandler);
        }
    }

    private void Publish<TMessage>(NetConnection connection, TMessage message) where TMessage : IMessage
    {
        var messageType = typeof(TMessage);

        if (_messageTypeToHandler.TryGetValue(messageType, out var handler))
        {
            ((ProcessMessageHandler<TMessage>)handler!).Invoke(connection, message);
        }
        else
        {
            Console.WriteLine($"No handler found for message type: {messageType}");
        }
    }

    private void MessageWork()
    {
        Console.WriteLine($"MessageWork thread started");
        Interlocked.Increment(ref _workerCount);

        try
        {
            while (_isRunning)
            {
                if (_packageUnitQueue.TryDequeue(out var messageUnit))
                {
                    var package = messageUnit.Package;

                    if (package.Request != null)
                    {
                        DoRequest(messageUnit.Connection, package.Request);
                    }

                    if (package.Response != null)
                    {
                        DoResponse(messageUnit.Connection, package.Response);
                    }
                }
                else
                {
                    _existMessageEvent.WaitOne();
                }
            }
        }
        finally
        {
            Interlocked.Decrement(ref _workerCount);
            Console.WriteLine($"MessageWork thread end");
        }
    }

    private void DoRequest(NetConnection connection, Request request)
    {
        if (request.UserRegisterRequest != null)
        {
            Publish(connection, request.UserRegisterRequest);
        }

        if (request.UserLoginRequest != null)
        {
            Publish(connection, request.UserLoginRequest);
        }
    }

    private void DoResponse(NetConnection connection, Response response)
    {
        if (response.UserRegisterResponse != null)
        {
            Publish(connection, response.UserRegisterResponse);
        }

        if (response.UserLoginResponse != null)
        {
            Publish(connection, response.UserLoginResponse);
        }
    }
}