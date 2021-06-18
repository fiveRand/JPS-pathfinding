using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class pathQuestManager : MonoBehaviour
{
    public JPS jps;
    Queue<PathRequest> requestQueue = new Queue<PathRequest>();

    PathRequest curRequest;

    static pathQuestManager instance;
    bool isProcessingPath;

    private void Awake()
    {
        instance = this;

    }

    public static void RequestPath(Vector3 start, Vector3 end, Action<Vector3[], bool> call)
    {
        PathRequest newRequest = new PathRequest(start, end, call);
        instance.requestQueue.Enqueue(newRequest);
        instance.TryProcessNext();
    }
    void TryProcessNext()
    {
        if(isProcessingPath ==false && requestQueue.Count > 0)
        {
            curRequest = requestQueue.Dequeue();
            isProcessingPath = true;
            jps.StartFindPath(curRequest.pathStart, curRequest.pathEnd);
        }
    }
    
    public void FinishedProcessingPath(Vector3[] path,bool success)
    {
        curRequest.callback(path, success);
        isProcessingPath = false;
        TryProcessNext();
    }

    struct PathRequest
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;

        public Action<Vector3[], bool> callback;

        public PathRequest(Vector3 start, Vector3 end, Action<Vector3[],bool> call)
        {
            pathStart = start;
            pathEnd = end;
            callback = call;
        }
    }
}
