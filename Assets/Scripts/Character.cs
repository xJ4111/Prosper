using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Stats")]
    public float Health;

    [Header("Interaction")]
    public bool Busy;
    public bool AtBase;

    public EnvironmentTile PriorityTarget;

    [Header("Pathfinding")]
    [SerializeField] private float SingleNodeMoveTime = 0.5f;
    public EnvironmentTile CurrentPosition { get; set; }
    public EnvironmentTile CurrentTarget;
    public bool TargetReached;
    public bool Moving;

    private void Update()
    {
        TargetReachedCheck();
        FinishThenMove();
        EnterBase();
    }

    #region Interaction
    void EnterBase()
    {
        if (PlayerBase.M.RTBCalled == true && !AtBase)
        {
            if (CurrentTarget == PlayerBase.M.DoorTile && TargetReached)
            {
                Debug.Log("Moving");
                transform.position = PlayerBase.M.BaseTile.transform.position;
                AtBase = true;
            }
        }

        if(AtBase)
            transform.position = PlayerBase.M.BaseTile.transform.position;
    }

    public void Interact(Interactable Target)
    {
        Target.TargetingPlayer = this;
        Target.Interacted = true;
        GoTo(Target.Tile);
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
        if (PriorityTarget)
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
