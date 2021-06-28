using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public float spd = 5;
    Vector3[] path;

    int targetIndex;
    SpriteRenderer circleOutline;
    Rigidbody2D rb;
    Coroutine coroutine;
    private void Start()
    {
        circleOutline = this.transform.GetChild(0).GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
    }
    public void MoveTo(Vector3 curWayPoint)
    {
        Vector3 dir = (curWayPoint - transform.position).normalized;

        rb.velocity = dir * spd;
    }


    public void OnPath(Vector3[] newPath)
    {

        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        coroutine = StartCoroutine(TestFollowPath(newPath));
    }

    IEnumerator TestFollowPath(Vector3[] newPath)
    {
        int index = 0;
        Vector3 curWayPoint = newPath[index];
        rb.velocity = Vector2.zero;
        while (true)
        {
            if ((curWayPoint - transform.position).sqrMagnitude <= 0.5)
            {
                index++;
                if (index >= newPath.Length)
                {
                    rb.velocity = Vector2.zero;
                    yield break;

                }

                curWayPoint = newPath[index];
            }
            MoveTo(curWayPoint);
            yield return null;
        }
    }

    public void OnPathFound(Vector3[] newPath,bool pathSuccess)
    {
        if(pathSuccess)
        {
            path = newPath;
            targetIndex = 0;

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = StartCoroutine(FollowPath());
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
                if (targetIndex >= path.Length)
                {
                    rb.velocity = Vector2.zero;
                    yield break;

                }

                curWayPoint = path[targetIndex];
            }
            MoveTo(curWayPoint);

            yield return null;
        }
    }
    public void MouseSelectedVisible(bool visible)
    {
        circleOutline.gameObject.SetActive(visible);
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
