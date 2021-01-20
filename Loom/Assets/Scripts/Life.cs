using UnityEngine;

public class Life : MonoBehaviour
{
    [SerializeField]
    private GameObject model, deadModel;
    private GameObject deathPrefab, deathEffect;

    // Awake is called before Start
    public void Awake()
    {
        deathPrefab = Resources.Load<GameObject>("Prefabs/Death");
    }

    // Start is called before the first frame update
    public void Start()
    {

    }

    // Update is called once per frame
    public void Update()
    {

    }

    // Kills the player
    public void Die()
    {
        // Swap from the current model to the dead one
        model.SetActive(false);
        deadModel.SetActive(true);

        // Spawn in the blood particle effect
        deathEffect = (GameObject) Instantiate(deathPrefab);
        deathEffect.transform.position = transform.position;
        deathEffect.transform.parent = transform;
    }
}
