using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class Players : MonoBehaviour
{
    public GameObject CameraFollowPrefab;
    public GameObject MainCameraPrefab;
    public GameObject PlayerPrefab;
    public GameObject SwordPrefab;

    public Vector3 SpawnPoint1;
    public Vector3 SpawnPoint2;

    private void Start()
    {

        SpawnPlayer();

    }

    private void SpawnPlayer()
    {

        int playersCount = PhotonNetwork.PlayerList.Length;

        if (playersCount <= 1)
        {

            GameObject MainCamera = PhotonNetwork.Instantiate(MainCameraPrefab.name, SpawnPoint1 + Vector3.up * 2, Quaternion.identity);
            GameObject CameraFollow = PhotonNetwork.Instantiate(CameraFollowPrefab.name, SpawnPoint1 + Vector3.up * 2, Quaternion.identity);

            GameObject Player = PhotonNetwork.Instantiate(PlayerPrefab.name, SpawnPoint1, Quaternion.identity);

            Player.GetComponent<StarterAssets.ThirdPersonController>()._mainCamera = MainCamera;

            foreach (Transform child in Player.transform.GetComponentsInChildren<Transform>())
            {
                if (child.tag.Contains("CinemachineTarget"))
                {
                    CameraFollow.GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = child;
                }

                if (child.tag.Contains("ItemSelect"))
                {
                    child.GetComponent<InventoryAsset.ItemSelect>()._mainCamera = MainCamera;
                }

            }

            PhotonNetwork.Instantiate(SwordPrefab.name, Player.transform.forward * 2, Quaternion.identity);

        }
        else
        {

            GameObject MainCamera = PhotonNetwork.Instantiate(MainCameraPrefab.name, SpawnPoint2 + Vector3.up, Quaternion.identity);
            GameObject CameraFollow = PhotonNetwork.Instantiate(CameraFollowPrefab.name, SpawnPoint2 + Vector3.up, Quaternion.identity);

            GameObject Player = PhotonNetwork.Instantiate(PlayerPrefab.name, SpawnPoint2, Quaternion.identity);

            Player.GetComponent<StarterAssets.ThirdPersonController>()._mainCamera = MainCamera;

            foreach (Transform child in Player.transform.GetComponentsInChildren<Transform>())
            {
                if (!child.tag.Contains("CinemachineTarget")) continue;

                CameraFollow.GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = child;
            }

            PhotonNetwork.Instantiate(SwordPrefab.name, Player.transform.forward * 2, Quaternion.identity);

        }

    }

}
