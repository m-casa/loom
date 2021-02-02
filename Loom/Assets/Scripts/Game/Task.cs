using UnityEngine;

public class Task : MonoBehaviour
{
    // Is used to pass the current interaction state of the player to
    //  an interactable object
    public void Interact(bool _isUsing)
    {
        if (_isUsing)
        Debug.Log(_isUsing);
    }
}
