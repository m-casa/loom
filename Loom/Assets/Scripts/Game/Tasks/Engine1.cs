using UnityEngine;

public class Engine1 : MonoBehaviour
{
    // Start is called before the first frame update
    public Task task;
    public Fuel fuel;
    private float taskTime, timeToFinish;

    // Awake is called before Start
    public void Awake()
    {
        taskTime = 4;
        timeToFinish = taskTime;
    }

    // Update is called once per frame
    public void Update()
    {
        // Check if this task is finished, else if the player is using the task, begin counting down to finish
        if (timeToFinish <= 0 && !task.finished)
        {
            AudioManager.instance.StopSound("Using");
            AudioManager.instance.PlaySound("Complete");
            GameManager.players[Client.instance.myId].GetComponent<Role>().progressBar.value = 0;

            task.finished = true;
            task.outlinable.enabled = false;
            fuel.task.finished = false;
            fuel.task.outlinable.enabled = true;

            // Reset the task states to false
            task.isBeingUsed = false;
            task.isBeingHeld = false;
            timeToFinish = taskTime;
        }
        else if (task.isBeingHeld && !task.finished)
        {
            if (!AudioManager.instance.CheckSound("Using"))
            {
                AudioManager.instance.PlaySound("Using");
            }

            timeToFinish -= 1 * Time.deltaTime;

            // Update the player's progress bar to represent when the task will finsih
            GameManager.players[Client.instance.myId].GetComponent<Role>().progressBar.value = (taskTime - timeToFinish) / taskTime * 100;

            // Reset the task states to false
            task.isBeingUsed = false;
            task.isBeingHeld = false;
        }

        if (task.resetTask)
        {
            // Reset this task
            task.resetTask = false;
            timeToFinish = taskTime;
            task.finished = true;
            task.outlinable.enabled = false;
        }
    }
}
