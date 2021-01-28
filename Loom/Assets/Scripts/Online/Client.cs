using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Net;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;
    //public string ip = "127.0.0.1";
    public string ip = "67.165.178.128";
    public int port = 26950;
    public int myId = 0;
    public TCP tcp;
    public UDP udp;
    private bool isConnected = false;

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

    // Create a new instace of TCP and UDP
    private void Start()
    {
        tcp = new TCP();
        udp = new UDP();
    }

    // Unity editor does not properly close connections when leaving play mode until you enter play mode again
    // So close the connection manually or else the port will be locked
    private void OnApplicationQuit()
    {
        Disconnect();
    }

    // Connects our client to the server through TCP
    public void ConnectToServer()
    {
        InitializeClientData();

        isConnected = true;
        tcp.Connect();
    }

    // TCP setup for the client
    public class TCP
    {
        // Will store the TCP instance we create
        public TcpClient socket;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        // Will setup an instance of our client's TCP connection information to send to the server
        public void Connect()
        {
            // Initializes a new TcpClient and sets up its buffers
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            // This buffer is for the received data
            receiveBuffer = new byte[dataBufferSize];

            // Connect to the server using the TcpClient we created as the instance
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        // Prepares our connected data to be read
        private void ConnectCallback(IAsyncResult _result)
        {
            // The connected data
            socket.EndConnect(_result);

            // Check if we are connected to the server
            if (!socket.Connected)
            {
                return;
            }

            // If we're connected to the server then get the stream
            stream = socket.GetStream();

            // Initialize a packet that we can store our data in
            receivedData = new Packet();

            // Begin reading from our stream of data
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        // Sends TCP data to the server
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

        // Prepares our received data to be read
        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                // The size of the data represented as an int
                int _byteLength = stream.EndRead(_result);

                // If there is no data we would disconnect
                if (_byteLength <= 0)
                {
                    instance.Disconnect();
                    return;
                }

                // A new array for storing data; Size is based on what we received as an int
                byte[] _data = new byte[_byteLength];

                // Copy the data we received to the _data byte array
                Array.Copy(receiveBuffer, _data, _byteLength);

                // Before resetting, we want to make sure none of our packets get split up
                //  or else we will lose data; HandleData will determine when to reset
                receivedData.Reset(HandleData(_data));

                // Continue reading data from the stream
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error receiving TCP data: {_ex}");
                Disconnect();
            }
        }

        // Determine whether or not we have handled all data
        private bool HandleData(byte[] _data)
        {
            // Initialize a variable that will hold the length of our packet
            int _packetLength = 0;

            // Stores the data received in a packet as bytes
            receivedData.SetBytes(_data);

            // Check if receivedData (our packet) has 4 or more unread bytes, which indicates the start of our packet
            // An int consists of 4 bytes; This data is always at the beginning of a packet and represents its length
            if (receivedData.UnreadLength() >= 4)
            {
                // Since the beginning of the data is greater than or equal to 4 bytes, read its int for our packet's length
                _packetLength = receivedData.ReadInt();

                // If there is no more data, then HandleData returns true which allows the packet to be reset and reused
                if (_packetLength <= 0)
                {
                    return true;
                }
            }

            // If we still have data to read, and we have enough room to read that data, 
            //  then continue receiving the next complete packet
            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                // Store the bytes of the packet into a new byte array
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);

                // Since our code won't be run on the same thread, execute it on the main thread
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();

                        // Pass our handler a packet
                        packetHandlers[_packetId](_packet);
                    }
                });

                // Reset the packet length variable and determine if we will begin receiving the next complete packet
                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();

                    // If there is no more data, then HandleData returns true which allows the packet to be reset and reused
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

        // Close out our connection with the server through TCP
        public void Disconnect()
        {
            instance.Disconnect();
            
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    // UDP setup for the client
    public class UDP 
    {
        // Will store the UDP instance we create
        public UdpClient socket;

        // Will be used to pass to the socket our UDP instance
        public IPEndPoint endPoint;

        public UDP()
        {
            // Creates a UDP instance
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        // Will setup an instance of our client's UDP connection information to send to the server
        public void Connect(int _localPort)
        {
            // This UDP instance will use the client's local port
            socket = new UdpClient(_localPort);

            // Connect to the server using the UDP instance we created
            socket.Connect(endPoint);

            // Begin receiving data from the server
            socket.BeginReceive(ReceiveCallback, null);

            // This packet's sole purpose is to immediately initiate a connection with the server
            //  and open up the client's local ports so that they can receive messages
            using (Packet _packet = new Packet())
            {
                SendData(_packet);
            }
        }

        // Sends UDP data to the server
        public void SendData(Packet _packet)
        {
            try
            {
                // Because of the way UDP works, it's hard to give clients their own UDP id
                _packet.InsertInt(instance.myId);
                if (socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via UDP: {_ex}");
            }
        }

        // Prepares our received data to be read
        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                // A new array for storing data from the server
                byte[] _data = socket.EndReceive(_result, ref endPoint);

                // Continue receiving data
                socket.BeginReceive(ReceiveCallback, null);

                // Disconnect the client if the data received is less than 4 bytes
                // NOTE: Might not have to disconnect, as it could be a common occurence that data is less than 4 bytes
                if (_data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }

                // Pass our handlers any data that needs to be read
                HandleData(_data);
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error receiving UDP data: {_ex}");
                Disconnect();
            }
        }

        // Handle the data packets received from the server
        private void HandleData(byte[] _data)
        {
            using (Packet _packet = new Packet(_data))
            {
                // Read out the current packet's length
                int _packetLength = _packet.ReadInt();

                // Read out the specified amount of bytes from the packet's length into the data variable
                _data = _packet.ReadBytes(_packetLength);
            }

            // Since our code won't be run on the same thread, execute it on the main thread
            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_data))
                {
                    int _packetId = _packet.ReadInt();

                    // Pass our handler a packet
                    packetHandlers[_packetId](_packet);
                }
            });
        }

        // Close out our connection with the server through UDP
        public void Disconnect()
        {
            instance.Disconnect();

            endPoint = null;
            socket = null;
        }
    }

    // Initialize our packet handlers
    private void InitializeClientData()
    {
        // These packets are for receiving
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome, ClientHandle.Welcome },
            { (int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
            { (int)ServerPackets.playerInput, ClientHandle.PlayerInput },
            { (int)ServerPackets.playerRotation, ClientHandle.PlayerRotation },
            { (int)ServerPackets.playerPosition, ClientHandle.PlayerPosition },
            { (int)ServerPackets.playerState, ClientHandle.PlayerState },
            { (int)ServerPackets.destroyPlayer, ClientHandle.DestroyPlayer },
        };
        Debug.Log("Initialized packets.");
    }

    // Closes our TCP and UDP connections to the server
    private void Disconnect()
    {
        if (isConnected)
        {
            Debug.Log("Disconnected from server.");

            isConnected = false;

            tcp.socket.Close();
            udp.socket.Close();
        }
    }
}
