using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    [Header("Stats")]
    public float Health = 100;
    public float Damage;
    public float AttackRange;
    public float AttackSpeed;
    public float HeadshotChance;
    public float SnapTime;

    protected HealthBar HB;

    [Header("Combat")]
    public bool Searching;
    public bool Attacking;
    public Character AttackTarget;

    [Header("Interaction")]
    public bool Busy;
    public bool Garrisoned;

    public EnvironmentTile PriorityTarget;
    public Building TargetBuilding;

    [Header("Pathfinding")]
    public float SingleNodeMoveTime = 0.5f;
    public EnvironmentTile CurrentPosition { get; set; }
    public EnvironmentTile CurrentTarget;
    public bool TargetReached;
    public bool Moving;

    private void Start()
    {
        UpdateStats();
        HB = GetComponentInChildren<HealthBar>();
    }

    private void Update()
    {
        TargetReachedCheck();
        FinishThenMove();
        EnterBuilding();
        HB.UpdateBar(Health, PlayerBase.M.Stat("hp"));
        Die();

        Combat();
    }

    #region Combat
    void UpdateStats()
    {
        Health = PlayerBase.M.Stat("hp");
        Damage = PlayerBase.M.Stat("dmg");
        AttackRange = PlayerBase.M.Stat("ar");
        AttackSpeed = PlayerBase.M.Stat("as");
        HeadshotChance = PlayerBase.M.Stat("hc");
        SnapTime = PlayerBase.M.Stat("st");
    }

    void Combat()
    {
        if(Zombies.M.AllZombies.Count > 0)
        {
            if (!AttackTarget && !Searching)
            {
                StartCoroutine(NewTarget());
            }

            if (!Attacking && InRange())
            {
                if (AttackTarget)
                {
                    Attacking = true;
                    StartCoroutine(Attack());
                }
            }
        }
    }

    IEnumerator NewTarget()
    {
        Searching = true;
        yield return new WaitForSeconds(SnapTime);
        AttackTarget = ClosestZombie();
        Searching = false;
    }

    public IEnumerator Attack()
    {
        yield return new WaitForSeconds(1.0f / AttackSpeed);

        if (AttackTarget && InRange())
        {
            FaceTarget(AttackTarget.transform.position);

            if (AttackTarget.Health - Damage > 0)
            {
                if(Random.Range(0,101) < HeadshotChance)
                    AttackTarget.Health -= (int)(Damage * 2);
                else
                    AttackTarget.Health -= (int)(Damage);

                StartCoroutine(Attack());
            }
            else
            {
                if (Random.Range(0, 101) < HeadshotChance)
                    AttackTarget.Health -= (int)(Damage * 2);
                else
                    AttackTarget.Health -= (int)(Damage);

                Attacking = false;
                AttackTarget = null;
                Searching = false;
            }
        }
        else
        {
            Attacking = false;
            AttackTarget = null;
            Searching = false;
        }
    }

    Zombie ClosestZombie()
    {
        float dist = float.MaxValue;
        Zombie closest = null;

        foreach (Zombie z in Zombies.M.AllZombies)
        {
            if (Vector3.Distance(transform.position, z.transform.position) < dist)
            {
                closest = z;
                dist = Vector3.Distance(transform.position, z.transform.position);
            }
        }

        return closest;
    }

    bool InRange()
    {
        if (AttackTarget)
            return Vector3.Distance(transform.position, AttackTarget.transform.position) < AttackRange;

        return false;
    }

    protected virtual void Die()
    {
        if (Health <= 0)
        {
            PlayerBase.M.Players.Remove(this);
            Destroy(gameObject);
        }
    }

    protected void FaceTarget(Vector3 target)
    {
        transform.rotation = Quaternion.LookRotation(target - transform.position, Vector3.up);
    }

    #endregion

    #region Interaction
    void EnterBuilding()
    {
        if (TargetBuilding)
        {
            if (CurrentPosition == TargetBuilding.DoorTile)
            {
                transform.position = TargetBuilding.Centre.transform.position;
            }
        }
    }

    public void Interact(Interactable Target)
    {
        Target.TargetingPlayer = this;
        Target.Interacted = true;
        GoTo(Target.TargetTile(CurrentPosition.Position));
        StartCoroutine(Target.Harvest());
    }

    void TargetReachedCheck()
    {
        if (CurrentTarget)
        {
            TargetReached = CurrentPosition == CurrentTarget && Vector3.Distance(transform.position, CurrentTarget.Position) < 0.05f;
        }
            
    }

    void FinishThenMove()
    {
        if (PriorityTarget && !TargetBuilding)
        {
            if (!Busy)
            {
                GoTo(PriorityTarget);
                PriorityTarget = null;
            }
        }
    }
    #endregion

    #region Pathfinding
    public void GoTo(List<EnvironmentTile> route)
    {
        // Clear all coroutines before starting the new route so 
        // that clicks can interupt any current route animation
        StopAllCoroutines();
        CurrentTarget = route[route.Count - 1];
        StartCoroutine(DoGoTo(route));

    }
    public void GoTo(EnvironmentTile target)
    {
        CurrentTarget = target;
        List<EnvironmentTile> route = Environment.M.Solve(CurrentPosition, target);

        // Clear all coroutines before starting the new route so 
        // that clicks can interupt any current route animation
        StopAllCoroutines();
        StartCoroutine(DoGoTo(route));

    }

    private IEnumerator DoGoTo(List<EnvironmentTile> route)
    {
        // Move through each tile in the given route
        if (route != null)
        {
            for (int count = 0; count < route.Count; ++count)
            {
                CurrentPosition = route[count];
                yield return DoMove(transform.position, CurrentPosition.Position);
            }
        }
    }

    private IEnumerator DoMove(Vector3 position, Vector3 destination)
    {
        // Move between the two specified positions over the specified amount of time
        if (position != destination)
        {
            transform.rotation = Quaternion.LookRotation(destination - position, Vector3.up);

            Vector3 p = transform.position;
            float t = 0.0f;

            while (t < SingleNodeMoveTime)
            {
                t += Time.deltaTime;
                p = Vector3.Lerp(position, destination, t / SingleNodeMoveTime);
                transform.position = p;

                Moving = transform.position != CurrentPosition.Position;

                yield return null;
            }
        }
    }
    #endregion
}
