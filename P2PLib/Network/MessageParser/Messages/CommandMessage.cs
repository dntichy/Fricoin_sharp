using ChainUtils;
using Engine.Network.MessageParser;
using P2PLib.Network.Components.Enums;
using P2PLib.Network.Components.Interfaces;
using System;
using System.Text;
using System.Xml;

namespace P2PLib.Network.MessageParser.Messages
{
    public class CommandMessage : IMessage
    {

        public CommandType Command { get; set; }
        public int Type
        {
            get { return (int)MessageType.CommandMessage; }
        }
        private ClientDetails mClient;
        public ClientDetails Client
        {
            get { return mClient; }
            set { mClient = value; }
        }

        private byte[] mData;
        public byte[] Data
        {
            get { return mData; }
            set { mData = value; }
        }

        public Byte[] GetMessagePacket()
        {
            String textResult = "";
            textResult += "<message>";
            textResult += "<type>" + Type + "</type>";
            textResult += "<command>" + Command + "</command>";
            textResult += "<data>" + HexadecimalEncoding.ToHexString(Data) + "</data>";
            textResult += "<clientdetails><name>" + mClient.ClientName + "</name><ipaddress>" + mClient.ClientIPAddress + "</ipaddress><listenport>" + mClient.ClientListenPort + "</listenport></clientdetails>";
            textResult += "</message>";

            return Encoding.UTF8.GetBytes(textResult);
        }




        public bool Parse(Byte[] data)
        {
            //parse the message from "incoming data packet"
            String messageContent = Encoding.UTF8.GetString(data);
            //////////////////////////////////////////////////////////////////////////
            //Data validation
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(messageContent);
            }
            catch (XmlException ex)
            {
                System.Diagnostics.Debug.WriteLine("There was an xml parsing error in the received message : " +
                                                   ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("There was an error in the received message : " + ex.Message);
                return false;
            }

            MessageType type = MessageType.EmptyMessage;

            XmlElement messageElement = xmlDoc.DocumentElement;
            if (messageElement.Name == "message")
            {
                foreach (XmlNode node in messageElement.ChildNodes)
                {
                    if (node.Name == "type")
                    {
                        type = (MessageType)Enum.Parse(typeof(MessageType), node.InnerText);
                        break;
                    }
                }
            }

            if (type != MessageType.CommandMessage)
            {
                Console.WriteLine("The supplied data was the wrong message type!");
                return false;
            }


            foreach (XmlNode node in messageElement.ChildNodes)
            {
                if (node.Name == "data")
                {
                    this.Data = HexadecimalEncoding.FromHexStringToByte(node.InnerText);
                }
                else if (node.Name == "command")
                {
                    Enum.TryParse(node.InnerText, out CommandType command);

                    Command = command;
                }
                else if (node.Name == "clientdetails")
                {
                    mClient = new ClientDetails();

                    foreach (XmlNode detailsNode in node.ChildNodes)
                    {
                        if (detailsNode.Name == "name")
                        {
                            mClient.ClientName = detailsNode.InnerText;
                        }
                        else if (detailsNode.Name == "ipaddress")
                        {
                            mClient.ClientIPAddress = detailsNode.InnerText;
                        }
                        else if (detailsNode.Name == "listenport")
                        {
                            mClient.ClientListenPort = int.Parse(detailsNode.InnerText);
                        }
                    }
                }
            }

            //////////////////////////////////////////////////////////////////////////
            return true;
        }

        public IMessage Clone()
        {
            CommandMessage result = new CommandMessage();
            result.Client = Client;
            result.Data = Data;
            result.Command = Command;
            return result;
        }
    }
}