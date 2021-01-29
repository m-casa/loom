using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    [SerializeField]
    private Material[] color;
    [SerializeField]
    private GameObject model, deadModel;

    // Change to the color selected on the main menu
    public void ChangeColor(int _color)
    {
        model.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = color[_color];
    }
}
