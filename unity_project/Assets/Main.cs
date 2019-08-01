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

    async void  Start()
    {
        Library.Initialize();

        await Task.Delay(1000);

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

    private void OnApplicationQuit()
    {
        Library.Deinitialize();
    }
}
