using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AudioManager;

public class aa_Warden : ActiveAttack
{
    public EnemyNavNode areaCenter { get; private set; } = null;
    protected int baseSensorCount = 2;
    protected int sensorCount = 2;

    protected int baseRingCount = 4;
    protected int ringCount = 4;

    protected float diff;

    private List<GameObject> spawnedSensors = new List<GameObject>();
    protected AlertNodeVariables alertNodeVariables = new AlertNodeVariables(3);

    protected float basePlayerSightWaitMin = 0.2f;
    protected float basePlayerSightWaitMax = 0.6f;

    protected float baseCheckTargetSpeed = 15;
    protected float baseCheckTargetAccel = 400;

    protected Node_Wait n_PlayerSightWait;
    protected Node_GotoTargetStatic n_GoToSeenTarget;

    public aa_Warden(Enemy owner) : base(owner)
    {
        name = "Warden";
        toolTip = "He guards something, just not sure what";
        ignoreSounds = new SoundType[] { SoundType.e_STALK_CLOSE_IN };

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

        n_PlayerSightWait = new Node_Wait(basePlayerSightWaitMin, basePlayerSightWaitMax);
        n_GoToSeenTarget = new Node_GotoTargetStatic(this, owner, baseCheckTargetSpeed, baseCheckTargetAccel);

        Node root = new Selector(new List<Node>()
        {
            // Attack any player that gets within range
            new Sequence(new List<Node>()
            {
                new Node_CheckInPlayerRange(owner, 3),
                new Node_AttackInRange(owner, 3)
            }),
            // Moves toward the target and freezes once there
            new Sequence(new List<Node>()
            {
                // Will return true if the player is seen or if the last seen location has not been checked yet
                new Selector(new List<Node>()
                {
                    new Node_CheckPlayerInSight(this, owner.navAgent, 30, 0.2f, areaCenter.position, wanderRange),
                    new Node_CheckHasStaticTarget(this),
                }),
                n_PlayerSightWait,
                new Node_AlertClear(alertNodeVariables),
                n_GoToSeenTarget,
                // Does a final sweep for the player once at the location
                new Selector(new List<Node>()
                {
                    new Node_CheckPlayerInSight(this, owner.navAgent, 40, -0.8f),
                    new Node_Wait(2),
                }),
                new Node_RemoveTarget(this)
            }),
            // Checks if this warden has any alerts and goes to that alert
            new Sequence(new List<Node>()
            {
                new Node_AlertCheck(alertNodeVariables),
                new Node_AlertGoto(alertNodeVariables, owner),
                new Node_Wait(3),
                new Node_AlertRemoveFirst(alertNodeVariables)
            }),
            // Wander by default
            new Node_Wander(this, owner.navAgent, 5)
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
        if (!DeskController.PointInOffice(data.transform.position) && Vector3.Distance(data.transform.position, areaCenter.position) < wanderRange)
        {
            alertNodeVariables.queue.Clear();
            alertNodeVariables.queue.Add(data.transform.position);
            return true;
        }

        return false;
    }
    protected void OnSensorAlert(WardenSensorController sensor)
    {
        alertNodeVariables.AddQueueItem(sensor.trans.position);
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
