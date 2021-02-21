using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FlatKit;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab, playerPrefab, swipeCard, map, 
        sabotage, fixElectrical;
    public GameObject[] fixWiring, divertPower, uploadData, 
        shortTask, longTask, doors, fixReactor, fixO2;
    public Emergency emergency;
    public FogSettings fogSettings;
    public VolumeProfile currentProfile, normalProfile, sabotageProfile;
    private ColorAdjustments currentColorAdjustments, normalColorAdjustments, sabotageColorAdjustments;
    private float simulationTimer;

    // Make sure there is only once instance of this client
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            simulationTimer = 0;
            currentProfile.TryGet(out currentColorAdjustments);
            normalProfile.TryGet(out normalColorAdjustments);
            sabotageProfile.TryGet(out sabotageColorAdjustments);
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }

    // FixedUpdate will be called at the same rate as the tick rate
    public void FixedUpdate()
    {
        // Record at what point in time the last frame finished rendering
        simulationTimer += Time.deltaTime;

        // Catch up with the game time.
        // Advance the physics simulation in portions of Time.fixedDeltaTime
        // Note that generally, we don't want to pass variable delta to Simulate as that leads to unstable results.
        while (simulationTimer >= Time.fixedDeltaTime)
        {
            simulationTimer -= Time.fixedDeltaTime;

            // Simulate movement for every character on the server at once
            Physics.Simulate(Time.fixedDeltaTime);
        }
    }

    // Spawns in a new player for the client
    public void SpawnPlayer(int _id, string _username, int _colorId, Vector3 _position, Quaternion _rotation)
    {
        GameObject _player;

        // Check whether to instantiate the local player or another player
        if (_id == Client.instance.myId)
        {
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
            _player.GetComponent<Role>().map = map;
            _player.GetComponent<Role>().emergency = emergency;
        }
        else
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
        }

        _player.GetComponent<PlayerManager>().id = _id;
        _player.GetComponent<PlayerManager>().username = _username;
        _player.GetComponent<PlayerManager>().colorId = _colorId;
        _player.GetComponent<PlayerManager>().ChangeName();
        _player.GetComponent<PlayerManager>().ChangeIcon();
        _player.GetComponent<PlayerManager>().ChangeColor();
        players.Add(_id, _player.GetComponent<PlayerManager>());
    }

    // Despawn any left over dead bodies
    public void DespawnBodies()
    {
        GameObject body;

        foreach (PlayerManager player in players.Values)
        {
            body = player.GetComponent<Life>().body;

            if (body != null)
            {
                Destroy(body);
            }
        }
    }

    // Pick out the crewmate's tasks at random
    public void AssignTasks()
    {
        int rngFixWiring1 = Random.Range(0, 5);
        int rngFixWiring2 = rngFixWiring1;

        int rngDivertPower1 = Random.Range(0, 10);
        int rngDivertPower2 = rngDivertPower1;

        int rngUploadData1 = Random.Range(0, 5);
        int rngUploadData2 = rngUploadData1;

        int rngShortTask1 = Random.Range(0, 7);
        int rngShortTask2 = Random.Range(0, 7);

        int rngLongTask1 = Random.Range(0, 7);
        int rngLongTask2 = Random.Range(0, 7);

        // Assign everyone the swipe card task
        swipeCard.GetComponent<Task>().finished = false;
        swipeCard.GetComponent<Task>().outlinable.enabled = true;

        // Pick two random fix wiring tasks
        fixWiring[rngFixWiring1].GetComponent<Task>().finished = false;
        fixWiring[rngFixWiring1].GetComponent<Task>().outlinable.enabled = true;
        while (rngFixWiring2 == rngFixWiring1)
        {
            rngFixWiring2 = Random.Range(0, 5);
        }
        fixWiring[rngFixWiring2].GetComponent<Task>().finished = false;
        fixWiring[rngFixWiring2].GetComponent<Task>().outlinable.enabled = true;

        // Pick two random divert power tasks
        divertPower[rngDivertPower1].GetComponent<Task>().finished = false;
        divertPower[rngDivertPower1].GetComponent<Task>().outlinable.enabled = true;
        while (rngDivertPower2 == rngDivertPower1)
        {
            rngDivertPower2 = Random.Range(0, 10);
        }
        divertPower[rngDivertPower2].GetComponent<Task>().finished = false;
        divertPower[rngDivertPower2].GetComponent<Task>().outlinable.enabled = true;

        // Pick two random upload data tasks
        uploadData[rngUploadData1].GetComponent<Task>().finished = false;
        uploadData[rngUploadData1].GetComponent<Task>().outlinable.enabled = true;
        while (rngUploadData2 == rngUploadData1)
        {
            rngUploadData2 = Random.Range(0, 5);
        }
        uploadData[rngUploadData2].GetComponent<Task>().finished = false;
        uploadData[rngUploadData2].GetComponent<Task>().outlinable.enabled = true;

        // Pick two random short tasks
        shortTask[rngShortTask1].GetComponent<Task>().finished = false;
        shortTask[rngShortTask1].GetComponent<Task>().outlinable.enabled = true;
        while (rngShortTask2 == rngShortTask1)
        {
            rngShortTask2 = Random.Range(0, 5);
        }
        shortTask[rngShortTask2].GetComponent<Task>().finished = false;
        shortTask[rngShortTask2].GetComponent<Task>().outlinable.enabled = true;

        // Pick two random long tasks
        longTask[rngLongTask1].GetComponent<Task>().finished = false;
        longTask[rngLongTask1].GetComponent<Task>().outlinable.enabled = true;
        while (rngLongTask2 == rngLongTask1)
        {
            rngLongTask2 = Random.Range(0, 5);
        }
        longTask[rngLongTask2].GetComponent<Task>().finished = false;
        longTask[rngLongTask2].GetComponent<Task>().outlinable.enabled = true;
    }

    // Reset all tasks in the game
    public void ResetTasks()
    {
        // Reset card swipe task
        swipeCard.GetComponent<Task>().resetTask = true;

        // Reset short tasks
        for (int i = 0; i < shortTask.Length; i++)
        {
           instance.shortTask[i].GetComponent<Task>().resetTask = true;
        }

        // Reset long tasks
        for (int i = 0; i < instance.longTask.Length; i++)
        {
            instance.longTask[i].GetComponent<Task>().resetTask = true;
        }

        // Reset fix wiring tasks
        for (int i = 0; i < instance.fixWiring.Length; i++)
        {
            instance.fixWiring[i].GetComponent<Task>().resetTask = true;
        }

        // Reset divert power tasks
        for (int i = 0; i < instance.divertPower.Length; i++)
        {
            instance.divertPower[i].GetComponent<Task>().resetTask = true;
        }

        // Reset upload data tasks
        for (int i = 0; i < instance.uploadData.Length; i++)
        {
            instance.uploadData[i].GetComponent<Task>().resetTask = true;
        }
    }

    // If the local player is an imposter, deactivate the sabotage buttons
    public void DeactivateSabotages()
    {
        if (players[Client.instance.myId].GetComponent<Role>().isImposter)
        {
            sabotage.GetComponentInChildren<ElectricalSabotage>().button.interactable = false;
            sabotage.GetComponentInChildren<O2Sabotage>().button.interactable = false;
            sabotage.GetComponentInChildren<ReactorSabotage>().button.interactable = false;
            players[Client.instance.myId].GetComponent<Role>().sabotageTimerText.text = "Sabotage unvailable!";
        }
    }

    // If the local player is an imposter, deactivate the door sabotage buttons
    public void DeactivateDoorSabotages()
    {
        foreach (GameObject door in doors)
        {
            DeactivateDoorButton(door.GetComponent<DoorInfo>().doorId);
            StartDoorCooldown(door.GetComponent<DoorInfo>().doorId);
        }
    }

    // Deactivate the sabotage button for the specified door
    public void DeactivateDoorButton(int _doorId)
    {
        foreach (DoorSabotage sabotageButton in sabotage.GetComponentsInChildren<DoorSabotage>())
        {
            foreach (GameObject door in sabotageButton.doors)
            {
                if (door.GetComponent<DoorInfo>().doorId == _doorId)
                {
                    sabotageButton.button.interactable = false;
                    return;
                }
            }
        }
    }

    // Start a cooldown on the sabotage button for the specified door
    public void StartDoorCooldown(int _doorId)
    {
        foreach (DoorSabotage sabotageButton in sabotage.GetComponentsInChildren<DoorSabotage>())
        {
            foreach (GameObject door in sabotageButton.doors)
            {
                if (door.GetComponent<DoorInfo>().doorId == _doorId)
                {
                    // Do not reset the door cooldown unless it is not active and not interactable
                    if (!sabotageButton.activeCooldown && sabotageButton.button.interactable == false)
                    {
                        sabotageButton.currentCooldown = sabotageButton.doorCooldown;
                        sabotageButton.activeCooldown = true;
                    }
                    
                    return;
                }
            }
        }
    }

    // Open any doors that were left closed when the round ended
    public void ResetDoors()
    {
        // Shut every door
        foreach (GameObject door in doors)
        {
            door.SetActive(false);
        }

        // Reset every door sabotage button
        foreach (DoorSabotage sabotageButton in sabotage.GetComponentsInChildren<DoorSabotage>())
        {
            sabotageButton.button.interactable = true;
        }
    }

    // If the local player isn't an imposter, turn off the lights
    // Also, if the local player isn't dead, they should be able to fix the lights
    public void TurnOffLights()
    {
        if (!players[Client.instance.myId].GetComponent<Role>().isImposter)
        {
            fogSettings.useDistance = true;
            fogSettings.useHeight = true;
        }

        if (!players[Client.instance.myId].GetComponent<Life>().isDead)
        {
            fixElectrical.GetComponent<Task>().finished = false;
            fixElectrical.GetComponent<Task>().outlinable.enabled = true;
        }
    }

    // Turn on the lights and do not allow others to interact with them any longer
    public void TurnOnLights()
    {
        fogSettings.useDistance = false;
        fogSettings.useHeight = false;
        fixElectrical.GetComponent<Task>().resetTask = true;
    }

    // If the local player isn't dead, they should be able to fix the oxygen
    public void TurnOffO2()
    {
        if (!players[Client.instance.myId].GetComponent<Life>().isDead)
        {
            fixO2[0].GetComponent<Task>().finished = false;
            fixO2[0].GetComponent<Task>().outlinable.enabled = true;

            fixO2[1].GetComponent<Task>().finished = false;
            fixO2[1].GetComponent<Task>().outlinable.enabled = true;
        }

        UIManager.instance.gameTimerText.gameObject.SetActive(true);
        currentColorAdjustments.colorFilter.Override(sabotageColorAdjustments.colorFilter.value);
    }

    // Turn on the oxygen and do not allow others to interact with them any longer
    public void TurnOnO2(int _O2PadId)
    {
        if (_O2PadId < 3)
        {
            fixO2[_O2PadId].GetComponent<Task>().resetTask = true;
        }
        else
        {
            fixO2[0].GetComponent<Task>().resetTask = true;
            fixO2[1].GetComponent<Task>().resetTask = true;

            UIManager.instance.gameTimerText.gameObject.SetActive(false);
            currentColorAdjustments.colorFilter.Override(normalColorAdjustments.colorFilter.value);
        }
    }

    // If the local player isn't dead, they should be able to restore the reactor
    public void MeltdownReactor()
    {
        if (!players[Client.instance.myId].GetComponent<Life>().isDead)
        {
            fixReactor[0].GetComponent<Task>().finished = false;
            fixReactor[0].GetComponent<Task>().outlinable.enabled = true;

            fixReactor[1].GetComponent<Task>().finished = false;
            fixReactor[1].GetComponent<Task>().outlinable.enabled = true;
        }

        UIManager.instance.gameTimerText.gameObject.SetActive(true);
        currentColorAdjustments.colorFilter.Override(sabotageColorAdjustments.colorFilter.value);
    }

    // Restore the reactor and do not allow others to interact with it any longer
    public void RestoreReactor()
    {
        fixReactor[0].GetComponent<Task>().resetTask = true;
        fixReactor[1].GetComponent<Task>().resetTask = true;

        UIManager.instance.gameTimerText.gameObject.SetActive(false);
        currentColorAdjustments.colorFilter.Override(normalColorAdjustments.colorFilter.value);
    }

    // Destroys a specified player for the client
    public void DestroyPlayer(int _id)
    {
        Button votingOption;

        // Look for and remove this player's voting card
        for (int i = 0; i < players.Count; i++)
        {
            votingOption = UIManager.instance.votingOption[i];

            if (votingOption.GetComponent<CardInfo>().id == _id)
            {
                votingOption.gameObject.SetActive(false);
                break;
            }
        }

        Destroy(players[_id].gameObject);
        players.Remove(_id);
    }
}
