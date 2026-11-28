using System.Collections;
using Photon.Pun;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    public PhotonView playerSetupView;

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
        if ((playerSetupView != null && !playerSetupView.IsMine) || isSwitching || transform.childCount == 0)
        {
            return;
        }

        int previousSelectedWeapon = selectedWeapon;

        for (int i = 0; i < Mathf.Min(transform.childCount, 9); i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
                selectedWeapon = i;
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

        Weapon oldWeapon = transform.GetChild(previousWeapon).GetComponent<Weapon>();
        if (oldWeapon != null)
        {
            oldWeapon.CancelAction();
            yield return oldWeapon.PlayHolster();
        }

        SelectWeapon();

        Weapon newWeapon = transform.GetChild(selectedWeapon).GetComponent<Weapon>();
        if (newWeapon != null) yield return newWeapon.PlayDraw();

        isSwitching = false;
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

        if (playerSetupView != null && playerSetupView.IsMine)
        {
            playerSetupView.RPC("SetTPWeapon", RpcTarget.AllBuffered, selectedWeapon);
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

        Weapon currentWeapon = CurrentWeapon;
        if (currentWeapon != null) currentWeapon.RefreshAmmoUI();
    }

    public Weapon CurrentWeapon => transform.childCount > selectedWeapon
        ? transform.GetChild(selectedWeapon).GetComponent<Weapon>()
        : null;
}
