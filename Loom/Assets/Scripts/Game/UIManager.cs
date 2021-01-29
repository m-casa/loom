using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject startMenu;
    public InputField usernameField;
    public Dropdown colorField;

    // Make sure there is only once instance of this client
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    // Will connect the client to the game server
    public void ConnectToServer()
    {
        usernameField.gameObject.SetActive(false);
        colorField.gameObject.SetActive(true);
        Client.instance.ConnectToServer();
    }
}
