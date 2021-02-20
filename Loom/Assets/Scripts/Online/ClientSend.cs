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

    // Send a request to the server to start a meeting
    public static void MeetingRequest()
    {
        using (Packet _packet = new Packet((int)ClientPackets.meetingRequest))
        {
            _packet.Write("Start a meeting.");

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

    // Send a message to the server letting it know a task was completed
    public static void CompletedTask()
    {
        using (Packet _packet = new Packet((int)ClientPackets.completedTask))
        {
            _packet.Write("Completed a task!");

            SendTCPData(_packet);
        }
    }

    // Send a request to the server letting it know to sabotage doors
    public static void SabotageDoors(GameObject[] _doors)
    {
        using (Packet _packet = new Packet((int)ClientPackets.sabotageDoors))
        {
            _packet.Write(_doors.Length);
            foreach (GameObject door in _doors)
            {
                _packet.Write(door.GetComponent<DoorInfo>().doorId);
            }

            SendTCPData(_packet);
        }
    }

    // Send a request to the server letting it know to open a door
    public static void OpenDoor(int _doorId)
    {
        using (Packet _packet = new Packet((int)ClientPackets.openDoor))
        {
            _packet.Write(_doorId);

            SendTCPData(_packet);
        }
    }

    // Send a request to the server letting it know to sabotage lights
    public static void SabotageElectrical()
    {
        using (Packet _packet = new Packet((int)ClientPackets.sabotageElectrical))
        {
            _packet.Write("Sabotage the lights!");

            SendTCPData(_packet);
        }
    }

    // Send a request to the server letting it know to fix the lights
    public static void FixElectrical()
    {
        using (Packet _packet = new Packet((int)ClientPackets.fixElectrical))
        {
            _packet.Write("Fix the lights!");

            SendTCPData(_packet);
        }
    }

    // Send a request to the server letting it know to sabotage O2
    public static void SabotageO2()
    {
        using (Packet _packet = new Packet((int)ClientPackets.sabotageO2))
        {
            _packet.Write("Sabotage the oxygen!");

            SendTCPData(_packet);
        }
    }

    // Send a request to the server letting it know to turn on oxygen
    public static void FixO2()
    {
        using (Packet _packet = new Packet((int)ClientPackets.fixO2))
        {
            _packet.Write("Fix the oxygen!");

            SendTCPData(_packet);
        }
    }

    // Send a request to the server letting it know to sabotage reactor
    public static void SabotageReactor()
    {
        using (Packet _packet = new Packet((int)ClientPackets.sabotageReactor))
        {
            _packet.Write("Sabotage the reactor!");

            SendTCPData(_packet);
        }
    }

    // Send a request to the server letting it know to restore reactor
    public static void FixReactor()
    {
        using (Packet _packet = new Packet((int)ClientPackets.fixReactor))
        {
            _packet.Write("Restore the reactor!");

            SendTCPData(_packet);
        }
    }

    #endregion
}
