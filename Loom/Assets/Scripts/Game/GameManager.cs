using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private float simulationTimer;

    // A new dictionary to keep track of our players and their ids
    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

    [HideInInspector]
    public PlayerManager localPlayerManager;

    // Make sure there is only once instance of this client
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            simulationTimer = 0;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
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
            localPlayerManager = _player.GetComponent<PlayerManager>();
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
        GameObject deadBody;

        foreach (PlayerManager player in players.Values)
        {
            deadBody = player.GetComponent<Life>().deadBody;

            if (deadBody != null)
            {
                Destroy(deadBody);
            }
        }
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
