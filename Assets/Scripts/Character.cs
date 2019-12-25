using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Interaction")]
    public bool Busy;

    [Header("Pathfinding")]
    [SerializeField] private float SingleNodeMoveTime = 0.5f;
    public EnvironmentTile CurrentPosition { get; set; }
    public bool Moving;


    #region Interaction
    public void Interact(Interactable Target)
    {
        Target.TargetingPlayer = this;
        GoTo(Environment.M.Solve(CurrentPosition, Target.Tile));
        StartCoroutine(Target.Harvest());
    }

    #endregion

    #region Pathfinding
    public void GoTo(List<EnvironmentTile> route)
    {
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
