using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Valve.Sockets;

public class Main : MonoBehaviour
{
    private void Awake()
    {
    }

    NetworkingSockets client;
    StatusCallback status;
    uint connection;

    void  Start()
    {
        Library.Initialize();

        ushort port = 8080;
        this.client = new NetworkingSockets();
        Address address = new Address();

        address.SetAddress("::1", port);

        this.connection = client.Connect(ref address);

        this.status = (info, context) => {
            switch (info.connectionInfo.state)
            {
                case ConnectionState.None:
                    break;

                case ConnectionState.Connected:
                    Debug.Log("Client connected to server - ID: " + connection);
                    break;

                case ConnectionState.ClosedByPeer:
                    client.CloseConnection(connection);
                    Debug.Log("Client disconnected from server");
                    break;

                case ConnectionState.ProblemDetectedLocally:
                    client.CloseConnection(connection);
                    Debug.Log("Client unable to connect");
                    break;
            }
        };
        
        DebugCallback debug = (type, message) => {
            Debug.Log("Debug - Type: " + type + ", Message: " + message);
        };

        NetworkingUtils utils = new NetworkingUtils();

        utils.SetDebugCallback(DebugType.Everything, debug);
    }
    const int maxMessages = 20;

    NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];

    void Update()
    {
        if (client == null)
        {
            return;
        }

        client.DispatchCallback(status);
        int netMessagesCount = client.ReceiveMessagesOnConnection(connection, netMessages, maxMessages);

        if (netMessagesCount > 0)
        {
            for (int i = 0; i < netMessagesCount; i++)
            {
                ref NetworkingMessage netMessage = ref netMessages[i];

                Debug.Log("Message received from server - Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);

                netMessage.Destroy();
            }
        }
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Q))
        {
            byte[] data = new byte[64];
            client.SendMessageToConnection(connection, data);    
        }
    }

    private void OnApplicationQuit()
    {
        Library.Deinitialize();
    }
}
