using UnityEngine;
using UnityEngine.UI;

public class O2Sabotage : MonoBehaviour
{
    public Button button;

    // Can request the server to enable the fog to represent no lights
    public void TurnOffO2()
    {
        ClientSend.SabotageO2();
    }
}
