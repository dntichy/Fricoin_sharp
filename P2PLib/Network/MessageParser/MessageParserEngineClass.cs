using System;
using System.Collections.Generic;
using Engine.Network.MessageParser;
using P2PLib.Network.Components.Enums;
using P2PLib.Network.Components.Interfaces;
using P2PLib.Network.MessageParser.Messages;

namespace P2PLib.Network.MessageParser
{
    public class MessageParserEngineClass : IMessageParserEngine
    {
        private Dictionary<int, IMessage> mPossibleMessages;

        public MessageParserEngineClass()
        {
            mPossibleMessages = new Dictionary<int, IMessage>();
            //add the standard messages
            mPossibleMessages[((int) MessageType.RegisterMessage)] = new RegisterMessage();
            mPossibleMessages[((int) MessageType.TextDataMessage)] = new TextMessage();
            mPossibleMessages[((int) MessageType.UnregisterMessage)] = new UnregisterMessage();
            mPossibleMessages[((int) MessageType.ResgisteredClientsListMessage)] = new RegisteredClientsListMessage();
            mPossibleMessages[((int) MessageType.CommandMessage)] = new CommandMessage();

        }

        public bool ContainsMessageType(int messageType)
        {
            return mPossibleMessages.ContainsKey(messageType);
        }

        public bool AddMessageType(int messageType, IMessage messageInstance)
        {
            if (messageInstance == null)
            {
                System.Diagnostics.Debug.WriteLine("Null instance supplied!");
                return false;
            }

            if (mPossibleMessages.ContainsKey(messageType))
            {
                System.Diagnostics.Debug.WriteLine("Message type already registered with this parser engine!");
                return false;
            }

            mPossibleMessages[messageType] = messageInstance;
            return true;
        }

        public bool ReplaceMessageType(int messageType, IMessage messageInstance)
        {
            if (messageInstance == null)
            {
                System.Diagnostics.Debug.WriteLine("Null instance supplied!");
                return false;
            }

            if (mPossibleMessages.ContainsKey(messageType))
            {
                mPossibleMessages[messageType] = messageInstance;
                return true;
            }

            System.Diagnostics.Debug.WriteLine("Message type not yet registered with this parser engine!");

            return false;
        }

        public void RemoveMessageType(int messageType)
        {
            if (mPossibleMessages.ContainsKey(messageType))
                mPossibleMessages.Remove(messageType);
        }

        public IMessage ParseMessage(Byte[] data)
        {
            foreach (KeyValuePair<int, IMessage> messageByType in mPossibleMessages)
            {
                if (messageByType.Value.Parse(data))
                    return messageByType.Value.Clone();
            }

            return null; //TODO -> use this instead? : new EmptyMessage();
        }
    }
}