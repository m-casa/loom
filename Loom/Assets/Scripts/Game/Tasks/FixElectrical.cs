using UnityEngine;
using UnityEngine.UI;

public class FixElectrical : MonoBehaviour
{

    public Task task;
    private float taskTime, timeToFinish;
    public Button button;

    // Awake is called before Start
    public void Awake()
    {
        taskTime = 3;
        timeToFinish = taskTime;
    }

    // Update is called once per frame
    public void Update()
    {
        // Check if this task is finished, else if the player is using the task, begin counting down to finish
        if (timeToFinish <= 0 && !task.finished)
        {
            task.finished = true;
            task.outlinable.enabled = false;
            Debug.Log("Finished fixing eletrical!");
            ClientSend.FixElectrical();

            // Reset the task states to false
            task.isBeingUsed = false;
            task.isBeingHeld = false;
            timeToFinish = taskTime;
        }
        else if (task.isBeingHeld && !task.finished)
        {
            timeToFinish -= 1 * Time.deltaTime;

            // Update the player's progress bar to represent when the task will finsih
            GameManager.players[Client.instance.myId].GetComponent<Role>().progressBar.value = ((taskTime - timeToFinish) / taskTime) * 100;

            // Reset the task states to false
            task.isBeingUsed = false;
            task.isBeingHeld = false;
        }

        if (task.resetTask)
        {
            // Reset this task
            task.resetTask = false;
            task.finished = true;
            task.outlinable.enabled = false;
            timeToFinish = taskTime;
        }
    }
}
