using System;
using System.Text;
using System.Xml;
using Engine.Network;
using Engine.Network.Components.Interfaces;

namespace P2PLib.Network.MessageParser.Messages
{
    public class TextMessage : IMessage
    {
        private String mText;


        public /*MessageType*/ int Type
        {
            get { return (int) MessageType.TextDataMessage; }
        }


        public Byte[] GetMessagePacket()
        {
            String textResult = "";
            textResult += "<message>";
            textResult += "<type>" + Type + "</type>";
            textResult += "<text>" + Text + "</text>";
            textResult += "</message>";

            return ASCIIEncoding.UTF8.GetBytes(textResult);
        }


        public String Text
        {
            get { return mText; }
            set { mText = value; }
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
                        type = (MessageType) Enum.Parse(typeof(MessageType), node.InnerText);
                        break;
                    }
                }
            }

            if (type != MessageType.TextDataMessage)
            {
                System.Diagnostics.Debug.WriteLine("The supplied data was the wrong message type!");
                return false;
            }

            //////////////////////////////////////////////////////////////////////////
            // The real data parsing
            this.mText = "";


            foreach (XmlNode node in messageElement.ChildNodes)
            {
                if (node.Name == "text")
                {
                    this.mText = node.InnerText;
                }

                else if (node.Name == "textcolor")
                {
                    int a, r, g, b;
                    a = r = g = b = 0;
                    foreach (XmlNode detailsNode in node.ChildNodes)
                    {
                        if (detailsNode.Name == "a")
                        {
                            a = int.Parse(detailsNode.InnerText);
                        }
                        else if (detailsNode.Name == "r")
                        {
                            r = int.Parse(detailsNode.InnerText);
                        }
                        else if (detailsNode.Name == "g")
                        {
                            g = int.Parse(detailsNode.InnerText);
                        }
                        else if (detailsNode.Name == "b")
                        {
                            b = int.Parse(detailsNode.InnerText);
                        }
                    }
                }
                else if (node.Name == "position")
                {
                    foreach (XmlNode detailsNode in node.ChildNodes)
                    {
                        if (detailsNode.Name == "x")
                        {
                        }
                        else if (detailsNode.Name == "y")
                        {
                        }
                    }
                }
                else if (node.Name == "resolution")
                {
                    foreach (XmlNode detailsNode in node.ChildNodes)
                    {
                        if (detailsNode.Name == "width")
                        {
                        }
                        else if (detailsNode.Name == "height")
                        {
                        }
                    }
                }
            }

            //////////////////////////////////////////////////////////////////////////
            return true;
        }

        public IMessage Clone()
        {
            TextMessage result = new TextMessage();

            result.Text = mText;
            return result;
        }
    }
}