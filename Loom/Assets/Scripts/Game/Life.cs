using UnityEngine;

public class Life : MonoBehaviour
{
    [HideInInspector]
    public GameObject body;
    public GameObject model, deadModel;
    public bool isDead;
    private GameObject deathPrefab, deathEffect;

    // Awake is called before Start
    public void Awake()
    {
        isDead = false;
        deathPrefab = Resources.Load<GameObject>("Prefabs/Death");
    }

    // Kills the player
    public void Die()
    {
        // Hide the player's body and set their tag as a Ghost
        model.SetActive(false);
        GetComponent<PlayerManager>().nameInidcator.gameObject.SetActive(false);
        gameObject.tag = "Ghost";

        // If this is the local player, do not allow them to fix sabotages
        // Also, let them see dead players
        if (GameManager.players[Client.instance.myId].tag.Equals("Ghost"))
        {
            AudioManager.instance.PlaySound("Killed");

            GameManager.instance.fixElectrical.GetComponent<Task>().resetTask = true;
            GameManager.instance.fixO2[0].GetComponent<Task>().resetTask = true;
            GameManager.instance.fixO2[1].GetComponent<Task>().resetTask = true;
            GameManager.instance.fixReactor[0].GetComponent<Task>().resetTask = true;
            GameManager.instance.fixReactor[1].GetComponent<Task>().resetTask = true;

            foreach (PlayerManager player in GameManager.players.Values)
            {
                if (player.GetComponent<Life>().isDead)
                {
                    player.nameInidcator.gameObject.SetActive(true);
                }
            }
        }

        // Don't spawn a body if it was killed for being ejected
        if (!UIManager.instance.activeMeeting)
        {
            // Spawn in the dead body and assign it the correct color
            body = Instantiate(deadModel);
            body.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = model.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
            body.transform.position = transform.position;

            // Spawn in the blood particle effect
            deathEffect = Instantiate(deathPrefab);
            deathEffect.transform.position = transform.position;
            deathEffect.transform.parent = body.transform;
        }

        isDead = true;
    }

    // Respawn the player
    public void Respawn()
    {
        // Show the player's body again
        model.SetActive(true);
        GetComponent<PlayerManager>().nameInidcator.gameObject.SetActive(true);

        // Destroy the player's dead body if it is still in the ship
        if (body != null)
        {
            Destroy(body);
        }

        isDead = false;
    }
}
