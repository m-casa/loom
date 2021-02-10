using UnityEngine;

public class ClientSend : MonoBehaviour
{
    // Send data to the server using TCP
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();

        // Send packet to the server
        Client.instance.tcp.SendData(_packet);
    }

    // Send Data to the server using UDP
    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    // Send back a message to the server that we received the welcome packet
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            // Prepare packet with what we will send to the server
            _packet.Write(Client.instance.myId);
            _packet.Write(UIManager.instance.usernameField.text);

            SendTCPData(_packet);
        }
    }

    // Send a request to spawn the player in everyone's instance
    public static void SpawnRequest()
    {
        using (Packet _packet = new Packet((int)ClientPackets.spawnRequest))
        {
            _packet.Write(UIManager.instance.usernameField.text);
            _packet.Write(UIManager.instance.colorField.value);

            SendTCPData(_packet);
        }
    }

    // Sends a packet to the server containing the players state
    public static void PlayerState(Vector3 _moveDirection, Quaternion _rotation, int _tickNumber)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerState))
        {
            _packet.Write(_moveDirection);
            _packet.Write(_rotation);
            _packet.Write(_tickNumber);

            SendUDPData(_packet);
        }
    }

    // Send a request to the server to start the round
    public static void RoundRequest()
    {
        using (Packet _packet = new Packet((int)ClientPackets.roundRequest))
        {
            _packet.Write("Start the round.");

            SendTCPData(_packet);
        }
    }

    // Sends a packet to the server specifying which player was just killed
    public static void KillRequest(int _id)
    {
        using (Packet _packet = new Packet((int)ClientPackets.killRequest))
        {
            _packet.Write(_id);

            SendTCPData(_packet);
        }
    }

    // Send a request to the server to report a body
    public static void ReportRequest()
    {
        using (Packet _packet = new Packet((int)ClientPackets.reportRequest))
        {
            _packet.Write("Report a dead body.");

            SendTCPData(_packet);
        }
    }

    // Send in the player's vote to the server
    public static void PlayerVote(int _playerId)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerVote))
        {
            _packet.Write(_playerId);

            SendTCPData(_packet);
        }
    }

    // Send the ejected player's id to the server
    public static void ConfirmEject(int _ejectedId)
    {
        using (Packet _packet = new Packet((int)ClientPackets.confirmEject))
        {
            _packet.Write(_ejectedId);

            SendTCPData(_packet);
        }
    }
    #endregion
}
