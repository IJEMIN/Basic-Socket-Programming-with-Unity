﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

public class Chat : MonoBehaviour
{
    public TransportTCP m_transport;

    public ChatText commentTextPrefab;

    public Transform commentHolder;

    public string m_hostAddress = "127.0.0.1";

    public int m_port = 50763;

    private bool m_isServer = false;

    // Use this for initialization
    void Start()
    {
        m_transport.onStateChanged += OnEventHandling;
    }


    IEnumerator UpdateChatting()
    {
        while (true)
        {
            byte[] buffer = new byte[1400];

            int recvSize = m_transport.Receive(ref buffer, buffer.Length);
            if (recvSize > 0)
            {
                string message = System.Text.Encoding.UTF8.GetString(buffer);
                Debug.Log("Recv data:" + message);

                AddComment(message);
            }

            yield return null;
        }
    }


    public void Send(string message)
    {
        message = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message;

        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);

        m_transport.Send(buffer, buffer.Length);

        AddComment(message);
    }


    void AddComment(string message)
    {
        var newComment = Instantiate(commentTextPrefab, commentHolder);
        newComment.SetUp(message);
    }
    void OnApplicationQuit()
    {
        if (m_transport != null)
        {
            if (m_isServer)
            {
                m_transport.StopServer();
            }
            else
            {
                m_transport.Disconnect();
            }
        }
    }

    public void OnEventHandling(NetEventState state)
    {
        switch (state.type)
        {
            case NetEventType.Connect:
                AddComment("접속");
                Debug.Log("접속");
                break;

            case NetEventType.Disconnect:
                Debug.Log("접속 종료");
                AddComment("접속 종료");
                break;
        }
    }


    public void CreateRoom()
    {
        m_transport.StartServer(m_port, 1);
        m_isServer = true;
        StartCoroutine("UpdateChatting");
    }

    public void JoinChatRoom()
    {
        bool ret = m_transport.Connect(m_hostAddress, m_port);

        if (ret)
        {
           StartCoroutine("UpdateChatting");
        }
        else
        {
            Debug.LogError("Failed");
        }
    }


    public void Leave()
    {
        if (m_isServer == true)
        {
            m_transport.StopServer();
        }
        else
        {
            m_transport.Disconnect();
        }

        StopCoroutine("UpdateChatting");
    }


}