using UnityEngine;
using EPOOutline;

public class Task : MonoBehaviour
{
    public Outlinable outlinable;
    public bool isBeingUsed;
    public bool isBeingHeld;

    // Awake is called before Start
    public void Awake()
    {
        isBeingUsed = false;
    }

    // Is used to pass the current interaction state of the player to
    //  an interactable object
    public void Interact(bool _isBeingUsed, bool _isBeingHeld)
    {
        isBeingUsed = _isBeingUsed;
        isBeingHeld = _isBeingHeld;
    }
}
