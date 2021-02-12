using UnityEngine;

public class Garbage : MonoBehaviour
{
    // Start is called before the first frame update
    public Task task;
    public Dumpster1 dumpster1;
    public Dumpster2 dumpster2;
    public bool taskFinished, filledDumpster1;
    private float taskTime, timeToFinish;

    // Awake is called before Start
    public void Awake()
    {
        taskTime = 4;
        timeToFinish = taskTime;
        taskFinished = false;
    }

    // Update is called once per frame
    public void Update()
    {
        // Check if this task is finished, else if the player is using the task, begin counting down to finish
        if (timeToFinish <= 0 && !taskFinished)
        {
            taskFinished = true;
            task.outlinable.enabled = false;

            // Check if we've finished the first part of the task, else finish the second part
            if (!filledDumpster1)
            {
                filledDumpster1 = true;
                dumpster1.taskFinished = false;
                dumpster1.task.outlinable.enabled = true;
                Debug.Log("Finished collecting garbage!");

                // Reset the task states to false
                task.isBeingUsed = false;
                task.isBeingHeld = false;
                timeToFinish = taskTime;
            }
            else
            {
                filledDumpster1 = false;
                dumpster2.taskFinished = false;
                dumpster2.task.outlinable.enabled = true;
                Debug.Log("Finished collecting garbage!");

                // Reset the task states to false
                task.isBeingUsed = false;
                task.isBeingHeld = false;
                timeToFinish = taskTime;
            }
        }
        else if (task.isBeingHeld && !taskFinished)
        {
            timeToFinish -= 1 * Time.deltaTime;

            // Update the player's progress bar to represent when the task will finsih
            GameManager.players[Client.instance.myId].GetComponent<Role>().progressBar.value = ((taskTime - timeToFinish) / taskTime) * 100;

            // Reset the task states to false
            task.isBeingUsed = false;
            task.isBeingHeld = false;
        }
    }
}
