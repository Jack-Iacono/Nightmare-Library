using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public class Action_AttackInRange : Action_Attack
{
    private bool hasAttacked = false;

    private Transform transform;
    private NavMeshAgent agent;
    private float range = 5;

    // Checks the player, static world
    private LayerMask attackLayers = 1 << 6 | 1 << 9;

    public Action_AttackInRange(Enemy enemy, float range = 5) : base(enemy)
    {
        this.agent = enemy.navAgent;
        this.range = range;
        transform = agent.transform;
    }
    public override Status Check(float dt)
    {
        if (!hasAttacked)
        {
            agent.speed = 0;

            foreach (PlayerController p in PlayerController.playerInstances.Values)
            {
                if(Vector3.Distance(p.transform.position, transform.position) <= range)
                {
                    Ray ray = new Ray(transform.position, p.transform.position - transform.position);
                    RaycastHit hit;

                    if(Physics.Raycast(ray, out hit, range, attackLayers))
                    {
                        if(hit.collider.gameObject == p.gameObject)
                        {
                            if (p.isAlive)
                            {
                                Attack(p);
                            }
                            
                            hasAttacked = true;
                        }
                    }
                }
            }
        }

        status = Status.SUCCESS;
        return status;
    }
    protected override void OnResetNode()
    {
        base.OnResetNode();
        hasAttacked = false;
    }
}
