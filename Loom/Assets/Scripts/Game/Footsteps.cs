using ECM.Controllers;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public void PlayRandomFootstep()
    {
        if (GetComponentInParent<LocalFirstPersonController>().moveDirection != Vector3.zero)
        {
            Sound footstep = AudioManager.instance.GetRandomFootstep();
            footstep.audioSource.Play();
        }
    }
}
