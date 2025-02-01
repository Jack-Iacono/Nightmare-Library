using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.Services.Authentication;

using static EnemyPreset;

public class Enemy : MonoBehaviour
{
    public static List<Enemy> enemyInstances = new List<Enemy>();
    protected static List<EnemyPreset> inUsePresets = new List<EnemyPreset>();
    protected static List<aAttackEnum> inUseActiveAttacks = new List<aAttackEnum>();
    protected static List<pAttackEnum> inUsePassiveAttacks = new List<pAttackEnum>();

    [Header("Enemy Characteristics")]
    [SerializeField]
    public float moveSpeed = 10;
    public float fovRange = 100;
    public float attackRange = 2;

    [NonSerialized]
    public NavMeshAgent navAgent;
    private AudioSource audioSrc;

    [SerializeField]
    protected Vector3 spawnLocation;
    protected Vector3 targetLocation = Vector3.zero;

    public ObjectPool objPool = new ObjectPool();

    [Header("Attack Variables")]
    [NonSerialized]
    public EnemyPreset enemyType;

    public EnemyPreset.aAttackEnum aAttack;
    public EnemyPreset.pAttackEnum pAttack;

    protected ActiveAttack activeAttackTree;
    protected PassiveAttack passiveAttackTree;

    protected Evidence[] evidence = new Evidence[EnemyPreset.EvidenceCount];

    [Space(10)]
    public LayerMask interactionLayers = 1 << 10;

    [Space(10)]
    public AudioClip musicLoverSound;
    public event EventHandler<string> OnPlaySound;
    
    [Space(10)]
    private List<GameObject> footprintList = new List<GameObject>();
    public delegate void FootprintDelegate(Vector3 pos);
    public event FootprintDelegate OnSpawnFootprint;

    public delegate void TrapDelegate(Vector3 pos);
    public event TrapDelegate OnSpawnTrap;

    [Space(10)]
    [SerializeField]
    private MeshRenderer hallucinationMesh;
    [SerializeField]
    private Material hallucinationMaterial;
    public event EventHandler<bool> OnHallucination;

    [Space(10)]
    public float lightFlickerRange = 40f;
    public event EventHandler OnLightFlicker;

    public delegate void OnInitializeDelegate();
    public event OnInitializeDelegate OnInitialize;

    #region Initialization

    private void Awake()
    {
        if(!enemyInstances.Contains(this))
            enemyInstances.Add(this);
    }

    private void Start()
    {
        Initialize();
    }
    public virtual void Initialize()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = moveSpeed;

        audioSrc = GetComponent<AudioSource>();

        GameController.OnGamePause += OnGamePause;

        navAgent.Warp(spawnLocation);

        // Gets the enemy preset that this will follow, does not pick one which is already in use
        List<EnemyPreset> validPresets = new List<EnemyPreset>(GameController.instance.enemyPresets);
        foreach(EnemyPreset preset in inUsePresets)
        {
            validPresets.Remove(preset);
        }
        enemyType = validPresets[UnityEngine.Random.Range(0, validPresets.Count)];
        inUsePresets.Add(enemyType);

        // Chooses a random active and passive attack from the preset
        aAttack = enemyType.GetRandomActiveAttack(inUseActiveAttacks.ToArray());
        pAttack = enemyType.GetRandomPassiveAttack(inUsePassiveAttacks.ToArray());

        // Add these attacks to a list that ensures that other enemies don't use the same attacks, not that this would cause problems tho
        inUseActiveAttacks.Add(aAttack);
        inUsePassiveAttacks.Add(pAttack);

        // Gets the attack script for each chosen attack
        activeAttackTree = enemyType.GetActiveAttack(aAttack, this);
        passiveAttackTree = enemyType.GetPassiveAttack(pAttack, this);

        // Initializes the attacks
        if(activeAttackTree != null)
            activeAttackTree.Initialize();
        if(passiveAttackTree != null)
            passiveAttackTree.Initialize();

        // Gets the evidence from the enemy preset
        evidence = enemyType.GetEvidence(this);

        OnInitialize?.Invoke();
    }

    public virtual void Spawn()
    {
        //Spawn Stuff
        navAgent.Warp(spawnLocation);
    }

    #endregion

    public virtual void PlayerDamageSwing() { }
    public virtual void PlayerDamageShoot() { }

    protected void Update()
    {
        float dt = Time.deltaTime;

        if(activeAttackTree != null )
            activeAttackTree.Update(dt);
        if(passiveAttackTree != null )
            passiveAttackTree.Update(dt);

        foreach (Evidence e in evidence)
        {
            e.UpdateProcess(dt);
        }
    }

    public void Activate(bool b)
    {
        enabled = b;
        if(!navAgent)
            navAgent = GetComponent<NavMeshAgent>();
        navAgent.enabled = b;
    }

    public void PlaySound(string soundName)
    {
        OnPlaySound?.Invoke(this, soundName);

        if(!audioSrc)
            audioSrc = GetComponent<AudioSource>();

        switch (soundName)
        {
            case "musicLover":
                audioSrc.volume = 2;
                audioSrc.PlayOneShot(musicLoverSound);
                break;
        }
    }
    public void SpawnFootprint()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 5);

        if (NetworkConnectionController.IsRunning)
        {
            OnSpawnFootprint?.Invoke(hit.point);
        }
        else
        {
            var print = objPool.GetObject(PrefabHandler.Instance.e_EvidenceFootprint);

            print.GetComponent<FootprintController>().Place(hit.point, Quaternion.identity);
            print.SetActive(true);
        }
    }
    public void SpawnTrap()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 5);

        if (NetworkConnectionController.IsRunning)
        {
            OnSpawnTrap?.Invoke(hit.point);
        }
        else
        {
            GameObject print = objPool.GetObject(PrefabHandler.Instance.e_EvidenceTrap);

            Interactable.interactables[print].Place(hit.point, Quaternion.identity);
            print.SetActive(true);
        }

    }
    public void SetHallucinating(bool b, bool invokeEvent = true)
    {
        if (b)
        {
            hallucinationMesh.material = hallucinationMaterial;
        }
        else
        {
            hallucinationMesh.material = null;
        }

        if (invokeEvent)
            OnHallucination?.Invoke(this, b);
    }
    public void FlickerLights(bool invokeEvent = true)
    {
        Collider[] col = Physics.OverlapSphere(transform.position, lightFlickerRange, interactionLayers);

        if (col.Length > 0)
        {
            for(int i = 0; i < col.Length; i++)
            {
                if(Interactable.interactables[col[i].gameObject].allowEnemyFlicker)
                    Interactable.interactables[col[i].gameObject].EnemyInteractFlicker();
            }
        }

        if (invokeEvent)
            OnLightFlicker?.Invoke(this, EventArgs.Empty);
    }

    public ActiveAttack GetActiveAttack()
    {
        return activeAttackTree;
    }
    public PassiveAttack GetPassiveAttack()
    {
        return passiveAttackTree;
    }

    protected virtual void OnGamePause(object sender, bool e)
    {
        if(navAgent.isOnNavMesh)
            navAgent.isStopped = e;
    }

    public override string ToString()
    {
        string s = $"{enemyType.enemyName}\nActive Attack: {aAttack.ToString()}\nPassive Attack: {pAttack.ToString()}\nEvidence: ";
        foreach(EnemyPreset.EvidenceEnum e in enemyType.evidence)
        {
            s += e.ToString() + ", ";
        }
        return s;
    }

    private void OnDestroy()
    {
        if(activeAttackTree != null)
            activeAttackTree.OnDestroy();
        if(passiveAttackTree != null)
            passiveAttackTree.OnDestroy();

        inUsePresets.Clear();
        inUseActiveAttacks.Clear();
        inUsePassiveAttacks.Clear();

        enemyInstances.Remove(this);
        GameController.OnGamePause -= OnGamePause;
    }
}
