using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SensorToolkit;
using ECM.Components;

public class Role : MonoBehaviour
{
    [HideInInspector]
    public GameObject map, sabotage;
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
        killCooldown = 20;
        emergencyTimer = 15;
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

        // Check if the player is looking at the map
        isUsingMap = Input.GetKeyDown(KeyCode.M);

        // Only be able to open the map if not in a meeting
        if (!UIManager.instance.activeMeeting)
        {
            CheckMap();
        }

        if (isImposter)
        {
            UpdateKillCooldown();
        }

        UpdateEmergencyCooldown();
    }

    // Used to decide if there are interactable objects
    public void CheckForInteractables(int _numOfInteractables)
    {
        numOfInteractables += _numOfInteractables;

        if (numOfInteractables > 0)
        {
            canInteract = true;

            // Change the UI based on the interactables nearby
            foreach (GameObject detectedObject in rangeSensor.GetDetected())
            {
                if (detectedObject.tag == "Interactable")
                {
                    useIndicator.CrossFadeAlpha(1f, 0f, false);
                }
                else if (detectedObject.tag == "Crewmate")
                {
                    killIndicator.CrossFadeAlpha(1f, 0f, false);
                }
                else if (detectedObject.tag == "DeadBody")
                {
                    reportIndicator.CrossFadeAlpha(1f, 0f, false);
                }
            }
        }
        else
        {
            canInteract = false;
            useIndicator.CrossFadeAlpha(0.25f, 0f, false);
            killIndicator.CrossFadeAlpha(0.25f, 0f, false);
            reportIndicator.CrossFadeAlpha(0.25f, 0f, false);
        }
    }

    // Update the role of the player
    public void UpdateRole(bool _isImposter)
    {
        isImposter = _isImposter;

        // Updates the player's HUD to reflect what their role is
        if (isImposter)
        {
            Button[] sabotageButtons = sabotage.GetComponentsInChildren<Button>();

            gameObject.tag = "Imposter";

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
            sabotage.SetActive(true);
            foreach (Button sabotageButton in sabotageButtons)
            {
                if (!sabotageButton.GetComponent<DoorSabotage>())
                {
                    sabotageButton.interactable = false;
                }
            }
            sabotageTimerText.gameObject.SetActive(true);
        }
        else
        {
            gameObject.tag = "Crewmate";

            roleIndicator.color = Color.white;
            roleIndicator.text = "Crewmate";

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

            // If the local player is a crewmate, give them tasks by starting with swipe card
            GameManager.instance.swipeCard.GetComponent<Task>().finished = false;
            GameManager.instance.swipeCard.GetComponent<Task>().outlinable.enabled = true;

            // Pick two random fix wiring tasks
            GameManager.instance.fixWiring[rngFixWiring1].GetComponent<Task>().finished = false;
            GameManager.instance.fixWiring[rngFixWiring1].GetComponent<Task>().outlinable.enabled = true;
            while (rngFixWiring2 == rngFixWiring1)
            {
                rngFixWiring2 = Random.Range(0, 5);
            }
            GameManager.instance.fixWiring[rngFixWiring2].GetComponent<Task>().finished = false;
            GameManager.instance.fixWiring[rngFixWiring2].GetComponent<Task>().outlinable.enabled = true;

            // Pick two random divert power tasks
            GameManager.instance.divertPower[rngDivertPower1].GetComponent<Task>().finished = false;
            GameManager.instance.divertPower[rngDivertPower1].GetComponent<Task>().outlinable.enabled = true;
            while (rngDivertPower2 == rngDivertPower1)
            {
                rngDivertPower2 = Random.Range(0, 10);
            }
            GameManager.instance.divertPower[rngDivertPower2].GetComponent<Task>().finished = false;
            GameManager.instance.divertPower[rngDivertPower2].GetComponent<Task>().outlinable.enabled = true;

            // Pick two random upload data tasks
            GameManager.instance.uploadData[rngUploadData1].GetComponent<Task>().finished = false;
            GameManager.instance.uploadData[rngUploadData1].GetComponent<Task>().outlinable.enabled = true;
            while (rngUploadData2 == rngUploadData1)
            {
                rngUploadData2 = Random.Range(0, 5);
            }
            GameManager.instance.uploadData[rngUploadData2].GetComponent<Task>().finished = false;
            GameManager.instance.uploadData[rngUploadData2].GetComponent<Task>().outlinable.enabled = true;

            // Pick two random short tasks
            GameManager.instance.shortTask[rngShortTask1].GetComponent<Task>().finished = false;
            GameManager.instance.shortTask[rngShortTask1].GetComponent<Task>().outlinable.enabled = true;
            while (rngShortTask2 == rngShortTask1)
            {
                rngShortTask2 = Random.Range(0, 5);
            }
            GameManager.instance.shortTask[rngShortTask2].GetComponent<Task>().finished = false;
            GameManager.instance.shortTask[rngShortTask2].GetComponent<Task>().outlinable.enabled = true;

            // Pick two random long tasks
            GameManager.instance.longTask[rngLongTask1].GetComponent<Task>().finished = false;
            GameManager.instance.longTask[rngLongTask1].GetComponent<Task>().outlinable.enabled = true;
            while (rngLongTask2 == rngLongTask1)
            {
                rngLongTask2 = Random.Range(0, 5);
            }
            GameManager.instance.longTask[rngLongTask2].GetComponent<Task>().finished = false;
            GameManager.instance.longTask[rngLongTask2].GetComponent<Task>().outlinable.enabled = true;
        }

        // If the task bar is not on, then turn it on
        if (!taskBar.gameObject.activeSelf)
        {
            taskBar.gameObject.SetActive(true);
        }

        // If the role indicator is not on then turn it on
        if (!roleIndicator.gameObject.activeSelf)
        {
            roleIndicator.gameObject.SetActive(true);
        }

        // If the timer for the panic button is not on then turn it on
        if (!emergencyTimerText.gameObject.activeSelf)
        {
            // Reset the panic button contraint
            emergencyTimer = 15;

            emergencyTimerText.gameObject.SetActive(true);
        }
    }

    // Update the player's HUD to display the winning team
    public void UpdateWinners(string _winningTeam)
    {
        gameObject.tag = "Untagged";

        // If the fog is on still, turn if off
        if (GameManager.instance.fogSettings.useDistance)
        {
            GameManager.instance.fogSettings.useDistance = false;
            GameManager.instance.fogSettings.useHeight = false;
        }

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

        // If the player was an imposter, revert their role, else reset tasks
        if (isImposter)
        {
            isImposter = false;
            killIndicator.gameObject.SetActive(false);
            sabotage.SetActive(false);
            sabotageTimerText.gameObject.SetActive(false);
        }
        else
        {
            // Reset card swipe
            GameManager.instance.swipeCard.GetComponent<Task>().resetTask = true;

            // Reset short task
            for (int i = 0; i < GameManager.instance.shortTask.Length; i++)
            {
                GameManager.instance.shortTask[i].GetComponent<Task>().resetTask = true;
            }

            // Reset long task
            for (int i = 0; i < GameManager.instance.longTask.Length; i++)
            {
                GameManager.instance.longTask[i].GetComponent<Task>().resetTask = true;
            }

            // Reset fix wiring
            for (int i = 0; i < GameManager.instance.fixWiring.Length; i++)
            {
                GameManager.instance.fixWiring[i].GetComponent<Task>().resetTask = true;
            }

            // Reset divert power
            for (int i = 0; i < GameManager.instance.divertPower.Length; i++)
            {
                GameManager.instance.divertPower[i].GetComponent<Task>().resetTask = true;
            }

            // Reset upload data
            for (int i = 0; i < GameManager.instance.uploadData.Length; i++)
            {
                GameManager.instance.uploadData[i].GetComponent<Task>().resetTask = true;
            }

        }

        // Reset any sabotage settings
        GameManager.instance.fixElectrical.GetComponent<Task>().resetTask = true;

        // If the player was dead, respawn their body
        if (life.isDead)
        {
            life.Respawn();
        }

        // If the task bar is active, then turn it off
        if (taskBar.gameObject.activeSelf)
        {
            taskBar.gameObject.SetActive(false);
        }

        // If the timer for the panic button is active, turn it off
        if (emergencyTimerText.gameObject.activeSelf)
        {
            emergencyTimerText.gameObject.SetActive(false);

            // Reset the number of uses on the emergency button
            if (emergency.numOfUses > 0)
            {
                emergency.numOfUses = 0;
            }
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
            killIndicator.text = "Can kill in: " + currentCooldown.ToString("0") + "s";
        }
    }

    // Update when the player is allowed to use the panic button
    private void UpdateEmergencyCooldown()
    {
        // Gives the player an update on when they are allowed to panic
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
                GameObject crewmate = crewmates[0];

                ClientSend.KillRequest(crewmate.GetComponent<PlayerManager>().id);

                Life crewmateLife = crewmate.GetComponent<Life>();
                crewmateLife.Die();
                crewmate.GetComponent<PlayerManager>().nameInidcator.gameObject.SetActive(false);

                currentCooldown = killCooldown;
                canKill = false;
            }
        }
    }

    // Report whicever body is within range
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
            if (map.activeSelf)
            {
                map.SetActive(false);
                GetComponent<MouseLook>().SetCursorLock(true);
            }
            else
            {
                map.SetActive(true);
                GetComponent<MouseLook>().SetCursorLock(false);
            }
        }
    }
}
