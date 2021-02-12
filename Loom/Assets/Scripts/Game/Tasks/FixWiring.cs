using UnityEngine;

public class FixWiring : MonoBehaviour
{
    public Task task;
    public bool taskFinished;
    private float taskTime, timeToFinish;

    // Awake is called before Start
    public void Awake()
    {
        taskTime = 6;
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
            Debug.Log("Finished fixing wires!");

            // Reset the task states to false
            task.isBeingUsed = false;
            task.isBeingHeld = false;
            timeToFinish = taskTime;
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
