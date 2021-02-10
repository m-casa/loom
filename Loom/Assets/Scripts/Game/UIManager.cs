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
    public Button[] votingOption;
    public Button skip;
    public Text meetingTimerText, serverMessage;
    private bool activeMeeting;
    private float meetingTimer;

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

    // Update is called once per frame
    public void Update()
    {
        if (activeMeeting && meetingTimer <= 0)
        {
            RevealVotes();
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
    public void SendSpawnRequest()
    {
        ClientSend.SpawnRequest();
    }

    // Announce to the player a message from the server
    public void Announce(string _serverMessage)
    {
        serverMessage.text = _serverMessage;
        serverMessage.CrossFadeAlpha(1f, 0f, true);
        serverMessage.CrossFadeAlpha(0f, 5f, false);
    }

    // Start the meeting
    public void StartMeeting()
    {
        PlayerManager localPlayer = GameManager.players[Client.instance.myId];
        PlayerManager onlinePlayer;
        CardInfo cardInfo;

        // Disable the local player's movement/input, and allow them to interact with the voting system
        localPlayer.GetComponent<LocalFirstPersonController>().enabled = false;
        localPlayer.GetComponent<RangeSensor>().enabled = false;
        localPlayer.GetComponent<Role>().canInteract = false;
        localPlayer.GetComponent<Role>().numOfInteractables = 0;
        localPlayer.GetComponent<MouseLook>().SetCursorLock(false);

        // Set up the number of voting cards equal to the number of players
        for (int i = 0; i < GameManager.players.Count; i++)
        {
            onlinePlayer = GameManager.players[i + 1];
            cardInfo = votingOption[i].GetComponent<CardInfo>();
            cardInfo.id = onlinePlayer.id;

            // Disable this player's movement if they are not the local player
            if (onlinePlayer != localPlayer)
            {
                onlinePlayer.GetComponent<OnlineFirstPersonController>().enabled = false;
            }

            // If the local player is dead, do not allow them to vote, else allow voting
            if (localPlayer.GetComponent<Life>().isDead)
            {
                votingOption[i].interactable = false;
                skip.interactable = false;
            }
            else
            {
                votingOption[i].interactable = true;
                skip.interactable = true;
            }

            // If this player is dead, darken the card and do not allow the local player to vote on it
            if (onlinePlayer.GetComponent<Life>().isDead)
            {
                ColorBlock newColor = votingOption[i].colors;
                newColor.disabledColor = new Color(255f, 0f, 0f, 0.25f);

                votingOption[i].colors = newColor;
                votingOption[i].interactable = false;
            }

            // Change the icon on this voting card to match this player's icon
            cardInfo.icon.sprite = onlinePlayer.GetComponent<PlayerManager>().spriteRenderer.sprite;

            // Change the text on this voting card to match this player's username
            cardInfo.username.text = onlinePlayer.GetComponent<PlayerManager>().username;

            // Enable this player's voting card
            votingOption[i].gameObject.SetActive(true);
        }

        // Enable the meeting menu after setting up each player's voting card
        meetingMenu.SetActive(true);

        // Start the meeting
        activeMeeting = true;
    }

    // Update the remaining time in the meeting
    public void UpdateMeetingTime(float _meetingTimer)
    {
        if (_meetingTimer > 0)
        {
            meetingTimer = _meetingTimer;
            meetingTimerText.text = meetingTimer.ToString("0") + "s";
        }
        else
        {
            meetingTimer = 0;
            meetingTimerText.text = meetingTimer.ToString("0") + "s";
        }
    }

    // Confirm the local player's vote
    public void ConfirmVote(Button _choice)
    {
        // Lock in the player's vote by making each voting option uninteractable
        skip.interactable = false;
        for (int i = 0; i < votingOption.Length; i++)
        {
            votingOption[i].interactable = false;

            // Check which player's voting card was chosen
            if (_choice == votingOption[i])
            {
                ColorBlock newColor = votingOption[i].colors;
                newColor.normalColor = new Color(0f, 255f, 0f);
                newColor.disabledColor = new Color(0f, 255f, 0f);

                votingOption[i].colors = newColor;
                ClientSend.PlayerVote(votingOption[i].GetComponent<CardInfo>().id);
            }
        }

        // Check if the voting choice was to skip
        if (_choice == skip)
        {
            ColorBlock newColor = skip.colors;
            newColor.normalColor = new Color(0f, 255f, 0f);
            newColor.disabledColor = new Color(0f, 255f, 0f);

            skip.colors = newColor;
            ClientSend.PlayerVote(11);
        }
    }

    // Update the current voting cards to display who voted and for whom they voted for
    public void UpdateVotingCards(int _fromClient, int _playerId)
    {
        CardInfo cardInfo;

        // Check if skip was voted for, else look through the voting cards to see who was chosen below
        if (_playerId == 11)
        {
            // Look through all the positions we can place a vote on for skip
            for (int i = 0; i < skip.GetComponent<CardInfo>().voterIcon.Length; i++)
            {
                // Check if a vote has already been placed on this position, if not place a vote
                if (!skip.GetComponent<CardInfo>().voterIcon[i].gameObject.activeSelf)
                {
                    skip.GetComponent<CardInfo>().voterIcon[i].sprite = GameManager.players[_fromClient].GetComponent<PlayerManager>().spriteRenderer.sprite;
                    skip.GetComponent<CardInfo>().voterIcon[i].gameObject.SetActive(true);
                    break;
                }
            }
        }

        // Look through all the voting cards
        for (int v = 0; v < votingOption.Length; v++)
        {
            // Get the info on this voting card
            cardInfo = votingOption[v].GetComponent<CardInfo>();

            // Check if this is the player that voted
            if (cardInfo.id == _fromClient)
            {
                cardInfo.checkmark.gameObject.SetActive(true);
            }

            if (_playerId == cardInfo.id)
            {
                // Look through all the positions we can place a vote on for this voting card
                for (int i = 0; i < cardInfo.voterIcon.Length; i++)
                {
                    // Check if a vote has already been placed on this position, if not place a vote
                    if (!cardInfo.voterIcon[i].gameObject.activeSelf)
                    {
                        cardInfo.voterIcon[i].sprite = GameManager.players[_fromClient].GetComponent<PlayerManager>().spriteRenderer.sprite;
                        cardInfo.voterIcon[i].gameObject.SetActive(true);
                        break;
                    }
                }
            }
        }
    }

    // Ends the meeting and resumes the round
    public void EndMeeting()
    {
        PlayerManager localPlayer = GameManager.players[Client.instance.myId];
        CardInfo cardInfo;
        ColorBlock newColor = skip.colors;
        newColor.normalColor = new Color(255f, 255f, 255f);
        newColor.disabledColor = new Color(255f, 255f, 255f);

        // Enable the local player's movement/input
        localPlayer.GetComponent<LocalFirstPersonController>().enabled = true;
        localPlayer.GetComponent<MouseLook>().SetCursorLock(true);
        localPlayer.GetComponent<RangeSensor>().enabled = true;

        // Enable movement for the other players
        foreach (PlayerManager onlinePlayer in GameManager.players.Values)
        {
            if (onlinePlayer != localPlayer)
            {
                onlinePlayer.GetComponent<OnlineFirstPersonController>().enabled = true;
            }
        }

        // Go through every voting card and reset it for the next meeting
        for (int v = 0; v < votingOption.Length; v++)
        {
            cardInfo = votingOption[v].GetComponent<CardInfo>();

            // Look through all the positions we placed votes on and hide the icons
            for (int i = 0; i < cardInfo.voterIcon.Length; i++)
            {
                // Check if a vote was placed on this position, if so hide the icon
                if (cardInfo.voterIcon[i].gameObject.activeSelf)
                {
                    cardInfo.voterIcon[i].gameObject.SetActive(false);
                }
            }

            cardInfo.revealer.SetActive(false);
            votingOption[v].colors = newColor;
            votingOption[v].gameObject.SetActive(false);
        }

        //  Go through the skip card and reset it for the next meeting
        for (int i = 0; i < skip.GetComponent<CardInfo>().voterIcon.Length; i++)
        {
            // Check if a vote was placed on this position, if so hide the icon
            if (skip.GetComponent<CardInfo>().voterIcon[i].gameObject.activeSelf)
            {
                skip.GetComponent<CardInfo>().voterIcon[i].gameObject.SetActive(false);
            }
        }
        skip.GetComponent<CardInfo>().revealer.SetActive(false);
        skip.colors = newColor;

        // Disable the meeting menu
        meetingMenu.SetActive(false);

        // Reset the timer and end the meeting
        meetingTimer = 120;
        activeMeeting = false;
    }

    // Reveal who each player voted for
    private void RevealVotes()
    {
        CardInfo cardInfo;

        // Go through every voting card and reveal the votes
        for (int i = 0; i < votingOption.Length; i++)
        {
            // Get the info on this voting card
            cardInfo = votingOption[i].GetComponent<CardInfo>();

            cardInfo.revealer.SetActive(true);
        }
        skip.GetComponent<CardInfo>().revealer.SetActive(true);
    }
}
