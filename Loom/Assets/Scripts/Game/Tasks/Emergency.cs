using UnityEngine;

public class Emergency : MonoBehaviour
{
    public Task task;
    public bool canPanic;
    public int numOfUses;

    // Awake is called before Start
    public void Awake()
    {
        canPanic = false;
        numOfUses = 0;
    }

    // Update is called once per frame
    public void Update()
    {
        // Only allow emergency meetings if there are no active sabotages
        if (GameManager.instance.fixElectrical.GetComponent<Task>().finished && 
            GameManager.instance.fixO2[0].GetComponent<Task>().finished)
        {
            // Only request a meeting if this player hasn't used up all of their emergency meetings
            if (numOfUses < 1)
            {
                // If the time constraint for hitting the emergency button is over, request a meeting
                if (canPanic && task.isBeingUsed)
                {
                    // Check if the local player is not a ghost
                    if (!GameManager.players[Client.instance.myId].tag.Equals("Ghost"))
                    {
                        ClientSend.MeetingRequest();

                        // Reset the task states to false
                        task.isBeingUsed = false;
                        task.isBeingHeld = false;

                        numOfUses++;
                    }
                }
            }
        }
    }
}
