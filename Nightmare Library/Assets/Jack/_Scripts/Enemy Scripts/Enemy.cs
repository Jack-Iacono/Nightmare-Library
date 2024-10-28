using System;
using UnityEngine;
using UnityEngine.AI;

using BehaviorTree;
using UnityEditor.Timeline;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
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

    public enum aAttackEnum { RUSH, STALKER, WARDEN, NULL };
    public enum pAttackEnum { IDOLS, TEMP, NULL };

    [Header("Attack Variables")]
    [SerializeField]
    public aAttackEnum aAttack;
    [SerializeField]
    public pAttackEnum pAttack;

    protected ActiveAttack activeAttackTree;
    protected PassiveAttack passiveAttackTree;

    public enum EvidenceEnum { HYSTERICS, MUSIC_LOVER, FOOTPRINT, TRAPPER, HALLUCINATOR, LIGHT_FLICKER };
    [Header("Evidence Variables")]

    [SerializeField]
    public List<EvidenceEnum> evidenceList = new List<EvidenceEnum>();
    protected List<Evidence> evidence = new List<Evidence>();

    [Space(10)]
    public LayerMask interactionLayers = 1 << 10;

    [Space(10)]
    public AudioClip musicLoverSound;
    public event EventHandler<string> OnPlaySound;
    
    [Space(10)]
    public GameObject footprintPrefab;
    public GameObject footprintPrefabOnline;
    private List<GameObject> footprintList = new List<GameObject>();
    public delegate void FootprintDelegate(Vector3 pos);
    public event FootprintDelegate OnSpawnFootprint;

    [Space(10)]
    public GameObject trapPrefab;
    public GameObject trapPrefabOnline;
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

    #region Initialization

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

        // Assigning Attacks
        switch (aAttack)
        {
            case aAttackEnum.RUSH:
                activeAttackTree = new aa_Rush(this);
                break;
            case aAttackEnum.STALKER:
                activeAttackTree = new aa_Stalk(this);
                break;
            case aAttackEnum.WARDEN:
                activeAttackTree = new aa_Warden(this);
                break;
        }

        switch (pAttack)
        {
            case pAttackEnum.IDOLS:
                passiveAttackTree = new pa_Idols(this);
                break;
            case pAttackEnum.TEMP:
                passiveAttackTree = new pa_Temps(this);
                break;
        }

        if(activeAttackTree != null)
            activeAttackTree.Initialize();
        if(passiveAttackTree != null)
            passiveAttackTree.Initialize();

        for(int i = 0; i < evidenceList.Count; i++)
        {
            switch(evidenceList[i])
            {
                case EvidenceEnum.HYSTERICS:
                    evidence.Add(new ev_Hysterics(this));
                    break;
                case EvidenceEnum.MUSIC_LOVER:
                    evidence.Add(new ev_MusicLover(this));
                    break;
                case EvidenceEnum.FOOTPRINT:
                    evidence.Add(new ev_Footprint(this));
                    if (!NetworkConnectionController.IsRunning)
                        objPool.PoolObject(footprintPrefab, 10);
                    break;
                case EvidenceEnum.TRAPPER:
                    evidence.Add(new ev_Trapper(this));
                    if (!NetworkConnectionController.IsRunning)
                        objPool.PoolObject(trapPrefab, 10);
                    break;
                case EvidenceEnum.HALLUCINATOR:
                    evidence.Add(new ev_Hallucinator(this));
                    break;
                case EvidenceEnum.LIGHT_FLICKER:
                    evidence.Add(new ev_Flicker(this));
                    break;
            }
        }
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
            activeAttackTree.UpdateTree(dt);
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
            var print = objPool.GetObject(footprintPrefab);

            print.GetComponent<FootprintController>().Activate();
            print.transform.position = hit.point;
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
            var print = objPool.GetObject(trapPrefab);

            print.GetComponent<TrapController>().Activate();
            print.transform.position = hit.point;
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

    protected virtual void OnGamePause(object sender, bool e)
    {
        if(navAgent.isOnNavMesh)
            navAgent.isStopped = e;
    }

    private void OnDestroy()
    {
        if(activeAttackTree != null)
            activeAttackTree.OnDestroy();
        if(passiveAttackTree != null)
            passiveAttackTree.OnDestroy();

        GameController.OnGamePause -= OnGamePause;
    }

    private void OnDrawGizmos()
    {
        
    }
}
