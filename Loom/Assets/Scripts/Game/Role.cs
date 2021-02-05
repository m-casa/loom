using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SensorToolkit;

public class Role : MonoBehaviour
{
    // Fields
    public Text roleIndicator, killIndicator;
    public Life life;
    private RangeSensor rangeSensor;
    public bool isImposter, canKill;
    private bool canInteract, isUsing, isHolding, isKilling, isReporting;
    private float killCooldown, currentCooldown;
    private int numOfInteractables;

    // Awake is called before Start
    public void Awake()
    {
        rangeSensor = GetComponent<RangeSensor>();
        canKill = false;
        canInteract = false;
        killCooldown = 20f;
        numOfInteractables = 0;
    }

    // Start is called before the first frame update
    public void Start()
    {
        currentCooldown = killCooldown;
    }

    // Update is called once per frame
    public void Update()
    {
        rangeSensor.Pulse();

        // Check if player is viewing an interactable object
        if (canInteract)
        {
            // Check for player input
            HandleInteractions();

            // Perform any interaction the player wants
            PerformInteractions();
        }

        if (isImposter)
        {
            UpdateKillCooldown();
        }
    }

    // Used to decide if there are interactable objects
    public void CheckForInteractables(int _numOfInteractables)
    {
        numOfInteractables += _numOfInteractables;

        if (numOfInteractables > 0)
        {
            canInteract = true;
        }
        else
        {
            canInteract = false;
        }
    }

    // Update the role of the player
    public void UpdateRole(bool _isImposter)
    {
        isImposter = _isImposter;

        // Updates the player's HUD to reflect what their role is
        if (isImposter)
        {
            roleIndicator.color = Color.red;
            roleIndicator.text = "Imposter";

            // If the local player became an imposter after the other player, then
            //  look for that player and change their nameplate to red so the local player knows who their teammate is
            foreach (PlayerManager player in GameManager.players.Values)
            {
                if (player.tag.Equals("Imposter"))
                {
                    player.nameInidcator.color = Color.red;
                }
            }

            currentCooldown = killCooldown;
            killIndicator.gameObject.SetActive(true);
        }
        else
        {
            roleIndicator.color = Color.white;
            roleIndicator.text = "Crewmate";
        }

        // If the role indicator is not on then turn it on
        if (!roleIndicator.gameObject.activeSelf)
        {
            roleIndicator.gameObject.SetActive(true);
        }
    }

    // Update the player's HUD to display the winning team
    public void UpdateWinners(string _winningTeam)
    {
        // Updates the player's HUD to reflect which team won the last round
        if (_winningTeam.Equals("Crewmates"))
        {
            roleIndicator.color = Color.green;
            roleIndicator.text = "Winners: " + _winningTeam;
        }
        else
        {
            roleIndicator.color = Color.red;
            roleIndicator.text = "Winners: " + _winningTeam;
        }

        // If the player was an imposter, revert their role
        if (isImposter)
        {
            isImposter = false;
            killIndicator.gameObject.SetActive(false);
        }

        // If the player was dead, respawn their body
        if (life.isDead)
        {
            life.Respawn();
        }
    }

    // If the player is an imposter, this will update their kill cooldown
    private void UpdateKillCooldown()
    {
        // Gives the player an update on when they are allowed to kill
        if (currentCooldown <= 0)
        {
            canKill = true;
            killIndicator.text = "Kill: F";
        }
        else
        {
            currentCooldown -= 1 * Time.deltaTime;
            killIndicator.text = currentCooldown.ToString("0");
        }
    }

    // Handles player input for interactions
    private void HandleInteractions()
    {
        isUsing = Input.GetKeyDown(KeyCode.E);

        isHolding = Input.GetKey(KeyCode.E);

        if (isImposter)
        {
            isKilling = Input.GetKeyDown(KeyCode.F);
        }

        isReporting = Input.GetKeyDown(KeyCode.Q);
    }

    // Performs any interactions based on player input
    private void PerformInteractions()
    {
        Use();

        if (canKill) {
            Kill();
        }

        Report();
    }

    // Use whicever object is within range
    private void Use()
    {
        List<GameObject> interactables = new List<GameObject>();

        // Check if there are interactables near the player
        foreach (GameObject detectedObject in rangeSensor.GetDetected())
        {
            if (detectedObject.tag == "Interactable")
            {
                interactables.Add(detectedObject);
            }
        }

        // Interact with the object that first entered the interaction range
        if (interactables.Count != 0)
        {
            GameObject interactable = interactables[0];
            Task task = interactable.GetComponent<Task>();
            task.Interact(isUsing, isHolding);
        }
    }

    // Kill whicever player is within range
    private void Kill()
    {
        if (isKilling)
        {
            List<GameObject> crewmates = new List<GameObject>();

            // Check if there are crewmates near the imposter
            foreach (GameObject detectedObject in rangeSensor.GetDetected())
            {
                if (detectedObject.tag == "Crewmate")
                {
                    crewmates.Add(detectedObject);
                }
            }

            // Kill the crewmate that first entered the kill range
            if (crewmates.Count != 0)
            {
                GameObject crewmate = crewmates[0];

                ClientSend.KillRequest(crewmate.GetComponent<PlayerManager>().id);

                Life crewmateLife = crewmate.GetComponent<Life>();
                crewmateLife.Die();
                crewmate.tag = "Dead";

                currentCooldown = killCooldown;
                canKill = false;
            }
        }
    }

    // Report whicever body is within range
    private void Report()
    {

    }
}
