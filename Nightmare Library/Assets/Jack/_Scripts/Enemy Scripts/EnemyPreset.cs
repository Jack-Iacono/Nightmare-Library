using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Enemy;

[Serializable]
[CreateAssetMenu(fileName = "EnemyPreset", menuName = "ScriptableObjects/EnemyPreset", order = 1)]
public class EnemyPreset : ScriptableObject
{
    public string enemyName;
    public string description;

    public enum aAttackEnum { RUSH, STALKER, WARDEN, NULL };
    public enum pAttackEnum { IDOLS, TEMP, SEARCH, NULL };

    public aAttackEnum[] activeAttacks = new aAttackEnum[0];
    public pAttackEnum[] passiveAttacks = new pAttackEnum[0];

    public const int EnemyEvidenceCount = 3;
    public enum EvidenceEnum { HYSTERICS, MUSIC_LOVER, FOOTPRINT, TRAPPER, HALLUCINATOR, LIGHT_FLICKER };
    public EvidenceEnum[] evidence = new EvidenceEnum[3];

    // This is used by other scripts for network data stuff
    public static readonly int EvidenceTypeCount = Enum.GetValues(typeof(EvidenceEnum)).Length;

    public aAttackEnum GetRandomActiveAttack()
    {
        return activeAttacks[UnityEngine.Random.Range(0, activeAttacks.Length)];
    }
    public aAttackEnum GetRandomActiveAttack(aAttackEnum[] exclude)
    {
        if(exclude.Length == 0)
            return GetRandomActiveAttack();

        List<aAttackEnum> temp = new List<aAttackEnum>(activeAttacks);
        for (int i = 0; i < exclude.Length; i++)
        {
            if (temp.Contains(exclude[i]))
                temp.Remove(exclude[i]);
        }

        if (temp.Count > 0)
            return temp[UnityEngine.Random.Range(0, temp.Count)];
        else
        {
            Debug.LogWarning("Enemy Active Attack Assignment Error for " + enemyName + ": No Valid Attacks");
            return aAttackEnum.NULL;
        }
    }

    public pAttackEnum GetRandomPassiveAttack()
    {
        return passiveAttacks[UnityEngine.Random.Range(0, passiveAttacks.Length)];
    }
    public pAttackEnum GetRandomPassiveAttack(pAttackEnum[] exclude)
    {
        if (exclude.Length == 0)
            return GetRandomPassiveAttack();

        List<pAttackEnum> temp = new List<pAttackEnum>(passiveAttacks);
        for (int i = 0; i < exclude.Length; i++)
        {
            if (temp.Contains(exclude[i]))
                temp.Remove(exclude[i]);
        }

        if (temp.Count > 0)
            return temp[UnityEngine.Random.Range(0, temp.Count)];
        else
        {
            Debug.LogWarning("Enemy Passive Attack Assignment Error for " + enemyName + ": No Valid Attacks");
            return pAttackEnum.NULL;
        }
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
        Evidence[] list = new Evidence[EnemyEvidenceCount];

        for(int i = 0; i < EnemyEvidenceCount; i++)
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

    public bool CheckEvidence(bool[] e)
    {
        // Run through all evidence in the evidence sent
        for(int i = 0; i < e.Length; i++)
        {
            // If the evidence is being used and is not present on this enemy, this is not the enemy being looked for
            if (e[i] && !evidence.Contains((EvidenceEnum)i))
            {
                return false;
            }
        }
        return true;
    }

}