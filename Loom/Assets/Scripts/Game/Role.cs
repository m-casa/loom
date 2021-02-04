using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SensorToolkit;

public class Role : MonoBehaviour
{
    // Fields
    public Text roleIndicator, killIndicator;
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
        killCooldown = 30f;
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
    public void UpdateRole()
    {
        if (isImposter)
        {
            roleIndicator.color = Color.red;
            roleIndicator.text = "Imposter";

            currentCooldown = killCooldown;
            killIndicator.gameObject.SetActive(true);
        }
        else
        {
            roleIndicator.color = Color.white;
            roleIndicator.text = "Crewmate";
        }

        roleIndicator.gameObject.SetActive(true);
    }

    // If the player is an imposter, this will update their kill cooldown
    private void UpdateKillCooldown()
    {
        if (currentCooldown <= 0)
        {
            canKill = true;
            killIndicator.text = "Kill";
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
                Life life = crewmate.GetComponent<Life>();
                life.Die();
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
