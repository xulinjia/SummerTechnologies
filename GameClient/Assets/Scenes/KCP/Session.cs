using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Session
{
    public abstract void Connect(string host,int port);
    public abstract void Close();
    public abstract int Send(byte[] data);
    public abstract int Receive(byte[] data);
}
