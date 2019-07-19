using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;

public class ClientSocket
{
    byte[] readBuff = new byte[1024];
    Socket socket;
    public ClientSocket()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public void Connect()
    {
        socket.BeginConnect("10.225.14.43", 18555, ConnectCallBack, socket);
    }

    public void ConnectCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Succ");
            socket.BeginReceive(readBuff, 0, 1024, 0, ReveiveCallBack, socket);
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
            socket.BeginReceive(readBuff, 0, 1024, 0, ReveiveCallBack, socket);
        }
        catch (Exception ex)
        {
            Debug.Log("Socket Receive fail:" + ex.ToString());
        }
    }

    public void Send(byte[] sendBytes)
    {
      socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
    }

    public void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket scoket = (Socket)ar.AsyncState;
            int count = socket.EndSend(ar);
            Debug.Log("Socket Send" + count);
        }
        catch (Exception ex)
        {

        }
    }
}
