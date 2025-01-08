using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Enemy;

[Serializable]
[CreateAssetMenu(fileName = "EnemyPreset", menuName = "ScriptableObjects/EnemyPreset", order = 1)]
public class EnemyPreset : ScriptableObject
{
    public static List<EnemyPreset> presets = new List<EnemyPreset>();
    public bool includeInGame = false;

    public string enemyName;
    public string description;

    public enum aAttackEnum { RUSH, STALKER, WARDEN, NULL };
    public enum pAttackEnum { IDOLS, TEMP, SEARCH, NULL };

    public aAttackEnum[] activeAttacks = new aAttackEnum[0];
    public pAttackEnum[] passiveAttacks = new pAttackEnum[0];

    public const int EvidenceCount = 3;
    public enum EvidenceEnum { HYSTERICS, MUSIC_LOVER, FOOTPRINT, TRAPPER, HALLUCINATOR, LIGHT_FLICKER };
    public EvidenceEnum[] evidence = new EvidenceEnum[3];

    public aAttackEnum GetRandomActiveAttack()
    {
        return activeAttacks[UnityEngine.Random.Range(0, activeAttacks.Length)];
    }
    public aAttackEnum GetRandomActiveAttack(aAttackEnum[] exclude)
    {
        List<aAttackEnum> temp = new List<aAttackEnum>(activeAttacks);
        for (int i = 0; i < exclude.Length; i++)
        {
            if (temp.Contains(exclude[i]))
                temp.Remove(exclude[i]);
        }

        if (temp.Count > 0)
            return temp[UnityEngine.Random.Range(0, temp.Count)];
        else
            return aAttackEnum.NULL;
    }

    public pAttackEnum GetRandomPassiveAttack()
    {
        return passiveAttacks[UnityEngine.Random.Range(0, passiveAttacks.Length)];
    }
    public pAttackEnum GetRandomPassiveAttack(pAttackEnum[] exclude)
    {
        List<pAttackEnum> temp = new List<pAttackEnum>(passiveAttacks);
        for (int i = 0; i < exclude.Length; i++)
        {
            if (temp.Contains(exclude[i]))
                temp.Remove(exclude[i]);
        }

        if (temp.Count > 0)
            return temp[UnityEngine.Random.Range(0, temp.Count)];
        else
            return pAttackEnum.NULL;
    }

    public ActiveAttack GetActiveAttack(aAttackEnum attack, Enemy e)
    {
        switch (attack)
        {
            case aAttackEnum.RUSH:
                return new aa_Rush(e);
            case aAttackEnum.STALKER:
                return new aa_Stalk(e);
            case aAttackEnum.WARDEN:
                return new aa_Warden(e);
            default:
                return null;
        }
    }
    public PassiveAttack GetPassiveAttack(pAttackEnum attack, Enemy e)
    {
        switch (attack)
        {
            case pAttackEnum.IDOLS:
                return new pa_Idols(e);
            case pAttackEnum.TEMP:
                return new pa_Temps(e);
            case pAttackEnum.SEARCH:
                return new pa_Screech(e);
            default:
                return null;
        }
    }

    /// <summary>
    /// Converts the preset's evidence enums into scripts
    /// </summary>
    /// <param name="e">The enemy that is requesting the evidence</param>
    /// <returns>The list of evidence</returns>
    public Evidence[] GetEvidence(Enemy e)
    {
        Evidence[] list = new Evidence[EvidenceCount];

        for(int i = 0; i < EvidenceCount; i++)
        {
            switch (evidence[i])
            {
                case EvidenceEnum.HYSTERICS:
                    list[i] = new ev_Hysterics(e);
                    break;
                case EvidenceEnum.MUSIC_LOVER:
                    list[i] = new ev_MusicLover(e);
                    break;
                case EvidenceEnum.FOOTPRINT:
                    list[i] = new ev_Footprint(e);
                    e.objPool.PoolObject(PrefabHandler.Instance.e_EvidenceFootprint, 10);
                    break;
                case EvidenceEnum.TRAPPER:
                    list[i] = new ev_Trapper(e);
                    e.objPool.PoolObject(PrefabHandler.Instance.e_EvidenceTrap, 10);
                    break;
                case EvidenceEnum.HALLUCINATOR:
                    list[i] = new ev_Hallucinator(e);
                    break;
                case EvidenceEnum.LIGHT_FLICKER:
                    list[i] = new ev_Flicker(e);
                    break;
            }
        }

        return list;
    }

#if UNITY_EDITOR
    // Sets the static list to include the correct enemy presets
    private void OnValidate()
    {
        if (includeInGame && !presets.Contains(this))
        {
            presets.Add(this);
        }
        else if (!includeInGame && presets.Contains(this))
        {
            presets.Remove(this);
        }
    }
#endif

}
