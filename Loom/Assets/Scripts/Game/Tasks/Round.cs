using UnityEngine;

public class Round : MonoBehaviour
{
    public Task task;

    // Update is called once per frame
    void Update()
    {
        // If the host is trying to start the round, request the server to start the round
        if (task.isBeingUsed && Client.instance.myId == 1)
        {
            // Only request a round if there are at least 4 players in the game
            if (GameManager.players.Count >= 4)
            {
                ClientSend.RoundRequest();

                // Reset the task states to false
                task.isBeingUsed = false;
                task.isBeingHeld = false;
            }
        }
    }
}
