using UnityEngine;
using UnityEngine.UI;

public class ElectricalSabotage : MonoBehaviour
{
    public Button button;

    // Can request the server to enable the fog to represent no lights
    public void TurnOffLights()
    {
        // If the fog is not yet enabled, request to enable it and disable the button
        if (!GameManager.instance.fogSettings.useDistance)
        {
            ClientSend.SabotageElectrical();
        }
    }
}
