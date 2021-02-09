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
    public Button skip;
    public Text meetingTimer;

    // Make sure there is only once instance of this client
    public void Awake()
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
        int cardId;

        // Disable the local player's movement/input, and allow them to interact with the voting system
        localPlayer.GetComponent<LocalFirstPersonController>().enabled = false;
        localPlayer.GetComponent<RangeSensor>().enabled = false;
        localPlayer.GetComponent<Role>().canInteract = false;
        localPlayer.GetComponent<Role>().numOfInteractables = 0;
        localPlayer.GetComponent<MouseLook>().SetCursorLock(false);

        // Show the number of voting cards equal to the number of players
        foreach (PlayerManager onlinePlayer in GameManager.players.Values)
        {
            cardId = onlinePlayer.id - 1;
            images = option[cardId].GetComponentsInChildren<Image>();

            // Disable this player's movement if they are not the local player
            if (onlinePlayer != localPlayer)
            {
                onlinePlayer.GetComponent<OnlineFirstPersonController>().enabled = false;
            }

            // If the local player is dead, do not allow them to vote, else allow voting
            if (localPlayer.GetComponent<Life>().isDead)
            {
                option[cardId].interactable = false;
                skip.interactable = false;
            }
            else
            {
                option[cardId].interactable = true;
                skip.interactable = true;
            }

            // If this player is dead, darken the card and do not allow the local player to vote on it
            if (onlinePlayer.GetComponent<Life>().isDead)
            {
                ColorBlock newColor = option[cardId].colors;
                newColor.disabledColor = new Color(255f, 0f, 0f, 0.25f);

                option[cardId].interactable = false;
                option[cardId].colors = newColor;
            }

            // Change the second image's sprite in the array (since we do not want to modify the parent's sprite)
            //  to match this player's icon
            images[1].sprite = onlinePlayer.GetComponent<PlayerManager>().spriteRenderer.sprite;

            // Change the text on this voting card to match this player's username
            option[cardId].GetComponentInChildren<Text>().text = onlinePlayer.GetComponent<PlayerManager>().username;

            // Enable this player's voting card
            option[cardId].gameObject.SetActive(true);
        }

        // Enable the meeting menu after setting up each player's voting card
        meetingMenu.SetActive(true);
    }

    // Update the remaining time in the meeting
    public void UpdateMeetingTime(float _meetingTimer)
    {
        meetingTimer.text = _meetingTimer.ToString("0") + "s";
    }

    // Confirm the local player's vote
    public void ConfirmVote(Button _choice)
    {
        // Lock in the player's vote
        for (int i = 0; i < option.Length; i++)
        {
            option[i].interactable = false;
        }
        skip.interactable = false;

        // Check if the voting choice was to skip
        if (_choice == skip)
        {
            ColorBlock newColor = skip.colors;
            newColor.normalColor = new Color(0f, 255f, 0f);
            newColor.disabledColor = new Color(0f, 255f, 0f);

            skip.colors = newColor;
            Debug.Log("Voted to skip");
            return;
        }

        int cardId;

        // Check which player's voting card was chosen
        foreach (PlayerManager onlinePlayer in GameManager.players.Values)
        {
            cardId = onlinePlayer.id - 1;

            if (_choice == option[cardId])
            {
                ColorBlock newColor = option[cardId].colors;
                newColor.normalColor = new Color(0f, 255f, 0f);
                newColor.disabledColor = new Color(0f, 255f, 0f);

                option[cardId].colors = newColor;
                Debug.Log("Voted for client " + onlinePlayer.id);
                return;
            }
        }
    }

    // Ends the meeting and resumes the round
    public void EndMeeting()
    {
        PlayerManager localPlayer = GameManager.players[Client.instance.myId];
        ColorBlock newColor = skip.colors;
        newColor.normalColor = new Color(255f, 255f, 255f);
        newColor.disabledColor = new Color(255f, 255f, 255f);

        // Enable the local player's movement/input
        localPlayer.GetComponent<LocalFirstPersonController>().enabled = true;
        localPlayer.GetComponent<RangeSensor>().enabled = true;
        localPlayer.GetComponent<MouseLook>().SetCursorLock(true);

        // Enable movement for the other players
        foreach (PlayerManager onlinePlayer in GameManager.players.Values)
        {
            if (onlinePlayer != localPlayer)
            {
                onlinePlayer.GetComponent<OnlineFirstPersonController>().enabled = true;
            }
        }

        // Hide all the voting options for the next meeting and reset the card colors
        for (int i = 0; i < option.Length; i++)
        {
            option[i].colors = newColor;
            option[i].gameObject.SetActive(false);
        }
        skip.colors = newColor;

        // Disable the meeting menu
        meetingMenu.SetActive(false);
    }
}
