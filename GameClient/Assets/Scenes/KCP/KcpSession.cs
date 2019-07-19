using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using KcpProject;
using Random = System.Random;

public class KcpSession 
{
    private UInt32 mNextUpdateTime = 0;
    bool in_connect_stage_ = false;
    bool connect_succeed_ = false;
    long lastConnectTime;
    private Socket mSocket = null;
    private KCP mKCP = null;
    const string ASIO_KCP_CONNECT_PACKET = "asio_kcp_connect_package get_conv";
    const string ASIO_KCP_SEND_BACK_CONV_PACKET = "asio_kcp_connect_back_package get_conv:";
    const string ASIO_KCP_DISCONNECT_PACKET = "asio_kcp_disconnect_packag";
    private ByteBuffer mRecvBuffer = ByteBuffer.Allocate(1024 * 32);
    byte[] buffer = new byte[1024];

    public void Connect(string host,int port)
    {
        mRecvBuffer.Clear();
        var endpoint = IPAddress.Parse(host);
        mSocket = new Socket(endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        mSocket.BeginConnect(endpoint, port, ConnectCallBack, mSocket);
    }

    public void ConnectCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            in_connect_stage_ = true;
            socket.BeginReceive(buffer, 0, buffer.Length, 0, ReveiveCallBack, socket);
            Debug.Log("Socket Connect Succ");
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Receive fail:" + ex.ToString());

        }
    }

    public void ReveiveCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            int rn =  mSocket.Receive(mRecvBuffer.RawBuffer, mRecvBuffer.WriterIndex, mRecvBuffer.WritableBytes, SocketFlags.None);
            mRecvBuffer.WriterIndex += rn;
            socket.BeginReceive(mRecvBuffer.RawBuffer, 0, buffer.Length, 0, ReveiveCallBack, socket);
            Debug.Log("Receive -->" + rn);
            if (connect_succeed_)
            {
                byte[] buffs = new byte[1024];
                KcpReceive(buffs, 0, buffs.Length);
            }

        }
        catch (Exception ex)
        {
            Debug.Log("Socket Receive fail:" + ex.ToString());
        }
    }

    public void Send(byte[] sendBytes)
    {
        mSocket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, mSocket);
    }

    private void rawSend(byte[] data, int length)
    {
        if (mSocket != null)
        {
            mSocket.BeginSend(data, 0, length, 0, SendCallback, mSocket);
        }
    }

    public void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket mSocket = (Socket)ar.AsyncState;
            int count = mSocket.EndSend(ar);
            Debug.Log("Socket Send" + count);
        }
        catch (Exception ex)
        {

        }
    }

    public int KcpSend(byte[] data, int index, int length)
    {
        if (mSocket == null)
            return -1;

        if (mKCP.WaitSnd >= mKCP.SndWnd)
        {
            return 0;
        }

        mNextUpdateTime = 0;

        var n = mKCP.Send(data, index, length);

        //if (mKCP.WaitSnd >= mKCP.SndWnd)
        //{
            mKCP.Flush(false);
        //}
        return n;
    }

    public int KcpReceive(byte[] data, int index, int length)
    {
        // 上次剩下的部分
        if (mRecvBuffer.ReadableBytes > 0)
        {
            var recvBytes = Math.Min(mRecvBuffer.ReadableBytes, length);
            Buffer.BlockCopy(mRecvBuffer.RawBuffer, mRecvBuffer.ReaderIndex, data, index, recvBytes);
            mRecvBuffer.ReaderIndex += recvBytes;
            // 读完重置读写指针
            if (mRecvBuffer.ReaderIndex == mRecvBuffer.WriterIndex)
            {
                mRecvBuffer.Clear();
            }
            return recvBytes;
        }

        var inputN = mKCP.Input(mRecvBuffer.RawBuffer, mRecvBuffer.ReaderIndex, mRecvBuffer.ReadableBytes, true, true);
        if (inputN < 0)
        {
            mRecvBuffer.Clear();
            return inputN;
        }
        mRecvBuffer.Clear();

        // 读完所有完整的消息
        for (; ; )
        {
            var size = mKCP.PeekSize();
            if (size <= 0) break;

            mRecvBuffer.EnsureWritableBytes(size);

            var n = mKCP.Recv(mRecvBuffer.RawBuffer, mRecvBuffer.WriterIndex, size);
            if (n > 0) mRecvBuffer.WriterIndex += n;
        }

        // 有数据待接收
        if (mRecvBuffer.ReadableBytes > 0)
        {
            return KcpReceive(data, index, length);
        }

        return 0;
    }

    public void CreateKCP(uint code)
    {
        mKCP = new KCP(code, rawSend);
    }

    public void Update()
    {
        if(in_connect_stage_)
        {
            long now = DateTime.Now.Ticks;
            if (now - lastConnectTime > 10000000)
            {
                Debug.Log("Send connect------>");
                lastConnectTime = now;
                char[] arr = ASIO_KCP_CONNECT_PACKET.ToCharArray();
                char[] arr2 = new char[arr.Length + 1];
                for (int i = 0; i < arr.Length; i ++)
                {
                    arr2[i] = arr[i];
                }
                arr2[arr2.Length - 1] = '\0';
                Send(System.Text.Encoding.UTF8.GetBytes(arr2));
            }
            string str = System.Text.Encoding.UTF8.GetString(mRecvBuffer.RawBuffer);
            Debug.Log("Reveive connect :" + str);
            if(str.StartsWith(ASIO_KCP_SEND_BACK_CONV_PACKET))
            {
                mRecvBuffer.Clear();
                in_connect_stage_ = false;
                string retCodeStr = str.Replace(ASIO_KCP_SEND_BACK_CONV_PACKET, "");
                uint retCodeInt = uint.Parse(retCodeStr);
                CreateKCP(retCodeInt);
                connect_succeed_ = true;
                Debug.Log("Reveive KCP Index " + retCodeInt);
            }
        }
    }
}
