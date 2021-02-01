using UnityEngine;
using SensorToolkit;
using System.Collections.Generic;

public class Role : MonoBehaviour
{
    // Fields
    public bool isCewmate, isImposter, canKill;
    private GameObject interactableObject;
    private RangeSensor rangeSensor;
    private Life crewmateLife;
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

    // Start is called before the first frame update
    public void Start()
    {

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
    public void HandleInteractions()
    {
        isUsing = Input.GetKeyDown(KeyCode.E);

        if (isImposter)
        {
            isKilling = Input.GetKeyDown(KeyCode.F);
        }

        isReporting = Input.GetKeyDown(KeyCode.Q);
    }

    // Performs any interactions based on player input
    public void PerformInteractions()
    {
        if (isUsing) {
            Use();
        }
        else if (canKill && isKilling) {
            Kill();
        }
        else if (isReporting) {
            Report();
        }
    }

    // Use whicever object is within range
    public void Use()
    {
        interactableObject = rangeSensor.GetNearest();
    }

    // Kill whicever player is within range
    public void Kill()
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
            interactableObject = crewmates[0];
            crewmateLife = interactableObject.GetComponent<Life>();
            crewmateLife.Die();
        }
    }

    // Report whicever body is within range
    public void Report()
    {
        interactableObject = rangeSensor.GetNearest();
    }
}
