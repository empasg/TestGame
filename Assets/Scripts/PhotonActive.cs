using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class PhotonActive : MonoBehaviour
{

    void Start()
    {
        
        if (GetComponent<PhotonView>().IsMine)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
            
    }


}
