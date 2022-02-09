using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerprefab;

    public float minX;
    public float maxX;

    public float minY;
    public float maxY;

    private void Start()
    {
        if (GameObject.FindGameObjectWithTag("Player") == null)
        {
            Vector2 randomPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
            PhotonNetwork.Instantiate(playerprefab.name, randomPosition, Quaternion.identity);
        }
    }
    public void Update()
    {
        Debug.Log("Number Of Player"+GameObject.FindGameObjectsWithTag("Player").Length);
    }

}
