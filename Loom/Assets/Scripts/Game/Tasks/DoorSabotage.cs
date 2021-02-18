using UnityEngine;
using UnityEngine.UI;

public class DoorSabotage : MonoBehaviour
{
    public GameObject[] doors;
    public Button button;
    public ColorBlock newColor;
    public float doorCooldown, currentCooldown, ogAlphaValue, alphaValue;
    public bool activeCooldown;

    // Awake is called before Start
    public void Awake()
    {
        newColor = button.colors;
        doorCooldown = 20;
        ogAlphaValue = button.colors.disabledColor.a;
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
