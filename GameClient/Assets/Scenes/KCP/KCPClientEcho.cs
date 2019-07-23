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

    KcpSession kcpSession;

    void Start()
    {
        ConnectButton.onClick.AddListener(Connection);
        SendButton.onClick.AddListener(SendTxt);
    }

    public void Connection()
    {
        kcpSession = new KcpSession();
        kcpSession.Connect("10.225.14.43",18555);
    }

    public void SendTxt()
    {
        string str = InputField.text;
         for (int i = 0; i < 100000; i++)
         {
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(UnityEngine.Random.Range(0,100000000).ToString());
            //byte[] bs = System.Text.Encoding.UTF8.GetBytes(str);
            kcpSession.KcpSend(bs, 0, bs.Length);
        }
    }

    public void OnDestroy()
    {
        kcpSession.Dispose();
    }

    public void Update()
    {

    }
}
