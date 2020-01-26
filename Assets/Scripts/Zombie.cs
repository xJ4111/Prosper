using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Character
{
    public Character TargetPlayer;
    private float MaxHealth = 100;

    // Start is called before the first frame update
    void Start()
    {
        HB = GetComponentInChildren<HealthBar>();
        Variant();
    }

    // Update is called once per frame
    void Update()
    {
        ZombieAttack();
        HB.UpdateBar(Health, MaxHealth);
        Die();
    }

    void ZombieAttack()
    {
        if(!TargetBuilding)
        {
            Hunt();
        }

        if(!Attacking && InRange())
        {
            Attacking = true;
            StartCoroutine(AttackBuilding());
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
        GoTo(ClosestBuilding());
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
        if (TargetBuilding)
            return Vector3.Distance(transform.position, ClosestBuilding().transform.position) < AttackRange;

        return false;
    }
    #endregion

    #region Combat
    public IEnumerator AttackBuilding()
    {
        yield return new WaitForSeconds(1.0f / AttackSpeed);

        if (TargetBuilding)
        {
            FaceTarget(TargetBuilding.Centre.transform.position);

            if (TargetBuilding.Health - Damage > 0)
            {
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

    void Variant()
    {
        float Multiplier = Random.Range(0.75f, 1.2f);
        SingleNodeMoveTime *= Multiplier * 2;
        MaxHealth *= Multiplier * 4;
        Health = MaxHealth;

        transform.localScale = new Vector3(Multiplier, Multiplier, Multiplier);
    }
    #endregion



}
