﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Character
{
    public Character TargetPlayer;

    // Start is called before the first frame update
    void Start()
    {
        HB = GetComponentInChildren<HealthBar>();
    }

    // Update is called once per frame
    void Update()
    {
        ZombieAttack();
        HB.UpdateBar(Health, 100);
        Die();
    }

    void ZombieAttack()
    {
        Debug.Log(InRange());

        if(!TargetPlayer && !TargetBuilding)
        {
            Hunt();
        }

        if (!Attacking && InRange())
        {
            if (TargetPlayer)
            {
                Attacking = true;
                StartCoroutine(AttackPlayer());
            }
            else if (TargetBuilding)
            {
                Attacking = true;
                StartCoroutine(AttackBuilding());
            }
        }
    }

    protected override void Die()
    {
        if (Health <= 0)
        {
            Zombies.M.AllZombies.Remove(this);
            Destroy(gameObject);
        }
    }

    #region Target Finding
    void Hunt()
    {
        Debug.Log("Hunting...");

        if (ClosestPlayer())
        {
            TargetPlayer = ClosestPlayer();
            List<EnvironmentTile> Route = Environment.M.Solve(CurrentPosition, TargetPlayer.CurrentPosition);
            Route.RemoveAt(Route.Count - 1);
            GoTo(Route);
        }
        else
        {
            GoTo(ClosestBuilding());
        }
    }

    Character ClosestPlayer()
    {
        float dist = float.MaxValue;
        Character closest = null;

        foreach (Character player in PlayerBase.M.Players)
        {
            if (!player.Garrisoned)
            {
                if (Vector3.Distance(transform.position, player.transform.position) < dist)
                {
                    closest = player;
                    dist = Vector3.Distance(transform.position, player.transform.position);
                }
            }
        }

        return closest;
    }

    EnvironmentTile ClosestBuilding()
    {
        float dist = float.MaxValue;
        EnvironmentTile closest = null;

        foreach (EnvironmentTile tile in PlayerBase.M.Main.SpawnPoints)
        {
            if(tile)
            {
                if (Vector3.Distance(transform.position, tile.transform.position) < dist)
                {
                    closest = tile;
                    TargetBuilding = PlayerBase.M.Main;
                    dist = Vector3.Distance(transform.position, tile.transform.position);
                }
            }
        }

        return closest;
    }

    bool InRange()
    {
        if (TargetPlayer)
            return Vector3.Distance(transform.position, TargetPlayer.transform.position) < AttackRange;

        if (TargetBuilding)
            return Vector3.Distance(transform.position, ClosestBuilding().transform.position) < AttackRange;

        return false;
    }
    #endregion

    #region Combat
    public IEnumerator AttackPlayer()
    {
        yield return new WaitForSeconds(1.0f / AttackSpeed);


        if (TargetPlayer && !TargetPlayer.Garrisoned)
        {
            FaceTarget(TargetPlayer.transform.position);

            if (TargetPlayer.Health - Damage > 0)
            {
                TargetPlayer.Health -= (int)Damage;
                StartCoroutine(AttackPlayer());
            }
            else
            {
                TargetPlayer.Health -= (int)Damage;
                Attacking = false;
                TargetPlayer = null;
            }
        }
        else
        {
            Attacking = false;
            TargetPlayer = null;
        }
    }

    public IEnumerator AttackBuilding()
    {
        Debug.Log("Waiting");
        yield return new WaitForSeconds(1.0f / AttackSpeed);

        if (TargetBuilding)
        {
            FaceTarget(TargetBuilding.Centre.transform.position);

            if (TargetBuilding.Health - Damage > 0)
            {
                Debug.Log("Attacking");
                TargetBuilding.Health -= (int)Damage;
                StartCoroutine(AttackBuilding());
            }
            else
            {
                TargetBuilding.Health -= (int)Damage;
                Attacking = false;
                TargetBuilding = null;
            }
        }
    }
    #endregion



}
