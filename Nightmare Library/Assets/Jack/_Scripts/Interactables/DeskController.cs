using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeskController : MonoBehaviour
{
    public static DeskController instance;

    public static List<PlayerController> playersAtDesk = new List<PlayerController>();

    public GameObject offlineIdolPrefab;
    public List<GameObject> idolGameObjects = new List<GameObject>();

    private void Awake()
    {
        if (instance != null)
            Destroy(instance);

        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        playersAtDesk = new List<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
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

    public List<IdolController> GetIdolControllers(TaskSpawnIdols idolSpawner)
    {
        List<IdolController> idols = new List<IdolController>();
        foreach(GameObject g in idolGameObjects)
        {
            IdolController iCont = g.GetComponent<IdolController>();
            iCont.Initialize(idolSpawner);
            idols.Add(iCont);
        }
        return idols;
    }

    private void OnDestroy()
    {
        if(instance == this)
            instance = null;
    }
}
