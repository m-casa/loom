using UnityEngine;
using UnityEngine.UI;
using ECM.Controllers;
using ECM.Components;
using SensorToolkit;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject startMenu, meetingMenu;
    public InputField usernameField, ipAddressField;
    public Dropdown colorField;
    public Button[] votingOption;
    public Button skip;
    public Text gameTimerText, meetingTimerText, gameMessage;
    public bool activeMeeting, revealingVotes;
    private float meetingTimer;
    private int ejectedId;
    private bool playerEjected;

    // Make sure there is only once instance of this client
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            meetingTimer = 120;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }

    // Update is called once per frame
    public void Update()
    {
        // If the meeting came to an end, reveal the votes
        if (meetingTimer <= 0 && !revealingVotes)
        {
            revealingVotes = true;
            RevealVotes();
        }
    }

    // Will connect the client to the game server
    public void ConnectToServer()
    {
        if (usernameField.text != "" && usernameField.text.Length <= 13)
        {
            if (ipAddressField.text != "")
            {
                Client.instance.ip = ipAddressField.text;
            }

            usernameField.gameObject.SetActive(false);
            ipAddressField.gameObject.SetActive(false);
            colorField.gameObject.SetActive(true);

            Client.instance.CreateNewInstances();
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
        gameMessage.text = _serverMessage;
        gameMessage.CrossFadeAlpha(1f, 0f, false);
        gameMessage.CrossFadeAlpha(0f, 10f, false);
    }

    // Start the meeting
    public void StartMeeting(int _beginType)
    {
        if (_beginType == 1)
        {
            AudioManager.instance.PlaySound("Panic");
        }
        else
        {
            AudioManager.instance.PlaySound("Report");
        }

        PlayerManager localPlayer = GameManager.players[Client.instance.myId];
        PlayerManager onlinePlayer;
        CardInfo cardInfo;
        int playerCountOffset = 0;

        // Make sure the map is closed
        localPlayer.GetComponent<Role>().CloseMap();

        // Despawn any left over dead bodies
        GameManager.instance.DespawnBodies();

        // If this is a new player, make them a crewmate
        if (localPlayer.tag.Equals("Untagged"))
        {
            localPlayer.GetComponent<Role>().UpdateRole(false);
        }

        // If the local player is an imposter, hide the sabotage cooldown
        if (localPlayer.GetComponent<Role>().isImposter)
        {
            localPlayer.GetComponent<Role>().sabotageTimerText.enabled = false;
        }

        // Disable the local player's movement, sensor, emergency cooldown and allow them to interact with the voting system
        localPlayer.GetComponent<LocalFirstPersonController>().moveDirection = Vector3.zero;
        localPlayer.GetComponent<LocalFirstPersonController>().animator.SetFloat("horizontal", 0);
        localPlayer.GetComponent<LocalFirstPersonController>().animator.SetFloat("vertical", 0);
        localPlayer.GetComponent<RangeSensor>().enabled = false;
        localPlayer.GetComponent<Role>().canInteract = false;
        localPlayer.GetComponent<Role>().numOfInteractables = 0;
        localPlayer.GetComponent<Role>().emergencyTimerText.enabled = false;
        localPlayer.GetComponent<MouseLook>().SetCursorLock(false);

        // Set up the number of voting cards equal to the number of players
        for (int i = 0; i < GameManager.players.Count + playerCountOffset; i++)
        {
            // Make sure this player is in the game, if not update the player count offset
            if (GameManager.players.ContainsKey(i + 1))
            {
                onlinePlayer = GameManager.players[i + 1];
                cardInfo = votingOption[i].GetComponent<CardInfo>();
                cardInfo.id = onlinePlayer.id;

                // Disable this player's movement if they are not the local player
                if (onlinePlayer != localPlayer)
                {
                    onlinePlayer.GetComponent<OnlineFirstPersonController>().moveDirection = Vector3.zero;
                    onlinePlayer.GetComponent<OnlineFirstPersonController>().animator.SetFloat("horizontal", 0);
                    onlinePlayer.GetComponent<OnlineFirstPersonController>().animator.SetFloat("vertical", 0);
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
            else
            {
                // Is used to help offset the client IDs
                //  EX: If client 4 isn't in the game but 5 is, we need to skip 4 and loop one more time
                playerCountOffset++;
            }
        }

        // Reset the boolean which reveals votes
        revealingVotes = false;

        // Enable the meeting menu after setting up each player's voting card
        meetingMenu.SetActive(true);

        // Start the meeting
        activeMeeting = true;
    }

    // Update the remaining time in the meeting
    public void UpdateMeetingTime(float _meetingTimer)
    {
        // Display the time sent by the server, else keep it at 0
        if (_meetingTimer > 0)
        {
            meetingTimer = _meetingTimer;
            meetingTimerText.text = "Voting ends in: " + meetingTimer.ToString("0") + "s";
        }
        else
        {
            meetingTimer = 0;
            meetingTimerText.text = "Voting has ended!";
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

            // Check if the current voting card was chosen
            if (_choice == votingOption[i])
            {
                ColorBlock newColor = votingOption[i].colors;
                newColor.normalColor = new Color(0f, 255f, 0f);
                newColor.disabledColor = new Color(0f, 255f, 0f);

                votingOption[i].colors = newColor;

                // Send in the player's vote to the server
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

            // Send in the player's vote to the server
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
            // Get the info on this voting card
            cardInfo = skip.GetComponent<CardInfo>();

            // Add a vote to this card
            cardInfo.numOfVotes++;

            // Look through all the positions we can place a vote on for skip
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

        // Look through all the voting cards
        for (int v = 0; v < votingOption.Length; v++)
        {
            // Get the info on this voting card
            cardInfo = votingOption[v].GetComponent<CardInfo>();

            // Check if this is the player that voted
            if (_fromClient == cardInfo.id)
            {
                cardInfo.checkmark.gameObject.SetActive(true);
            }

            // Check if this is the player that was voted for
            if (_playerId == cardInfo.id)
            {
                // Add a vote to this card
                cardInfo.numOfVotes++;

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
        activeMeeting = false;

        PlayerManager localPlayer = GameManager.players[Client.instance.myId];
        CardInfo cardInfo;
        ColorBlock newColor = skip.colors;
        newColor.normalColor = new Color(255f, 255f, 255f);
        newColor.disabledColor = new Color(255f, 255f, 255f);

        // If the local player is an imposter, reset their cooldown
        if (localPlayer.GetComponent<Role>().isImposter)
        {
            localPlayer.GetComponent<Role>().currentCooldown = localPlayer.GetComponent<Role>().killCooldown;
            localPlayer.GetComponent<Role>().canKill = false;

            // Reset sabotages so they can't be used right after the meeting
            GameManager.instance.DeactivateSabotages();
            localPlayer.GetComponent<Role>().sabotageTimerText.enabled = true;
        }

        // Enable the local player's mouse movement, sensor and emergency timer
        localPlayer.GetComponent<MouseLook>().SetCursorLock(true);
        localPlayer.GetComponent<RangeSensor>().enabled = true;
        localPlayer.GetComponent<Role>().emergencyTimerText.enabled = true;

        // Go through every voting card and reset it for the next meeting
        for (int v = 0; v < votingOption.Length; v++)
        {
            // Get the info on this voting card
            cardInfo = votingOption[v].GetComponent<CardInfo>();

            // Look at all the positions votes can be placed
            for (int i = 0; i < cardInfo.voterIcon.Length; i++)
            {
                // Check if a vote was placed on this position, if so hide the icon
                if (cardInfo.voterIcon[i].gameObject.activeSelf)
                {
                    cardInfo.voterIcon[i].gameObject.SetActive(false);
                }
            }

            // Hide the voting card and reset its settings
            cardInfo.checkmark.gameObject.SetActive(false);
            cardInfo.revealer.SetActive(false);
            votingOption[v].gameObject.SetActive(false);
            votingOption[v].colors = newColor;
            cardInfo.numOfVotes = 0;
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

        // Reset the settings on the skip card
        skip.GetComponent<CardInfo>().revealer.SetActive(false);
        skip.colors = newColor;
        skip.GetComponent<CardInfo>().numOfVotes = 0;

        // Reset the emergency button timer and the meeting timer
        localPlayer.GetComponent<Role>().emergencyTimer = 25;
        meetingTimer = 120;

        // Disable the meeting menu
        meetingMenu.SetActive(false);

        // Check if a player was ejected, if so, let the server check if they were an imposter
        if (playerEjected)
        {
            ClientSend.ConfirmEject(ejectedId);
        }
    }

    // Reveal who each player voted for
    private void RevealVotes()
    {
        CardInfo currentCard;
        CardInfo mostVoted = skip.GetComponent<CardInfo>();
        CardInfo previouslyMostVoted = mostVoted;

        // Go through every voting card and reveal the votes
        for (int i = 0; i < votingOption.Length; i++)
        {
            // Get the info on this voting card
            currentCard = votingOption[i].GetComponent<CardInfo>();

            // Reveal the votes on this voting card
            currentCard.revealer.SetActive(true);

            // Check if the current card's number of votes is greater than the most voted card
            // Else check if there is a tie, which would skip the vote
            if (currentCard.numOfVotes > mostVoted.numOfVotes)
            {
                // Since the previously most voted card could have been a tie,
                //  check if the current card is still the greatest
                if (currentCard.numOfVotes > previouslyMostVoted.numOfVotes)
                {
                    mostVoted = currentCard;
                }
            }
            else if (currentCard.numOfVotes == mostVoted.numOfVotes)
            {
                skip.GetComponent<CardInfo>().numOfVotes = currentCard.numOfVotes;
                mostVoted = skip.GetComponent<CardInfo>();
                previouslyMostVoted = currentCard;
            }
        }

        // Reveal the votes on the skip card
        skip.GetComponent<CardInfo>().revealer.SetActive(true);

        // If a player was ejected, then they must be killed (but first check if they're still in the game)
        if (mostVoted != skip.GetComponent<CardInfo>() && GameManager.players.ContainsKey(mostVoted.id))
        {
            ejectedId = mostVoted.id;

            // Kill the player who was ejected
            GameManager.players[ejectedId].GetComponent<Life>().Die();
            
            playerEjected = true;
            Announce(GameManager.players[ejectedId].username + " was ejected!");
        }
        else
        {
            playerEjected = false;
            Announce("No one was ejected!");
        }
    }
}
