using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SensorToolkit;
using ECM.Components;

public class Role : MonoBehaviour
{
    [HideInInspector]
    public GameObject map;
    [HideInInspector]
    public Emergency emergency;

    public Text roleIndicator, useIndicator, killIndicator, reportIndicator, 
        emergencyTimerText, sabotageTimerText;
    public Slider progressBar, taskBar;
    public Life life;
    public float killCooldown, currentCooldown, emergencyTimer;
    public int numOfInteractables;
    public bool isImposter, canInteract, canKill;
    private RangeSensor rangeSensor;
    private bool isUsing, isHolding, isKilling, isReporting, isUsingMap;

    // Awake is called before Start
    public void Awake()
    {
        useIndicator.CrossFadeAlpha(0.25f, 0f, false);
        killIndicator.CrossFadeAlpha(0.25f, 0f, false);
        reportIndicator.CrossFadeAlpha(0.25f, 0f, false);
        rangeSensor = GetComponent<RangeSensor>();
        canKill = false;
        canInteract = false;
        killCooldown = 25;
        currentCooldown = killCooldown;
        emergencyTimer = 25;
        numOfInteractables = 0;
    }

    // Update is called once per frame
    public void Update()
    {
        // Update the Sensor and UI
        rangeSensor.Pulse();
        UpdateUI();

        // If interactables were found, allow interactions
        if (canInteract)
        {
            // Check for player input
            HandleInteractions();

            // Perform any interaction the player wants
            PerformInteractions();
        }

        // Player progress bar should be empty when no tasks are being done
        if (!canInteract || !isHolding)
        {
            progressBar.value = 0;
        }

        // If the player isn't currently in a meeting, let them check the map
        if (!UIManager.instance.activeMeeting)
        {
            isUsingMap = Input.GetKeyDown(KeyCode.M);
            CheckMap();
        }

        // Update emergency and kill cooldowns
        UpdateEmergencyCooldown();
        if (isImposter)
        {
            UpdateKillCooldown();
        }
    }

    // Update the UI depending on the objects near by
    public void UpdateUI()
    {
        useIndicator.CrossFadeAlpha(0.25f, 0f, false);
        killIndicator.CrossFadeAlpha(0.25f, 0f, false);
        reportIndicator.CrossFadeAlpha(0.25f, 0f, false);

        foreach (GameObject detectedObject in rangeSensor.GetDetected())
        {
            if (!life.isDead)
            {
                if (detectedObject.tag.Equals("Interactable"))
                {
                    useIndicator.CrossFadeAlpha(1f, 0f, false);
                }
                else if (detectedObject.tag.Equals("Crewmate"))
                {
                    killIndicator.CrossFadeAlpha(1f, 0f, false);
                }
                else if (detectedObject.tag.Equals("DeadBody"))
                {
                    reportIndicator.CrossFadeAlpha(1f, 0f, false);
                }
            }
            else if (detectedObject.tag.Equals("Interactable") && !detectedObject.GetComponent<Emergency>())
            {
                useIndicator.CrossFadeAlpha(1f, 0f, false);
            }
        }
    }

    // Decide if the player can interact with objects
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
        CloseMap();

        isImposter = _isImposter;

        // Update the player's HUD to reflect what their role is
        if (isImposter)
        {
            // Let the player know their role, and let them see their imposter abilities
            gameObject.tag = "Imposter";
            roleIndicator.color = Color.red;
            roleIndicator.text = "Imposter";
            killIndicator.gameObject.SetActive(true);
            sabotageTimerText.gameObject.SetActive(true);

            // If the local player became an imposter after the other player, then
            //  look for that player and change their nameplate to red so the local player knows who their teammate is
            foreach (PlayerManager player in GameManager.players.Values)
            {
                if (player.tag.Equals("Imposter"))
                {
                    player.nameInidcator.color = Color.red;
                }
            }

            // Don't allow the player to kill right away
            currentCooldown = killCooldown;

            // Don't allow the player to sabotage right away
            GameManager.instance.DeactivateSabotages();
            GameManager.instance.DeactivateDoorSabotages();
        }
        else
        {
            // Let the player know their role
            gameObject.tag = "Crewmate";
            roleIndicator.color = Color.green;
            roleIndicator.text = "Crewmate";

            GameManager.instance.AssignTasks();
        }

        // If the task bar is off, turn it on
        if (!taskBar.gameObject.activeSelf)
        {
            taskBar.gameObject.SetActive(true);
        }

        // If the role indicator is off, turn it on
        if (!roleIndicator.gameObject.activeSelf)
        {
            roleIndicator.gameObject.SetActive(true);
        }

        // If the timer for the emergency button is off, turn it on
        if (!emergencyTimerText.gameObject.activeSelf)
        {
            // Reset the panic button contraint
            emergencyTimer = 25;
            emergencyTimerText.gameObject.SetActive(true);
        }
    }

    // Update the player's HUD to display the winning team
    public void UpdateWinners(string _winningTeam)
    {
        CloseMap();

        // Update the player's HUD to reflect which team won the last round
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

        UIManager.instance.gameTimerText.gameObject.SetActive(false);

        gameObject.tag = "Untagged";

        // If the player is dead, respawn their body
        if (life.isDead)
        {
            life.Respawn();
        }

        // If the player was an imposter revert their role, else reset the tasks
        if (isImposter)
        {
            isImposter = false;
            killIndicator.gameObject.SetActive(false);
            sabotageTimerText.gameObject.SetActive(false);
        }
        else
        {
            GameManager.instance.ResetTasks();
        }

        // Reset any current sabotages
        GameManager.instance.TurnOnLights();
        GameManager.instance.TurnOnO2(3);
        GameManager.instance.RestoreReactor();
        GameManager.instance.ResetDoors();

        // If the task bar is on, turn it off
        if (taskBar.gameObject.activeSelf)
        {
            taskBar.gameObject.SetActive(false);
        }

        // If the timer for the emergency button is on, turn it off
        if (emergencyTimerText.gameObject.activeSelf)
        {
            emergencyTimerText.gameObject.SetActive(false);
            emergency.numOfUses = 0;
        }
    }

    // Update when the player is allowed to use the panic button
    private void UpdateEmergencyCooldown()
    {
        // Give the player an update on when they are allowed to panic
        if (emergencyTimer <= 0)
        {
            emergency.canPanic = true;
            emergencyTimerText.text = "Emergency available!";
        }
        else
        {
            emergency.canPanic = false;
            emergencyTimer -= 1 * Time.deltaTime;
            emergencyTimerText.text = "Emergency button in: " + emergencyTimer.ToString("0") + "s";
        }
    }

    // If the player is an imposter, update their kill cooldown
    private void UpdateKillCooldown()
    {
        // Give the player an update on when they are allowed to kill
        if (currentCooldown <= 0)
        {
            canKill = true;
            killIndicator.text = "Kill: F";
        }
        else
        {
            currentCooldown -= 1 * Time.deltaTime;
            killIndicator.text = "Can kill in: " + currentCooldown.ToString("0") + "s";
        }
    }

    // Handle player input for interactions
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

    // Perform any interactions based on player input
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
        GameObject closestInteractable;
        float shortestDistanceFromPlayer, newDistance;

        // Check if there are interactables near the player
        foreach (GameObject detectedObject in rangeSensor.GetDetected())
        {
            if (detectedObject.tag == "Interactable")
            {
                interactables.Add(detectedObject);
            }
        }

        // Interact with the object that is closest to the player
        if (interactables.Count != 0)
        {
            // Default closest will be the first interactable
            closestInteractable = interactables[0];
            shortestDistanceFromPlayer = Vector3.Distance(closestInteractable.transform.position, gameObject.transform.position);

            foreach (GameObject interactable in interactables)
            {
                newDistance = Vector3.Distance(interactable.transform.position, gameObject.transform.position);

                // If the new distance is shorter, set this interactable as the closest
                if (newDistance < shortestDistanceFromPlayer)
                {
                    closestInteractable = interactable;
                    shortestDistanceFromPlayer = newDistance;
                }
            }

            Task task = closestInteractable.GetComponent<Task>();
            task.Interact(isUsing, isHolding);

            // If the player is trying to fix the reactor, the script needs to know they are near
            if (closestInteractable.GetComponent<FixReactor>())
            {
                closestInteractable.GetComponent<FixReactor>().isNearPad = true;
            }
        }
    }

    // Kill whicever player is within range if not dead
    private void Kill()
    {
        if (isKilling && !life.isDead)
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
                PlayerManager crewmate = crewmates[0].GetComponent<PlayerManager>();

                ClientSend.KillRequest(crewmate.GetComponent<PlayerManager>().id);

                Life crewmateLife = crewmate.GetComponent<Life>();
                crewmateLife.Die();
                crewmate.nameInidcator.gameObject.SetActive(false);

                currentCooldown = killCooldown;
                canKill = false;
            }
        }
    }

    // Report whicever body is within range if not dead
    private void Report()
    {
        if (isReporting && !life.isDead)
        {
            List<GameObject> bodies = new List<GameObject>();

            // Check if there are bodies near the player
            foreach (GameObject detectedObject in rangeSensor.GetDetected())
            {
                if (detectedObject.tag == "DeadBody")
                {
                    bodies.Add(detectedObject);
                }
            }

            // Report the body that first entered the report range
            if (bodies.Count != 0)
            {
                // Destroy any dead bodies near the player to prevent report spamming
                foreach (GameObject deadBody in bodies)
                {
                    Destroy(deadBody);
                }

                ClientSend.ReportRequest();
            }
        }
    }

    // Interact with the map
    private void CheckMap()
    {
        if (isUsingMap)
        {
            // If the player is looking at the map, close it
            // Else, open the map and show sabotages if the player is an imposter
            if (map.GetComponent<Image>().enabled)
            {
                CloseMap();
            }
            else
            {
                OpenMap();
            }
        }
    }

    // Closes the map
    public void CloseMap()
    {
        map.GetComponentInChildren<RawImage>().enabled = false;
        map.GetComponentInChildren<Text>().enabled = false;
        foreach (Image image in map.GetComponentsInChildren<Image>())
        {
            image.enabled = false;
        }
        foreach (Button button in map.GetComponentsInChildren<Button>())
        {
            button.enabled = false;
        }

        GetComponent<MouseLook>().SetCursorLock(true);
    }

    // Opens the map
    private void OpenMap()
    {
        if (isImposter)
        {
            map.GetComponentInChildren<RawImage>().enabled = true;
            map.GetComponentInChildren<Text>().enabled = true;
            foreach (Image image in map.GetComponentsInChildren<Image>())
            {
                image.enabled = true;
            }
            foreach (Button button in map.GetComponentsInChildren<Button>())
            {
                button.enabled = true;
            }
        }
        else
        {
            map.GetComponent<Image>().enabled = true;
            map.GetComponentInChildren<RawImage>().enabled = true;
        }

        GetComponent<MouseLook>().SetCursorLock(false);
    }
}
