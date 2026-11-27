using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerPhotonSoundManager : MonoBehaviour
{
    public AudioSource footstepSource;
    public AudioClip footstepSFX;

    public AudioSource gunShootSFX;
    public AudioClip[] allGunShootsSFX;

    public void PlayFootstepSFX()
    {
        GetComponent<PhotonView>().RPC(methodName:"PlayFootStep_RPC",RpcTarget.All);
    }
    [PunRPC]
    public void PlayFootStep_RPC()
    {
        footstepSource.clip = footstepSFX;
        
        //Pich and volumn
        footstepSource.pitch = UnityEngine.Random.Range(0.7f,1.2f);
        footstepSource.volume = UnityEngine.Random.Range(0.2f,0.3f);

        footstepSource.Play();
    }

    public void PlayShootSFX(int index)
    {
        GetComponent<PhotonView>().RPC(methodName:"PlayShootSFX_RPC",RpcTarget.All, index);
    }


    [PunRPC]
    public void PlayShootSFX_RPC(int index)
    {
        gunShootSFX.clip = allGunShootsSFX[index];
        //Pich and volumn
        gunShootSFX.pitch = UnityEngine.Random.Range(0.7f,1.2f);
        gunShootSFX.volume = UnityEngine.Random.Range(0.2f,0.3f);

        gunShootSFX.Play();
    }
}
