using Google.Protobuf;

namespace GameCommon.Network;

public class MessageRouter : Singleton<MessageRouter>
{
    public delegate void MessageHandler<in TMessage>(NetConnection connection, TMessage message)
        where TMessage : IMessage;

    private record MessageUnit(NetConnection Connection, IMessage Message);

    private readonly Queue<MessageUnit> _messageUnitQueue = new();
    private readonly Dictionary<Type, Delegate?> _messageTypeToHandler = new();

    public void AddMessage(NetConnection connection, IMessage message)
    {
        _messageUnitQueue.Enqueue(new MessageUnit(connection, message));

        Console.WriteLine(_messageUnitQueue.Count);
    }

    public void Subscribe<TMessage>(MessageHandler<TMessage> messageHandler) where TMessage : IMessage
    {
        var messageType = typeof(TMessage);

        if (_messageTypeToHandler.ContainsKey(messageType))
        {
            _messageTypeToHandler[messageType] = Delegate.Combine(_messageTypeToHandler[messageType], messageHandler);
        }
        else
        {
            _messageTypeToHandler.Add(messageType, messageHandler);
        }
    }

    public void Unsubscribe<TMessage>(MessageHandler<TMessage> messageHandler) where TMessage : IMessage
    {
        var messageType = typeof(TMessage);

        if (_messageTypeToHandler.ContainsKey(messageType))
        {
            _messageTypeToHandler[messageType] = Delegate.Remove(_messageTypeToHandler[messageType], messageHandler);
        }
    }
}