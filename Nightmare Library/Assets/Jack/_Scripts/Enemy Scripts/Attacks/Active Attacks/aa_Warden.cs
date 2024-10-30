using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;
using UnityEngine.AI;

public class aa_Warden : ActiveAttack
{
    EnemyNavNode areaCenter = null;
    protected int sensorCount = 2;
    protected int ringCount = 4;

    protected float diff;

    protected List<WardenSensorController> alertQueue = new List<WardenSensorController>();
    protected int alertQueueMax = 3;

    protected Vector3 lastSeenLocation = Vector3.zero;

    public aa_Warden(Enemy owner) : base(owner)
    {
        diff = wanderRange / ringCount;
    }

    protected override Node SetupTree()
    {
        owner.navAgent = owner.GetComponent<NavMeshAgent>();
        AssignArea();
        GetWanderLocations(areaCenter.position, ringCount);
        PlaceSensors();

        // Establishes the Behavior Tree and its logic
        Node root = new Selector(new List<Node>()
        {
            new Sequence(new List<Node>()
            {
                new CheckPlayerInSight(this, owner.navAgent, 10, 0.8f),
            }),
            new Sequence(new List<Node>()
            {
                new CheckConditionWardenAlert(this),
                new TaskWardenCheckAlert(this, owner),
                new TaskWait(3),
                new TaskClearAlertLocation(this)
            }),
            new TaskWander(this, owner.navAgent)
        });

        return root;
    }

    protected void AssignArea()
    {
        areaCenter = EnemyNavGraph.GetRandomNavPoint();
        //Debug.DrawRay(areaCenter.position, Vector3.up * 100, Color.yellow, 10f);
    }
    protected void PlaceSensors()
    {
        for(int i = 0; i < sensorCount * ringCount; i++)
        {
            int ring = Mathf.FloorToInt(i / sensorCount);
            Vector3 pos = validWanderLocations[ring][Random.Range(0, validWanderLocations[ring].Count)];

            //Debug.DrawRay(pos, Vector3.up * 100, UnityEngine.Color.cyan, 10f);
            SpawnSensor(pos);
        }
    }

    protected void OnSensorAlert(WardenSensorController sensor)
    {
        if(alertQueue.Count < alertQueueMax && !alertQueue.Contains(sensor))
        {
            alertQueue.Add(sensor);
        }
    }

    public bool IsAlertEmpty()
    {
        return alertQueue.Count == 0;   
    }
    public WardenSensorController GetAlertItem()
    {
        return alertQueue[0];
    }
    public void RemoveAlertItem()
    {
        if(alertQueue.Count > 0)
            alertQueue.RemoveAt(0);
    }
    
    protected void SpawnSensor(Vector3 pos)
    {
        GameObject sensor = GameObject.Instantiate(EnemyPrefabHandler.Instance.wardenSensor, pos, Quaternion.identity);
        sensor.name = "Sensor: " + sensor.transform.position;
        sensor.GetComponentInChildren<WardenSensorController>().onSensorAlert += OnSensorAlert;
    }
}
