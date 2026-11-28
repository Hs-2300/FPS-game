using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class SendAnimationEffectToSoundFXManager : MonoBehaviour
{
    public PlayerPhotonSoundManager playerPhotonSoundManager;
    private Movement movement;

    void Awake()
    {
        movement = GetComponentInParent<Movement>();
    }
    // Start is called before the first frame update
    public void TriggerFootstepSFX()
    {
        if (movement != null && movement.CanPlayFootstep && playerPhotonSoundManager != null)
            playerPhotonSoundManager.PlayFootstepSFX();
    }
}
