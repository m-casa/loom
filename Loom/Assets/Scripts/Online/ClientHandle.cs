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

    // Reads a packet from the server with player spawn information
    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        int _color = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();

        UIManager.instance.startMenu.SetActive(false);
        GameManager.instance.SpawnPlayer(_id, _username, _color, _position, _rotation);
    }

    // Reads a packet from the server with player input information
    public static void PlayerInput(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _moveDirection = _packet.ReadVector3();

        // Check if this player is spawned in first
        if (GameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.GetComponent<OnlineFirstPersonController>().moveDirection = _moveDirection;
        }
    }

    // Reads a packet from the server with player rotation information
    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        // Check if this player is spawned in first
        if (GameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.transform.rotation = _rotation;
        }
    }

    // Reads a packet from the server with player position information
    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        // Check if this player is spawned in first
        if (GameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.transform.position = _position;
        }
    }

    // Reads a packet from the server with the server's state of the player
    public static void PlayerState(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        int _tickNumber = _packet.ReadInt();

        // Check if this player is spawned in first
        if (GameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            if (_id == Client.instance.myId)
            {
                _player.GetComponent<LocalFirstPersonController>().SyncPlayer(_position, _tickNumber);
            }
            else
            {
                _player.GetComponent<OnlineFirstPersonController>().SyncPlayer(_position);
            }
        }
    }

    // Reads a packet from the server letting us know the player's role
    public static void PlayerRole(Packet _packet)
    {
        int _id = _packet.ReadInt();
        bool _isImposter = _packet.ReadBool();

        if (_id == Client.instance.myId)
        {
            GameManager.players[_id].GetComponent<Role>().isImposter = _isImposter;
            GameManager.players[_id].GetComponent<Role>().UpdateRole();
        }
        else
        {
            if (_isImposter)
            {
                GameManager.players[_id].tag = "Imposter";
            }
            else
            {
                GameManager.players[_id].tag = "Crewmate";
            }
        }
    }

    // Reads a packet from the server letting us know which player to destroy
    public static void DestroyPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.instance.DestroyPlayer(_id);
    }
}
