using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using Unity.VisualScripting;
using TMPro;
using Photon.Pun.UtilityScripts;


public class LoadBoard : MonoBehaviour
{
    public GameObject playerHolder;

    [Header("Options")]
    public float refreshRate = 1f;

    [Header("UI")]
    public GameObject[] slots;
    [Space]
    public TextMeshProUGUI[] scoreText;
    public TextMeshProUGUI[] nameText;

    private void Start()
    {
        InvokeRepeating(nameof(Refresh),time:1f,refreshRate);
    }
    
    public void Refresh()
    {
        foreach (var slot in slots)
        {
            slot.SetActive(false);
        }

        var sortedPlayerList = 
            (from player in PhotonNetwork.PlayerList orderby player.GetScore() descending select player).ToList();

        int i = 0;
        foreach(var player in sortedPlayerList)
        {
            slots[i].SetActive(true);

            if (player.NickName == "")
            {
                player.NickName = "Unnamed";
            }

            nameText[i].text = player.NickName;
            scoreText[i].text = player.GetScore().ToString();

            i++;

        }
    }

    private void Update()
    {
        playerHolder.SetActive(Input.GetKey(KeyCode.Tab));
    }
}
