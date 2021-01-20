using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId = 0;
    public TCP tcp;

    // A delegate basically says "feel free to assign any method to this delegate if the signature matches"
    // Since our HandleData method has a "using" that matches the "Packet _packet" signature,
    //  we know that is where the packet is being handled, hence this delegate's name
    private delegate void PacketHandler(Packet _packet);
    // A new dictionary to keep track of our packet handlers and their ids
    private static Dictionary<int, PacketHandler> packetHandlers;

    // Make sure there is only once instance of this client
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    // Create a new instace of TCP
    private void Start()
    {
        tcp = new TCP();
    }

    public void ConnectToServer()
    {
        InitializeClientData();
        tcp.Connect();
    }

    public class TCP
    {
        // Will store instance we get in the server's connect callback
        public TcpClient socket;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect()
        {
            // Initialize new TcpClient and its buffers to be used for the socket
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            // This buffer is for the TCP connection
            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        // For connecting
        private void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);

            // Check if we are connected
            if (!socket.Connected)
            {
                return;
            }

            // If we're connected then get stream to read data
            stream = socket.GetStream();

            receivedData = new Packet();

            // Begin reading from our stream of data
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        // Send packet to the server
        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error sending data to server via TCP: {_ex}");
            }
        }

        // For receiving
        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                // The size of the data represented as an int
                int _byteLength = stream.EndRead(_result);

                // If there is no data we would disconnect
                if (_byteLength <= 0)
                {
                    // TODO: disconnect
                    return;
                }

                // A new array for storing data; Size is based on what we received
                byte[] _data = new byte[_byteLength];
                // Copy the data we received to the _data byte array
                Array.Copy(receiveBuffer, _data, _byteLength);

                // Before resetting, we want to make sure none of our packets get split up
                //  or else we will lose data; HandleData will determine reset
                receivedData.Reset(HandleData(_data));

                // Continue reading data from the stream
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error receiving TCP data: {_ex}");
                // TODO: disconnect
            }
        }

        // Determine whether or not we have handled all data
        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            receivedData.SetBytes(_data);

            // Check if receivedData has 4 or more unread bytes, which indicates we have the start of one of our packets
            // An int consists of 4 bytes, and the first data of any packet is an int which represents its length
            if (receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt();
                // If there is no more data then reset the received data
                if (_packetLength <= 0)
                {
                    return true;
                }
            }

            // If we still have data to read, and we have enough room to read that data
            //  then continue receiving the next complete packet
            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                // Store the bytes of the packet into a new byte array
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                // Since our code won't be run on the same thread, execute it on main thread
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        // Pass our handler a packet
                        packetHandlers[_packetId](_packet);
                    }
                });

                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    // If there is no more data then reset the received data
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true;
            }

            return false;
        }
    }

    // Initialize all our handlers
    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome, ClientHandle.Welcome }
        };
        Debug.Log("Initialized packets.");
    }
}
