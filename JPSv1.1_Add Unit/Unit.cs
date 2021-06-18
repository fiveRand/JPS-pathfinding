using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public Transform target;
    public float spd = 5;
    Vector3[] path;

    int targetIndex;

    Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {

            pathQuestManager.RequestPath(transform.position, target.position, OnPathFound);
        }
    }

    public void OnPathFound(Vector3[] newPath,bool pathSuccess)
    {
        if(pathSuccess)
        {
            path = newPath;
            targetIndex = 0;
            StopCoroutine(FollowPath());
            StartCoroutine(FollowPath());
        }
    }

    IEnumerator FollowPath()
    {
        Vector3 curWayPoint = path[0];

        while(true)
        {
            if ((curWayPoint - transform.position).sqrMagnitude <= 0.5)
            {
                targetIndex++;
                if (targetIndex >= path.Length) yield break;

                curWayPoint = path[targetIndex];
            }
            if (transform.position == curWayPoint)
            {

            }
            Vector3 dir = (curWayPoint - transform.position).normalized;

            rb.velocity = dir * spd;

            yield return null;
        }
    }

    public void OnDrawGizmos()
    {
        if(path != null)
        {
            for(int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(path[i], Vector3.one);

                if (i == targetIndex) Gizmos.DrawLine(transform.position, path[i]);
                else Gizmos.DrawLine(path[i - 1], path[i]);
            }
        }
    }
}
