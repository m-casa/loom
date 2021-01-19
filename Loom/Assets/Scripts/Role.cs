using UnityEngine;
using SensorToolkit;

public class Role : MonoBehaviour
{
    // Fields
    public bool isCewmate, isImposter, canKill;
    private GameObject interactableObject;
    private RangeSensor rangeSensor;
    private Life crewmateLife;
    private bool use, kill, report;

    // Properties
    public bool canInteract { get; set; }

    // Awake is called before Start
    public void Awake()
    {
        rangeSensor = GetComponent<RangeSensor>();
        canKill = false;
        canInteract = false;
    }

    // Start is called before the first frame update
    public void Start()
    {

    }

    // Update is called once per frame
    public void Update()
    {
        // Check if player is viewing an interactable object
        if (canInteract)
        {
            // Check for player input
            HandleInteractions();

            // Perform any interaction the player wants
            PerformInteractions();
        }
    }

    // Handles player input for interactions
    public void HandleInteractions()
    {
        use = Input.GetKeyDown(KeyCode.E);

        if (isImposter)
        {
            kill = Input.GetKeyDown(KeyCode.F);
        }

        report = Input.GetKeyDown(KeyCode.Q);
    }

    // Performs any interactions based on player input
    public void PerformInteractions()
    {
        if (use) {
            Use();
        }
        else if (canKill && kill) {
            Kill();
        }
        else if (report) {
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
        interactableObject = rangeSensor.GetNearest();

        if (interactableObject.tag == "Crewmate")
        {
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
