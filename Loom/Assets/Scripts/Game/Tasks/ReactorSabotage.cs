using UnityEngine;
using UnityEngine.UI;

public class ReactorSabotage : MonoBehaviour
{
    public Button button;

    // Can request the server to enable the fog to represent no lights
    public void MeltdownReactor()
    {
        ClientSend.SabotageReactor();
    }
}
