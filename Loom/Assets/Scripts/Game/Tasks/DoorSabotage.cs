using UnityEngine;
using UnityEngine.UI;

public class DoorSabotage : MonoBehaviour
{
    public GameObject[] doors;
    public Button button;
    public float doorCooldown, currentCooldown;
    public bool activeCooldown;

    // Awake is called before Start
    public void Awake()
    {
        doorCooldown = 25;
        activeCooldown = false;
    }

    // Update is called once per frame
    public void Update()
    {
        if (activeCooldown)
        {
            if (currentCooldown <= 0)
            {
                activeCooldown = false;

                button.interactable = true;
            }
            else
            {
                currentCooldown -= 1 * Time.deltaTime;
            }
        }
    }

    // Request the server to close this area's doors
    public void CloseDoors()
    {
        ClientSend.SabotageDoors(doors);
    }
}
