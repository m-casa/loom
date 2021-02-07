using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public int id, colorId;
    public string username;
    public GameObject localPlayer;
    public Material[] color;
    public Sprite[] icon;
    public Text nameInidcator;
    public SpriteRenderer spriteRenderer;
    public SkinnedMeshRenderer skinnedMeshRenderer;

    // LateUpdate is called after Update
    public void LateUpdate()
    {
        // Have this player's names face the local player
        nameInidcator.transform.LookAt(nameInidcator.transform.position + localPlayer.transform.forward);
    }

    // Change to the username chosen on the main menu
    public void ChangeName()
    {
        nameInidcator.text = username;
    }

    // Change the icon to represent the chosen color
    public void ChangeIcon()
    {
        spriteRenderer.sprite = icon[colorId];
    }

    // Change to the color selected on the main menu
    public void ChangeColor()
    {
        skinnedMeshRenderer.sharedMaterial = color[colorId];
    }
}
