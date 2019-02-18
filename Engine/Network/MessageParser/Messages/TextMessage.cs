using System;
using System.Drawing;
using System.Text;
using System.Xml;
using Engine.Network.Components.Interfaces;

namespace Engine.Network.MessageParser.Messages
{
    public class TextMessage : IMessage
    {
        private String mText;
        private int mTextSize;
        private String mTextFont;
        private Color mTextColor;

        public /*MessageType*/int Type
        {
            get { return (int)MessageType.TextDataMessage; }
        }

 

        public Byte[] GetMessagePacket()
        {
            String textResult = "";
            textResult += "<message>";

            textResult += "<type>" + Type + "</type>";
            textResult += "<text>" + Text + "</text>";
            textResult += "<textsize>" + TextSize + "</textsize>";
            textResult += "<textfont>" + TextFont + "</textfont>";
            textResult += "<textcolor><a>" + TextColor.A + "</a><r>" + TextColor.R + "</r><g>" + TextColor.G + "</g><b>" + TextColor.B + "</b></textcolor>";

            textResult += "</message>";

            return ASCIIEncoding.UTF8.GetBytes(textResult);
        }

        public Color TextColor
        {
            get { return mTextColor; }
            set { mTextColor = value; }
        }

        public String Text
        {
            get { return mText; }
            set { mText = value; }
        }

        public int TextSize
        {
            get { return mTextSize; }
            set { mTextSize = value; }
        }

        public String TextFont
        {
            get { return mTextFont; }
            set { mTextFont = value; }
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
                System.Diagnostics.Debug.WriteLine("There was an xml parsing error in the received message : " + ex.Message);
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

            if (type != MessageType.TextDataMessage)
            {
                System.Diagnostics.Debug.WriteLine("The supplied data was the wrong message type!");
                return false;
            }

            //////////////////////////////////////////////////////////////////////////
            // The real data parsing
            this.mText = "";
            this.mTextColor = Color.Empty;
            this.mTextFont = "";
            this.mTextSize = 0;
        

            foreach (XmlNode node in messageElement.ChildNodes)
            {
                if (node.Name == "text")
                {
                    this.mText = node.InnerText;
                }
                else if (node.Name == "textsize")
                {
                    this.mTextSize = int.Parse(node.InnerText);
                }
                else if (node.Name == "textfont")
                {
                    this.mTextFont = node.InnerText;
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
                    this.mTextColor = System.Drawing.Color.FromArgb(a, r, g, b);
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
            result.TextSize = mTextSize;
            result.TextFont = mTextFont;
            result.TextColor = Color.FromArgb(mTextColor.A, mTextColor.R, mTextColor.G, mTextColor.B);
            return result;
        }
    }
}