using System.Collections;
using System.Collections.Generic;
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

    public List<GameObject> SpawnIdols(int count, TaskSpawnIdols idolSpawner)
    {
        List<GameObject> list = new List<GameObject>();
        for(int i = 0; i < count; i++)
        {
            GameObject idol = Instantiate(offlineIdolPrefab, idolSpawnLocations[i].position, Quaternion.identity, transform);
            idol.GetComponent<IdolController>().Initialize(idolSpawner);
            list.Add(idol);
        }
        return list;
    }
}
