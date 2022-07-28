using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour
{
    
    public Transform pathHolder;
    public float Speed = 5f;
    public float WaitTime = 2f;
    public float turnSpeed = 90f;
    public Light spotlight;
    public float viewDistance;
    public LayerMask viewMask;
    public float TimeToSpotPlayer = .5f;
    public Action OnGuardHasSpottedPlayer;

    private float playerVisibleTimer;
    private float viewAngle;
    private Transform player;
    private Color originalSpotlightColour;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        viewAngle = spotlight.spotAngle;
        originalSpotlightColour = spotlight.color;

        Vector3[] wayPoints = new Vector3[pathHolder.childCount];
        for(int i = 0; i < wayPoints.Length; i++)
        {
            wayPoints[i] = pathHolder.GetChild(i).position;
        }
        StartCoroutine(FollowPath(wayPoints));
       
    }
    private void Update()
    {
        if (CanSeePlayer())
        {
            playerVisibleTimer += Time.deltaTime;
        }
        else 
        {
            playerVisibleTimer -= Time.deltaTime;
        }
        playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0, TimeToSpotPlayer);
        spotlight.color = Color.Lerp(originalSpotlightColour, Color.red, playerVisibleTimer / TimeToSpotPlayer);

        if (playerVisibleTimer >= TimeToSpotPlayer)
        {
            if (OnGuardHasSpottedPlayer == null)
            {
                OnGuardHasSpottedPlayer?.Invoke();
            }
        }
    }

    private bool CanSeePlayer()
    {
        if (Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);
            if (angleBetweenGuardAndPlayer < viewAngle / 2)
            {
                if (!Physics.Linecast(transform.position, player.position, viewMask))
                {
                    return true;
                }
            }

        }
        return false;
    }

    private IEnumerator FollowPath(Vector3[] waypoints)
    {
        transform.position = waypoints[0];

        int targetWaypointIndex = 1;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex];
        transform.LookAt(targetWaypoint);

        while (true)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, Speed * Time.deltaTime);
            if (transform.position == targetWaypoint)
            {
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                targetWaypoint = waypoints[targetWaypointIndex];
                yield return TurnToFace(targetWaypoint);
            }
            yield return null;
        }
    }
    private IEnumerator TurnToFace(Vector3 lookTarget)
    {
        Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
        float counter = 0;
        while (transform.forward != dirToLookTarget)
        {
            transform.forward = Vector3.Lerp(transform.forward, dirToLookTarget, counter/turnSpeed); //45
            counter += Time.deltaTime; // 45
            yield return null;
        }

    }
    

    void OnDrawGizmos()
    {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;
        foreach(Transform waypoint in pathHolder)
        {
            Gizmos.DrawSphere(waypoint.position, .3f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
        Gizmos.DrawLine(previousPosition, startPosition);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }
    // Start is called before the first frame update

}
