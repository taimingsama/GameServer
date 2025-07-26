using System.Diagnostics;
using Google.Protobuf;

namespace GameCommon.Network;

public class MessageRouter : Singleton<MessageRouter>
{
    public delegate void ProcessMessageHandler<in TMessage>(NetConnection connection, TMessage message)
        where TMessage : IMessage;

    private record MessageUnit(NetConnection Connection, IMessage Message);

    private readonly Queue<MessageUnit> _messageUnitQueue = new();
    private readonly Dictionary<Type, Delegate?> _messageTypeToHandler = new();

    public void AddMessage(NetConnection connection, IMessage message)
    {
        _messageUnitQueue.Enqueue(new MessageUnit(connection, message));

        Console.WriteLine(_messageUnitQueue.Count);
    }

    public void Subscribe<TMessage>(ProcessMessageHandler<TMessage> processMessageHandler) where TMessage : IMessage
    {
        var messageType = typeof(TMessage);

        if (_messageTypeToHandler.ContainsKey(messageType))
        {
            _messageTypeToHandler[messageType] = Delegate.Combine(_messageTypeToHandler[messageType], processMessageHandler);
        }
        else
        {
            _messageTypeToHandler.Add(messageType, processMessageHandler);
        }

        Console.WriteLine($"{messageType}委托长度 : {_messageTypeToHandler[messageType]?.GetInvocationList().Length ?? 0}");
    }

    public void Unsubscribe<TMessage>(ProcessMessageHandler<TMessage> processMessageHandler) where TMessage : IMessage
    {
        var messageType = typeof(TMessage);

        if (_messageTypeToHandler.ContainsKey(messageType))
        {
            _messageTypeToHandler[messageType] = Delegate.Remove(_messageTypeToHandler[messageType], processMessageHandler);
        }
    }
}