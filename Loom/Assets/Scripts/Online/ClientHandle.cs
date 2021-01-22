using UnityEngine;
using System.Net;
using ECM.Controllers;

public class ClientHandle : MonoBehaviour
{
    // Reads a welcome packet from the server
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    // Reads a packet from the server with player spawn position information
    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
    }

    // Reads a packet from the server with player position information
    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        GameManager.players[_id].transform.position = _position;
    }

    // Reads a packet from the server with player rotation information
    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.players[_id].transform.rotation = _rotation;
    }

    // Reads a packet from the server with player rotation information
    public static void PlayerInput(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _moveDirection = _packet.ReadVector3();

        GameManager.players[_id].GetComponent<OnlineFirstPersonController>().moveDirection = _moveDirection;
    }
}
