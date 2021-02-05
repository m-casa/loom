using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public Material[] color;
    public GameObject model, localPlayer;
    public Text nameInidcator;

    // LateUpdate is called after Update
    public void LateUpdate()
    {
        // Have this player's names face the local player
        nameInidcator.transform.LookAt(nameInidcator.transform.position + localPlayer.transform.forward);
    }

    // Change to the color selected on the main menu
    public void ChangeColor(int _color)
    {
        model.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = color[_color];
    }

    // Change to the username chosen on the main menu
    public void ChangeName(string _username)
    {
        nameInidcator.text = _username;
    }
}
