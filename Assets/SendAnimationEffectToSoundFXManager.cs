using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class SendAnimationEffectToSoundFXManager : MonoBehaviour
{
    public PlayerPhotonSoundManager playerPhotonSoundManager;
    // Start is called before the first frame update
    public void TriggerFootstepSFX()
    {
        playerPhotonSoundManager.PlayFootstepSFX();
    }
}
