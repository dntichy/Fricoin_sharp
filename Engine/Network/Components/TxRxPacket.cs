using System;
using System.Net.Sockets;

namespace Engine.Network
{
    public class TxRxPacket
    {
        public TxRxPacket(Socket socket)
        {
            mCurrentSocket = socket;
            mDataBuffer = new Byte[1024];
        }

        public TxRxPacket(Socket socket, int bufferSize)
        {
            mCurrentSocket = socket;
            mDataBuffer = new Byte[bufferSize];
        }

        public void StoreCurrentData()
        {
            int storedBufferLength = (mStoredBuffer != null) ? mStoredBuffer.Length : 0;
            byte[] newBuffer = new byte[storedBufferLength + mDataBuffer.Length];
            for (int i = 0; i < storedBufferLength; ++i)
            {
                newBuffer[i] = mStoredBuffer[i];
            }

            for (int i = 0; i < mDataBuffer.Length; ++i)
            {
                newBuffer[storedBufferLength + i] = mDataBuffer[i];
            }

            mStoredBuffer = newBuffer;
            mDataBuffer = null;
            mDataBuffer = new Byte[1024];
        }

        public Socket mCurrentSocket;
        public Byte[] mDataBuffer;
        public Byte[] mStoredBuffer;
    }
}

