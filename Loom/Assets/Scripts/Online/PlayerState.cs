using UnityEngine;

public class PlayerState
{
    public Vector3 moveDirection;
    public Vector3 position;

    public PlayerState()
    {
        moveDirection = Vector3.zero;
        position = Vector3.zero;
    }
}