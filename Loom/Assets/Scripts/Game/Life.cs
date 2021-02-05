using UnityEngine;

public class Life : MonoBehaviour
{
    public bool isDead;
    [SerializeField]
    private GameObject model, deadModel;
    private GameObject deadBody, deathPrefab, deathEffect;

    // Awake is called before Start
    public void Awake()
    {
        isDead = false;
        deathPrefab = Resources.Load<GameObject>("Prefabs/Death");
    }

    // Kills the player
    public void Die()
    {
        // Hide the player's body
        model.SetActive(false);

        // Spawn in the dead body and assign it the correct color
        deadBody = Instantiate(deadModel);
        deadBody.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = model.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
        deadBody.transform.position = transform.position;

        // Spawn in the blood particle effect
        deathEffect = Instantiate(deathPrefab);
        deathEffect.transform.position = transform.position;
        deathEffect.transform.parent = deadBody.transform;

        isDead = true;
    }

    // Respawn the player
    public void Respawn()
    {
        // Show the player's body again
        model.SetActive(true);

        // Destroy the player's dead body if it is still in the ship
        if (deadBody != null)
        {
            Destroy(deadBody);
        }

        isDead = false;
    }
}
