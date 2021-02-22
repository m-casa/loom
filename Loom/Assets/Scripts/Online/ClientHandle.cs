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
        GameManager.players[_id].GetComponent<PlayerManager>().localPlayer = GameManager.players[Client.instance.myId].gameObject;
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

        // Check if the local player or another player's is getting a role update
        if (_id == Client.instance.myId)
        {
            GameManager.players[_id].GetComponent<Role>().UpdateRole(_isImposter);
        }
        else
        {
            // If another player is the imposter, tag them so they can't be team killed
            if (_isImposter)
            {
                GameManager.players[_id].tag = "Imposter";

                // If the local player became an imposter before this player, then
                //  change this player's nameplate to red so the local player knows who their teammate is
                if (GameManager.players[Client.instance.myId].GetComponent<Role>().isImposter)
                {
                    GameManager.players[_id].GetComponent<PlayerManager>().nameInidcator.color = Color.red;
                }
            }
            else
            {
                GameManager.players[_id].tag = "Crewmate";
            }
        }
    }

    // Reads a packet from the server letting us know to attend the meeting
    public static void Meeting(Packet _packet)
    {
        int _beginType = _packet.ReadInt();

        UIManager.instance.StartMeeting(_beginType);
    }

    // Reads a packet from the server letting us know the remining time of the meeting
    public static void RemainingTime(Packet _packet)
    {
        float _meetingTimer = _packet.ReadFloat();

        UIManager.instance.UpdateMeetingTime(_meetingTimer);
    }

    // Reads a packet from the server letting us know which player was voted for
    public static void PlayerVote(Packet _packet)
    {
        int _fromClient = _packet.ReadInt();
        int _playerId = _packet.ReadInt();

        UIManager.instance.UpdateVotingCards(_fromClient, _playerId);
    }

    // Reads a packet from the server letting us know to resume the current round
    public static void ResumeRound(Packet _packet)
    {
        string _msg = _packet.ReadString();

        UIManager.instance.EndMeeting();
    }

    // Reads a packet from the server letting us know which player died
    public static void KillPlayer(Packet _packet)
    {
        int _targetId = _packet.ReadInt();

        GameManager.players[_targetId].GetComponent<Life>().Die();

        if (_targetId == Client.instance.myId)
        {
            AudioManager.instance.PlaySound("Killed");
        }
    }

    // Reads a packet from the server letting us know that a body was reported
    public static void ReportBody(Packet _packet)
    {
        int _reporter = _packet.ReadInt();

        UIManager.instance.Announce(GameManager.players[_reporter].username + " reported a body!");

        GameManager.instance.DespawnBodies();
    }

    // Reads a packet from the server with the updated task bar value
    public static void TaskUpdate(Packet _packet)
    {
        float _updatedValue = _packet.ReadFloat();

        // Check if this player is spawned in first
        if (GameManager.players.TryGetValue(Client.instance.myId, out PlayerManager _player))
        {
            _player.GetComponent<Role>().taskBar.value = _updatedValue;
        }
    }

    // Reads a packet from the server to close a specific door
    public static void CloseDoor(Packet _packet)
    {
        GameObject[] doors = GameManager.instance.doors;
        int doorId = _packet.ReadInt();

        doors[doorId - 1].SetActive(true);
        doors[doorId - 1].GetComponent<DoorInfo>().openSound.Play();
        doors[doorId - 1].GetComponent<Task>().finished = false;

        GameManager.instance.DeactivateDoorButton(doorId);
    }

    // Reads a packet from the server to open a specific door
    public static void OpenDoor(Packet _packet)
    {
        GameObject[] doors = GameManager.instance.doors;
        int doorId = _packet.ReadInt();

        doors[doorId - 1].GetComponent<DoorInfo>().closeSound.Play();
        doors[doorId - 1].SetActive(false);
        doors[doorId - 1].GetComponent<Task>().finished = true;

        GameManager.instance.StartDoorCooldown(doorId);
    }

    // Reads a packet from the server to turn off the lights
    public static void TurnOffLights(Packet _packet)
    {
        string _msg = _packet.ReadString();

        GameManager.instance.DeactivateSabotages();
        GameManager.instance.TurnOffLights();
    }

    // Reads a packet from the server to turn on the lights
    public static void TurnOnLights(Packet _packet)
    {
        string _msg = _packet.ReadString();

        GameManager.instance.TurnOnLights();
    }

    // Reads a packet from the server to turn off the oxygen
    public static void TurnOffO2(Packet _packet)
    {
        string _msg = _packet.ReadString();

        GameManager.instance.DeactivateSabotages();
        GameManager.instance.TurnOffO2();
    }

    // Reads a packet from the server to turn on the lights
    public static void TurnOnO2(Packet _packet)
    {
        int _O2PadId = _packet.ReadInt();

        GameManager.instance.TurnOnO2(_O2PadId);
    }

    // Reads a packet from the server to meltdown the reactor
    public static void MeltdownReactor(Packet _packet)
    {
        string _msg = _packet.ReadString();

        GameManager.instance.DeactivateSabotages();
        GameManager.instance.MeltdownReactor();
    }

    // Reads a packet from the server to turn restore the reactor
    public static void RestoreReactor(Packet _packet)
    {
        string _msg = _packet.ReadString();

        GameManager.instance.RestoreReactor();
    }

    // Reads a packet from the server letting us know the remining game time
    public static void RemainingGameTime(Packet _packet)
    {
        float _remainingTime = _packet.ReadFloat();

        UIManager.instance.gameTimerText.text = "Premature death in: " + _remainingTime.ToString("0") + "s";
    }

    // Reads a packet from the server to turn on the lights
    public static void TimeToSabotage(Packet _packet)
    {
        float timeToSabotage = _packet.ReadFloat();

        // Check if this player is spawned in first
        if (GameManager.players.TryGetValue(Client.instance.myId, out PlayerManager _player))
        {
            // Update the time it will take until the imposters can sabotage again
            if (timeToSabotage > 0)
            {
                _player.GetComponent<Role>().sabotageTimerText.text = "Can sabotage in: " + timeToSabotage.ToString("0") + "s";
            }
            else
            {
                _player.GetComponent<Role>().sabotageTimerText.text = "Sabotage available!";

                // Reactivate the sabotage buttons
                GameManager.instance.sabotage.GetComponentInChildren<ElectricalSabotage>().button.interactable = true;
                GameManager.instance.sabotage.GetComponentInChildren<O2Sabotage>().button.interactable = true;
                GameManager.instance.sabotage.GetComponentInChildren<ReactorSabotage>().button.interactable = true;
            }
        }
    }

    // Reads a packet from the server letting us know which team won
    public static void Winners(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _winningTeam = _packet.ReadString();

        // Check if the local player or another player's is getting an end of round update
        if (_id == Client.instance.myId)
        {
            GameManager.players[_id].GetComponent<Role>().UpdateWinners(_winningTeam);
        }
        else
        {
            // Reset this player's nameplate to white in case they were an imposter
            GameManager.players[_id].GetComponent<PlayerManager>().nameInidcator.color = Color.white;

            // If another player was dead, respawn their body
            if (GameManager.players[_id].GetComponent<Life>().isDead)
            {
                GameManager.players[_id].GetComponent<Life>().Respawn();
            }

            // Reset this player's tag
            GameManager.players[_id].tag = "Untagged";
        }
    }

    // Reads a packet from the server letting us know which player to destroy
    public static void DestroyPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.instance.DestroyPlayer(_id);
    }
}
