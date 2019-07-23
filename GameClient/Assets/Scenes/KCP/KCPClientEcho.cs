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

    Session kcpSession;

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
            byte[] bs = System.Text.Encoding.Default.GetBytes(UnityEngine.Random.Range(0,100000000).ToString());
            kcpSession.Send(bs);
        }
    }

    public void OnDestroy()
    {
        kcpSession.Close();
    }

    byte[] b = new byte[1024];
    public void Update()
    {
        if(kcpSession != null)
        {
            int n = kcpSession.Receive(b);
            if(n > 0)
            {
                string str = System.Text.Encoding.Default.GetString(b, 0, n);
                Debug.Log("Reveive  :"+str);
            }
        }
    }
}
