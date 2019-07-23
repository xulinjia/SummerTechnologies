using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using KcpProject;
using Random = System.Random;

public class KcpSession : Session
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
    System.Timers.Timer connectTimer;
    System.Timers.Timer updateTimer;

    public override void Close()
    {
        if (connectTimer != null)
        {
            connectTimer.Stop();
            connectTimer.Dispose();
        }
        if (updateTimer != null)
        {
            updateTimer.Stop();
            updateTimer.Dispose();
        }
        if(mSocket != null)
        {
            mSocket.Close();
        }

        if(mKCP != null)
        {
            mKCP = null;
        }
    }

    public override void Connect(string host,int port)
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
            TryConnectKcp(this, null);
            connectTimer = new System.Timers.Timer(1000);
            connectTimer.Elapsed += new System.Timers.ElapsedEventHandler(TryConnectKcp);
            connectTimer.AutoReset = true;//设置是执行一次（false）还是一直执行(true);
            connectTimer.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件;

            updateTimer = new System.Timers.Timer();
            updateTimer.Elapsed += new System.Timers.ElapsedEventHandler((obj, eventArg) =>
            {
                Update();
            });
            updateTimer.Interval = 10;//毫秒 1秒=1000毫秒
            updateTimer.Enabled = true;//必须加上
            updateTimer.AutoReset = true;//执行一次 false，一直执行true 
            Debug.Log("Socket Connect Succ");
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Receive fail:" + ex.ToString());

        }
    }

    private void SocketSend(byte[] sendBytes)
    {
        mSocket.BeginSend(sendBytes, 0, sendBytes.Length, SocketFlags.None, SendCallback, mSocket);
    }

    private void rawSend(byte[] data, int length)
    {
        if (mSocket != null)
        {
            mSocket.BeginSend(data, 0, length, 0, SendCallback, mSocket);
            string str = System.Text.Encoding.UTF8.GetString(data);
            Debug.Log("kcpSend Data -->" + str);
        }
    }

    static int allCount = 0;
    public void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket mSocket = (Socket)ar.AsyncState;
            int count = mSocket.EndSend(ar);
            allCount += count;
            Debug.Log("Socket Send -->" + count + ":" + allCount);

        }
        catch (Exception ex)
        {

        }
    }

    public override int Send(byte[] data)
    {
        if (mSocket == null)
            return -1;
        if (mKCP == null)
            return -1;
        if (mKCP.WaitSnd >= mKCP.SndWnd)
        {
            return 0;
        }
        mNextUpdateTime = 0;

        var n = mKCP.Send(data);
        if (mKCP.WaitSnd >= mKCP.SndWnd)
        {
            mKCP.Flush(true);
        }
        return n;
    }

    public override int Receive(byte[] data)
    {
        // 上次剩下的部分
        if (mRecvBuffer.ReadableBytes > 0)
        {
            var recvBytes = Math.Min(mRecvBuffer.ReadableBytes, data.Length);
            Buffer.BlockCopy(mRecvBuffer.RawBuffer, mRecvBuffer.ReaderIndex, data, 0, recvBytes);
            mRecvBuffer.ReaderIndex += recvBytes;
            // 读完重置读写指针
            if (mRecvBuffer.ReaderIndex == mRecvBuffer.WriterIndex)
            {
                mRecvBuffer.Clear();
            }
            return recvBytes;
        }

        if (mSocket == null)
            return -1;

        if (!mSocket.Poll(0, SelectMode.SelectRead))
        {
            return 0;
        }

        var rn = 0;
        try
        {
            rn = mSocket.Receive(mRecvBuffer.RawBuffer, mRecvBuffer.WriterIndex, mRecvBuffer.WritableBytes, SocketFlags.None);
            string str = System.Text.Encoding.UTF8.GetString(mRecvBuffer.RawBuffer);
            if (str.StartsWith(ASIO_KCP_SEND_BACK_CONV_PACKET))
            {
                if (in_connect_stage_)
                {
                    connectTimer.Stop();
                    connectTimer.Dispose();
                    in_connect_stage_ = false;
                    string retCodeStr = str.Replace(ASIO_KCP_SEND_BACK_CONV_PACKET, "");
                    uint retCodeInt = uint.Parse(retCodeStr);
                    CreateKCP(retCodeInt);
                    mRecvBuffer.Clear();
                    Debug.Log("KCP 连接成功" + retCodeInt);
                }
                else
                {
                    Debug.Log("ERROR：KCP 已经连接上了,重复的连接消息");
                }
                mRecvBuffer.Clear();
                return 0;
            }
            else if (str.StartsWith(ASIO_KCP_DISCONNECT_PACKET))
            {
                mRecvBuffer.Clear();
                return 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            rn = -1;
        }

        if (rn <= 0)
        {
            return rn;
        }
        mRecvBuffer.WriterIndex += rn;

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
            return Receive(data);
        }

        return 0;
    }

    public void CreateKCP(uint code)
    {
        mKCP = new KCP(code, rawSend);
    }

    public void TryConnectKcp(object source, System.Timers.ElapsedEventArgs e)
    {
        if (!in_connect_stage_)
            return;
        Debug.Log("Send connect------>");
        char[] arr = ASIO_KCP_CONNECT_PACKET.ToCharArray();
        char[] arr2 = new char[arr.Length + 1];
        for (int i = 0; i < arr.Length; i++)
        {
            arr2[i] = arr[i];
        }
        arr2[arr2.Length - 1] = '\0';
        SocketSend(System.Text.Encoding.UTF8.GetBytes(arr2));
        Debug.Log("Send--Connect");
    }

    public void Update()
    {
        if (mKCP == null)
            return;

        if (0 == mNextUpdateTime || mKCP.CurrentMS >= mNextUpdateTime)
        {
            mKCP.Update();
            mNextUpdateTime = mKCP.Check();
        }
    }
}
