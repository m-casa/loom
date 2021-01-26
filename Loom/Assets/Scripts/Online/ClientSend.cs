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
    #endregion
}
