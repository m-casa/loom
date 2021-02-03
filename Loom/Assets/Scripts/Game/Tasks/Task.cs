using UnityEngine;

public class Task : MonoBehaviour
{
    public bool isBeingUsed;

    // Awake is called before Start
    public void Awake()
    {
        isBeingUsed = false;
    }

    // Is used to pass the current interaction state of the player to
    //  an interactable object
    public void Interact(bool _isBeingUsed)
    {
        isBeingUsed = _isBeingUsed;
    }
}
