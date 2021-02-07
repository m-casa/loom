using UnityEngine;
using UnityEngine.UI;
using ECM.Controllers;
using ECM.Components;
using SensorToolkit;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject startMenu, meetingMenu;
    public InputField usernameField;
    public Dropdown colorField;
    public Button[] option;

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

    // Will connect the client to the game server
    public void ConnectToServer()
    {
        if (usernameField.text != "" && usernameField.text.Length <= 20)
        {
            usernameField.gameObject.SetActive(false);
            colorField.gameObject.SetActive(true);
            Client.instance.ConnectToServer();
        }
    }

    // Will request the server to spawn in the client
    public void SendRequest()
    {
        ClientSend.SpawnRequest();
    }

    // Start the meeting
    public void StartMeeting()
    {
        Image[] images;
        PlayerManager localPlayer = GameManager.players[Client.instance.myId];
        PlayerManager onlinePlayer;

        // Disable the local player's movement/input, and allow them to interact with the voting system
        localPlayer.GetComponent<LocalFirstPersonController>().enabled = false;
        localPlayer.GetComponent<MouseLook>().SetCursorLock(false);
        localPlayer.GetComponent<RangeSensor>().enabled = false;
        localPlayer.GetComponent<Role>().canInteract = false;

        // Show the number of vote options equal to the number of players
        for (int i = 0; i < GameManager.players.Count; i++)
        {
            images = option[i].GetComponentsInChildren<Image>();
            onlinePlayer = GameManager.players[i + 1];

            // Disable this player's movement if they are not the local player
            if (onlinePlayer != localPlayer)
            {
                onlinePlayer.GetComponent<OnlineFirstPersonController>().enabled = false;
            }

            // If the local player is dead, do not allow them to vote for anyone
            if (localPlayer.GetComponent<Life>().isDead)
            {
                option[i].interactable = false;
            }

            // If this player is dead, darken the card and do not allow the local player to vote on it
            if (onlinePlayer.GetComponent<Life>().isDead)
            {
                ColorBlock newColor = option[i].colors;
                newColor.disabledColor = new Color(255f, 0f, 0f, 0.25f);

                option[i].interactable = false;
                option[i].colors = newColor;
            }

            // Change the second image's sprite in the array (since we do not want to modify the parent's sprite)
            //  to match this player's icon
            images[1].sprite = onlinePlayer.GetComponent<PlayerManager>().spriteRenderer.sprite;

            // Change the text on this voting card to match this player's username
            option[i].GetComponentInChildren<Text>().text = onlinePlayer.GetComponent<PlayerManager>().username;

            // Enable this player's voting card
            option[i].gameObject.SetActive(true);
        }

        // Enable the meeting menu after setting up each player's voting card
        meetingMenu.SetActive(true);
    }
}
