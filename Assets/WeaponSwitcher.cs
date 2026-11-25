using System.Collections;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    public AnimationClip holster;
    public AnimationClip draw;
    
    private int selectedWeapon = 0;
    private bool isSwitching;
 
    // Start is called before the first frame update
    void Start()
    {
        SelectWeapon();
    }

    // Update is called once per frame
    void Update()
    {
        if (isSwitching || transform.childCount == 0)
        {
            return;
        }

        int previousSelectedWeapon = selectedWeapon;

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedWeapon = 0;
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedWeapon = 1;
        }
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedWeapon = 2;
        }
        if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            selectedWeapon = 3;
        }
        if(Input.GetKeyDown(KeyCode.Alpha5))
        {
            selectedWeapon = 4;
        }
        

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (selectedWeapon>=transform.childCount - 1)
            {
                selectedWeapon = 0;
            }
            else
            {
                selectedWeapon += 1;
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (selectedWeapon<=0)
            {
                selectedWeapon = transform.childCount - 1;
            }
            else
            {
                selectedWeapon -= 1;
            }
        }

        if (previousSelectedWeapon != selectedWeapon)
        {
            if (selectedWeapon >= transform.childCount)
            {
                selectedWeapon = transform.childCount - 1;
            }

            if (previousSelectedWeapon != selectedWeapon)
            {
                StartCoroutine(SwitchWeapon(previousSelectedWeapon));
            }
        }

    }

    IEnumerator SwitchWeapon(int previousWeapon)
    {
        isSwitching = true;

        Animation oldAnimation = transform.GetChild(previousWeapon).GetComponent<Animation>();
        yield return PlayWeaponAnimation(oldAnimation, holster);

        SelectWeapon();

        Animation newAnimation = transform.GetChild(selectedWeapon).GetComponent<Animation>();
        yield return PlayWeaponAnimation(newAnimation, draw);

        isSwitching = false;
    }

    IEnumerator PlayWeaponAnimation(Animation weaponAnimation, AnimationClip clip)
    {
        if (weaponAnimation == null || clip == null)
        {
            yield break;
        }

        if (weaponAnimation.GetClip(clip.name) == null)
        {
            weaponAnimation.AddClip(clip, clip.name);
        }

        weaponAnimation[clip.name].wrapMode = WrapMode.Once;
        weaponAnimation.Stop();
        if (weaponAnimation.Play(clip.name))
        {
            while (weaponAnimation.IsPlaying(clip.name))
            {
                yield return null;
            }
        }
    }

    void SelectWeapon()
    {
        if (transform.childCount == 0)
        {
            return;
        }

        if (selectedWeapon >= transform.childCount)
        {
            selectedWeapon = transform.childCount -1;
        }
        
        int i = 0;

        foreach(Transform _weapon in transform)
        {
            if (i == selectedWeapon)
            {
                _weapon.gameObject.SetActive(true);
            }
            else
            {
                _weapon.gameObject.SetActive(false);
            }
            i++;
        }
    }
}
