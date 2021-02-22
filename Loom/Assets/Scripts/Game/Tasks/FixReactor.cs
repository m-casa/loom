using UnityEngine;

public class FixReactor : MonoBehaviour
{
    public Task task;
    public int reactorPadId;
    public bool isNearPad, padNeedsReset;

    // Update is called once per frame
    public void Update()
    {
        // Check if this task is being held and update the server on its status
        if (!task.finished && isNearPad)
        {
            // Update the player's progress bar to represent if the reactor is pad is being held or not
            if (task.isBeingHeld)
            {
                if (!AudioManager.instance.CheckSound("Using"))
                {
                    AudioManager.instance.PlaySound("Using");
                }

                GameManager.players[Client.instance.myId].GetComponent<Role>().progressBar.value = 100;
            }

            ClientSend.FixReactor(reactorPadId, task.isBeingHeld);

            // Reset the task states to false
            task.isBeingUsed = false;
            task.isBeingHeld = false;
            isNearPad = false;
            padNeedsReset = true;
        }
        else if (padNeedsReset)
        {
            ClientSend.FixReactor(reactorPadId, task.isBeingHeld);
            padNeedsReset = false;
        }

        if (task.resetTask)
        {
            if (task.isBeingHeld)
            {
                AudioManager.instance.StopSound("Using");
                AudioManager.instance.PlaySound("Complete");
                GameManager.players[Client.instance.myId].GetComponent<Role>().progressBar.value = 0;
            }

            // Reset this task
            task.resetTask = false;
            task.finished = true;
            task.outlinable.enabled = false;
        }
    }
}
