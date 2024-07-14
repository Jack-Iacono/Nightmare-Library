using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeskController : MonoBehaviour
{
    public static DeskController instance;

    public static List<PlayerController> playersAtDesk = new List<PlayerController>();

    public GameObject offlineIdolPrefab;
    public List<Transform> idolSpawnLocations = new List<Transform>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            playersAtDesk.Add(other.GetComponent<PlayerController>());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            playersAtDesk.Remove(other.GetComponent<PlayerController>());
        }
    }

    public List<IdolController> SpawnIdols(int count, TaskSpawnIdols idolSpawner)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
        {
            List<IdolController> list = new List<IdolController>();
            for (int i = 0; i < count; i++)
            {
                GameObject idol = Instantiate(offlineIdolPrefab, idolSpawnLocations[i].position, Quaternion.identity, transform);
                IdolController temp = idol.GetComponent<IdolController>();
                temp.Initialize(idolSpawner);
                list.Add(temp);
            }
            return list;
        }
        else
        {
            return GetComponent<DeskNetwork>().SpawnIdols(count, idolSpawner);
        }
    }
}
