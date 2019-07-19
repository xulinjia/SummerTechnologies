using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System;
using KcpProject;
using Random = System.Random;

public class ClientEcho : MonoBehaviour
{

    public Button ConnectButton;
    public InputField InputField;
    public Button SendButton;
    public Text text;

    Socket mSocket;
    byte[] readBuff = new byte[1024];
    string recvStr = "";
    private KCP mKCP = null;
    const string ASIO_KCP_CONNECT_PACKET = "asio_kcp_connect_package get_conv";
    const string ASIO_KCP_SEND_BACK_CONV_PACKET = "asio_kcp_connect_back_package get_conv:";
    const string ASIO_KCP_DISCONNECT_PACKET = "asio_kcp_disconnect_packag";

    // Start is called before the first frame update
    void Start()
    {
        ConnectButton.onClick.AddListener(Connection);
        SendButton.onClick.AddListener(Send);
    }

    public void Connection()
    {
        mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Udp);
        mSocket.BeginConnect("127.0.0.1", 8888, ConnectCallBack, mSocket);
        mKCP = new KCP((uint)(new Random().Next(1, Int32.MaxValue)), rawSend);
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
        catch(SocketException ex)
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
            recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);

            socket.BeginReceive(readBuff, 0, 1024, 0, ReveiveCallBack, socket);
        }
        catch(Exception ex)
        {
            Debug.Log("Socket Receive fail:" + ex.ToString());
        }
    }

    public void Send()
    {
        string sendStr = InputField.text;
        if (sendStr == null)
        {
            sendStr = "";
        }
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        mSocket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, mSocket);
    }

    private void rawSend(byte[] data, int length)
    {
        if (mSocket != null)
        {
            mSocket.Send(data, length, SocketFlags.None);
        }
    }

    public void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket scoket = (Socket)ar.AsyncState;
            int count = mSocket.EndSend(ar);
            Debug.Log("Socket Send" + count);
        }
        catch(Exception ex)
        {

        }
    }

    public void Update()
    {
        text.text = recvStr;
    }
}
