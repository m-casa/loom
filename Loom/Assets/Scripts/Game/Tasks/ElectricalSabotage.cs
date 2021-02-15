using UnityEngine;
using UnityEngine.UI;

public class ElectricalSabotage : MonoBehaviour
{
    public Button button;

    // Enables the fog to represent no lights
    public void TurnOffLights()
    {
        // If the fog is not yet enabled, request to enable it and disable the button
        if (!GameManager.instance.fogSettings.useDistance)
        {
            ClientSend.SabotageElectrical();
        }
    }
}
