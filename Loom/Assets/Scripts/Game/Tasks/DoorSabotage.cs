using UnityEngine;
using UnityEngine.UI;

public class DoorSabotage : MonoBehaviour
{
    public GameObject[] doors;
    public Button button;

    // Request the server to close this area's doors
    public void CloseDoors()
    {
        ClientSend.SabotageDoors(doors);
    }
}
