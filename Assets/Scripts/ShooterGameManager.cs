using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ShooterGameManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    void Start()
    {
        var randomViewportPos = new Vector2(
            Random.Range(0.2f, 0.8f),
            Random.Range(0.2f, 0.8f)
        );
        var randomWorldpos = Camera.main.ViewportToWorldPoint(randomViewportPos);
        randomWorldpos = new Vector3(randomWorldpos.x, randomWorldpos.y, 0);
        PhotonNetwork.Instantiate(playerPrefab.name, randomWorldpos, Quaternion.identity);
    }
}
