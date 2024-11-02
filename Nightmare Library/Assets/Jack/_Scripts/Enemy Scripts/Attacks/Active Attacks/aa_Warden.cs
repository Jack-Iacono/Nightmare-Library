using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;
using UnityEngine.AI;

public class aa_Warden : ActiveAttack
{
    public EnemyNavNode areaCenter { get; private set; } = null;
    protected int sensorCount = 2;
    protected int ringCount = 4;

    protected float diff;

    protected List<WardenSensorController> alertQueue = new List<WardenSensorController>();
    protected int alertQueueMax = 3;

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
            // Attack any player that gets within range
            new Sequence(new List<Node>()
            {
                new CheckPlayerInRange(owner, 3),
                new TaskAttackPlayersInRange(owner.navAgent, 3)
            }),
            // Makes the agent wait if a new target is found
            new Sequence(new List<Node>()
            {
                new CheckConditionNewTarget(this, owner),
                new TaskWait(1)
            }),
            // Checks if the player is in sight and removes the alert queue if there is a player
            new Sequence(new List<Node>()
            {
                new CheckPlayerInSight(this, owner.navAgent, 30, 0.2f, areaCenter.position, wanderRange),
                new TaskWardenClearAlertQueue(this)
            }),
            // Moves toward the target and freezes once there
            new Sequence(new List<Node>()
            {
                new CheckConditionHasStaticTarget(this),
                new TaskGotoStaticTarget(this, owner, 10),
                new Selector(new List<Node>()
                {
                    new CheckPlayerInSight(this, owner.navAgent, 40, -0.8f),
                    new TaskWait(3),
                }),
                new TaskRemoveTarget(this)
            }),
            // Checks if this warden has any alerts and
            new Sequence(new List<Node>()
            {
                new CheckConditionWardenAlert(this),
                new TaskWardenCheckAlert(this, owner),
                new TaskWait(3),
                new TaskClearAlertLocation(this)
            }),
            // Wander by default
            new TaskWander(this, owner.navAgent, 5)
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
    public void ClearAlertItems()
    {
        alertQueue.Clear();
    }
    
    protected void SpawnSensor(Vector3 pos)
    {
        GameObject sensor = PrefabHandler.Instance.InstantiatePrefab(PrefabHandler.Instance.e_WardenSensor, pos, Quaternion.identity);
        sensor.name = "Sensor: " + sensor.transform.position;
        sensor.GetComponent<WardenSensorController>().onSensorAlert += OnSensorAlert;
    }
}
