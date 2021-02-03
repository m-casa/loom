using UnityEngine;

public class RoundController : MonoBehaviour
{
    public Task task;

    // Update is called once per frame
    void Update()
    {
        if (task.isBeingUsed && GameManager.instance.localPlayerManager.id == 1)
        {
            if (GameManager.players.Count >= 4)
            {
                ClientSend.RoundRequest();
                task.isBeingUsed = false;
            }
            else
            {

            }
        }
    }
}
