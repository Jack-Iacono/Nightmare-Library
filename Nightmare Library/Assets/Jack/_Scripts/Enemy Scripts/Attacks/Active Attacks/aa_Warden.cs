using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class aa_Warden : ActiveAttack
{
    public EnemyNavNode areaCenter { get; private set; } = null;
    protected int baseSensorCount = 2;
    protected int sensorCount = 2;

    protected int baseRingCount = 4;
    protected int ringCount = 4;

    protected float diff;

    private List<GameObject> spawnedSensors = new List<GameObject>();
    protected List<Vector3> alertQueue = new List<Vector3>();
    protected int alertQueueMax = 3;

    protected float basePlayerSightWaitMin = 1;
    protected float basePlayerSightWaitMax = 1;

    protected float baseCheckTargetSpeed = 10;
    protected float baseCheckTargetAccel = 400;

    protected TaskWait n_PlayerSightWait;
    protected TaskGotoStaticTarget n_GoToSeenTarget;

    public aa_Warden(Enemy owner) : base(owner)
    {
        name = "Warden";
        toolTip = "He guards something, just not sure what";

        diff = wanderRange / ringCount;
        hearingRadius = 1000;
    }

    public override void Initialize(int level = 1)
    {
        base.Initialize(level);

        // Use this to level up later. Controls the size of the area, ring count and trap quantity
        wanderRange = baseWanderRange * Mathf.FloorToInt(currentLevel / 2);
        ringCount = baseRingCount * Mathf.FloorToInt(currentLevel / 2);
        sensorCount = baseSensorCount * Mathf.FloorToInt(currentLevel / 2);

        owner.navAgent = owner.GetComponent<NavMeshAgent>();
        AssignArea();
        GetWanderLocations(areaCenter.position, ringCount);
        PlaceSensors();

        n_PlayerSightWait = new TaskWait(basePlayerSightWaitMin, basePlayerSightWaitMax);
        n_GoToSeenTarget = new TaskGotoStaticTarget(this, owner, baseCheckTargetSpeed, baseCheckTargetAccel);

        // Establishes the Behavior Tree and its logic
        Node root = new Selector(new List<Node>()
        {
            // Attack any player that gets within range
            new Sequence(new List<Node>()
            {
                new CheckPlayerInRange(owner, 3),
                new TaskAttackPlayersInRange(owner, 3)
            }),
            // Makes the agent wait if a new target is found
            new Sequence(new List<Node>()
            {
                new CheckConditionNewTarget(this, owner),
                n_PlayerSightWait
            }),
            // Checks if the player is in sight and removes the alert queue if there is a player
            new Sequence(new List<Node>()
            {
                new CheckPlayerInSight(this, owner.navAgent, 30, 0.2f, areaCenter.position, wanderRange),
                new TaskWardenClearAlertQueue(this),
                n_GoToSeenTarget
            }),
            // Moves toward the target and freezes once there
            new Sequence(new List<Node>()
            {
                new CheckConditionHasStaticTarget(this),
                n_GoToSeenTarget,
                new Selector(new List<Node>()
                {
                    new CheckPlayerInSight(this, owner.navAgent, 40, -0.8f),
                    new TaskWait(2),
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

        tree.SetupTree(root);
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

    public override bool DetectSound(AudioSourceController.SourceData data)
    {
        // Bypasses the typical hearing range check
        // Check that the sound is within the warden's area of partol
        if (Vector3.Distance(data.transform.position, areaCenter.position) < wanderRange)
        {
            alertQueue.Clear();
            alertQueue.Add(data.transform.position);
            return true;
        }

        return false;
    }
    protected void OnSensorAlert(WardenSensorController sensor)
    {
        if(alertQueue.Count < alertQueueMax && !alertQueue.Contains(sensor.trans.position))
        {
            alertQueue.Add(sensor.trans.position);
        }
    }

    public bool IsAlertEmpty()
    {
        return alertQueue.Count == 0;   
    }
    public Vector3 GetAlertItem()
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

        spawnedSensors.Add(sensor);
    }

    protected override void OnLevelChange(int level)
    {
        base.OnLevelChange(level);

        n_PlayerSightWait.OnLevelChange(basePlayerSightWaitMin, basePlayerSightWaitMax);
        n_GoToSeenTarget.OnLevelChange(baseCheckTargetSpeed, baseCheckTargetAccel);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        foreach(GameObject g in spawnedSensors)
        {
            if(PrefabHandler.Instance != null)
            {
                PrefabHandler.Instance.CleanupGameObject(g);
                PrefabHandler.Instance.DestroyGameObject(g);
            }
        }
    }
}
