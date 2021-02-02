using System.Collections.Generic;
using UnityEngine;
using SensorToolkit;

public class Role : MonoBehaviour
{
    // Fields
    public bool isCewmate, isImposter, canKill;
    private RangeSensor rangeSensor;
    private bool canInteract, isUsing, isKilling, isReporting;
    private int numOfInteractables;

    // Awake is called before Start
    public void Awake()
    {
        rangeSensor = GetComponent<RangeSensor>();
        canKill = false;
        canInteract = false;
        numOfInteractables = 0;
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

    // Handles player input for interactions
    private void HandleInteractions()
    {
        isUsing = Input.GetKey(KeyCode.E);

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
            task.Interact(isUsing);
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
            }
        }
    }

    // Report whicever body is within range
    private void Report()
    {

    }
}
