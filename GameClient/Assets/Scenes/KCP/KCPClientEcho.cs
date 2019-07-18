using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System;

public class KCPClientEcho : MonoBehaviour
{

    public Button ConnectButton;
    public InputField InputField;
    public Button SendButton;
    public Text text;

    Socket socket;
    byte[] readBuff = new byte[1024];
    string recvStr = "";
    // Start is called before the first frame update
    void Start()
    {
        ConnectButton.onClick.AddListener(Connection);
        SendButton.onClick.AddListener(Send);
    }

    public void Connection()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.BeginConnect("127.0.0.1", 8888, ConnectCallBack, socket);
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
        for (int i = 0; i < 10000; i++)
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
        }
    }

    public void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket scoket = (Socket)ar.AsyncState;
            int count = socket.EndSend(ar);
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
