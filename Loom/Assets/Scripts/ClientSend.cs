using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        // Send packet to the server
        Client.instance.tcp.SendData(_packet);
    }

    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        // Send packet to the server using udp
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    // Send back message that we received welcome packet
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

    // Send back message that we received UDP test packet
    public static void UDPTestReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.udpTestReceived))
        {
            // Prepare packet with what we will send to the server
            _packet.Write("Received a UDP packet.");

            SendUDPData(_packet);
        }
    }
    #endregion
}
