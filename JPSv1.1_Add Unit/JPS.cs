using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class JPS : MonoBehaviour
{
    public Grid grid;
    public pathQuestManager manager;
    public List<Node> foundedPath;

    Heap<Node> openSet;
    List<Node> closedSet;
    Node startNode;
    Node endNode;

    public bool CountResponseTime = false;
    Stopwatch sw = new Stopwatch();
    private void Start()
    {
        grid = GetComponent<Grid>();
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(JumpPointSearch(startPos, targetPos));
    }

    IEnumerator JumpPointSearch(Vector3 startPos, Vector3 targetPos)
    {
        if(CountResponseTime)
        {
            sw.Start();
        }

        startNode = grid.GetNodeFromVector(startPos); // 1. Create Start Node
        endNode = grid.GetNodeFromVector(targetPos);


        openSet = new Heap<Node>(grid.gridSize);
        closedSet = new List<Node>();

        Vector3[] wayPoints = new Vector3[0];
        bool pathSuccess = false;

        openSet.Add(startNode);
        if(startNode.notWalkable == false && endNode.notWalkable == false)
        {
            while (openSet.Count > 0) // 4 - 2. if OpenSet is Empty Stop Searching
            {
                Node curNode = openSet.RemoveFirst(); // 4-1. Get lowest F value Node
                closedSet.Add(curNode);

                if (curNode == endNode)
                {
                    if (CountResponseTime)
                    {
                        sw.Stop();
                        UnityEngine.Debug.Log("Path Founded : " + sw.ElapsedMilliseconds + " ms");
                    }

                    pathSuccess = true;
                    break;
                }

                identifySuccessor(curNode);
            }


            yield return null;

            if (pathSuccess)
            {
                wayPoints = backTrace(startNode,endNode);
            }
            manager.FinishedProcessingPath(wayPoints, pathSuccess);
        }
    }

    void identifySuccessor(Node curNode)
    {
        List<Node> neighboursList = getNeighbour(curNode);
        for(int i =0; i < neighboursList.Count; i++)
        {
            Node nbour = neighboursList[i];

            Node jumpNode = Jump(curNode, nbour);

            if (jumpNode == null || closedSet.Contains(jumpNode)) continue;

            if (jumpNode != null)
            {
                int jGcost = curNode.gCost + grid.GetDistanceNodeManHattan(curNode, jumpNode);

                if (!openSet.Contains(jumpNode) || jGcost < jumpNode.hCost)
                {

                    jumpNode.gCost = jGcost;
                    jumpNode.hCost = grid.GetDistanceNodeManHattan(jumpNode, endNode);
                    jumpNode.parent = curNode;

                    if (!openSet.Contains(jumpNode))
                    {

                        openSet.Add(jumpNode);
                    }
                }
            }
        }

    }
    List<Node> getNeighbour(Node curNode)
    {

        List<Node> Neighbour = new List<Node>();
        Node parent = curNode.parent;

        int x = curNode.x;
        int y = curNode.y;

        if (parent != null)
        {
            int dx = (x - parent.x) / Mathf.Max(Mathf.Abs(x - parent.x), 1);
            int dy = (y - parent.y) / Mathf.Max(Mathf.Abs(y - parent.y), 1);

            if (dx != 0 && dy != 0)
            {
                if (grid.isNotPassable(x, y + dy) == false)
                    Neighbour.Add(grid.GetNode(x, y + dy));
                if(grid.isNotPassable(x+dx,y) == false)
                    Neighbour.Add(grid.GetNode(x + dx, y));
                if(grid.isNotPassable(x+dx,y+dy) == false)
                    Neighbour.Add(grid.GetNode(x + dx, y + dy));

                if (grid.isNotPassable(x-dx,y+dy) == false && grid.isNotPassable(x-dx,y))
                    Neighbour.Add(grid.GetNode(x - dx, y + dy));
                if(grid.isNotPassable(x+dx,y-dy) == false && grid.isNotPassable(x,y-dy))
                    Neighbour.Add(grid.GetNode(x+dx, y -dy));
            }
            else
            {
                if(dx == 0) // Ver
                {
                    if(grid.isNotPassable(x,y+dy) ==false)
                        Neighbour.Add(grid.GetNode(x, y + dy));

                    if (grid.isNotPassable(x+1,y+dy) == false && grid.isNotPassable(x + 1, y))
                        Neighbour.Add(grid.GetNode(x + 1, y +dy));
                    if(grid.isNotPassable(x - 1, y + dy) == false && grid.isNotPassable(x-1,y))
                        Neighbour.Add(grid.GetNode(x-1,y+dy));
                }
                else // Hor
                {
                    if(grid.isNotPassable(x+dx,y) == false)
                        Neighbour.Add(grid.GetNode(x+dx,y));

                    if(grid.isNotPassable(x+dx,y+1) == false && grid.isNotPassable(x,y+1))
                        Neighbour.Add(grid.GetNode(x+dx,y+1));
                    if(grid.isNotPassable(x + dx, y - 1) == false && grid.isNotPassable(x,y-1))
                        Neighbour.Add(grid.GetNode(x+dx,y-1));
                }
            }
        }
        else
        {


            // Hor, Ver
            if (grid.isNotPassable(x + 1,y) == false)
                Neighbour.Add(grid.GetNode(x + 1, y));
            if (grid.isNotPassable(x-1,y) == false)
                Neighbour.Add(grid.GetNode(x - 1, y));
            if (grid.isNotPassable(x,y+1) ==false)
                Neighbour.Add(grid.GetNode(x, y + 1));
            if (grid.isNotPassable(x,y - 1) == false)
                Neighbour.Add(grid.GetNode(x, y - 1));

            // Diagonal

            if (grid.isNotPassable(x - 1, y - 1) ==false)
                Neighbour.Add(grid.GetNode(x - 1, y - 1));
            if (grid.isNotPassable(x + 1, y + 1) == false)
                Neighbour.Add(grid.GetNode(x+1,y+1));
            if(grid.isNotPassable(x-1,y+1) == false)
                Neighbour.Add(grid.GetNode(x-1,y+1));
            if(grid.isNotPassable(x+1,y-1) == false)
                Neighbour.Add(grid.GetNode(x+1,y-1));

        }

        return Neighbour;
    }
    Node Jump(Node curNode, Node nbour)
    {
        if (nbour == null || nbour.notWalkable) return null;
        if (endNode == nbour) return nbour;

        int dx = nbour.x - curNode.x;
        int dy = nbour.y - curNode.y;


        if (dx != 0 && dy != 0)
        {
            if (grid.isNotPassable(nbour.x - dx, nbour.y + dy) == false && grid.isNotPassable(nbour.x - dx, nbour.y) ||
                    grid.isNotPassable(nbour.x + dx, nbour.y - dy) == false && grid.isNotPassable(nbour.x, nbour.y - dy))
            {
                return nbour;
            }

            if (Jump(nbour, grid.GetNode(nbour.x + dx, nbour.y)) != null || 
                Jump(nbour, grid.GetNode(nbour.x, nbour.y + dy))!= null )
            {
                return nbour;
            }
        }
        else
        {
            if(dx != 0)
            {
                if (grid.isNotPassable(nbour.x + dx, nbour.y + 1) == false && grid.isNotPassable(nbour.x, nbour.y + 1) ||
                    grid.isNotPassable(nbour.x + dx, nbour.y - 1) == false && grid.isNotPassable(nbour.x, nbour.y - 1))
                {
                    return nbour;
                }

            }
            else
            {
                if (grid.isNotPassable(nbour.x + 1, nbour.y + dy) == false && grid.isNotPassable(nbour.x + 1, nbour.y) ||
                    grid.isNotPassable(nbour.x -1, nbour.y + dy) == false && grid.isNotPassable(nbour.x - 1, nbour.y ))
                {
                    return nbour;
                }
            }
        }
        return Jump(nbour, grid.GetNode(nbour.x + dx, nbour.y + dy));
    }
    Vector3[] backTrace(Node startNode,Node endNode)
    {
        List<Node> path = new List<Node>();
        Node temp = endNode;

        while (temp != startNode)
        {
            path.Add(temp);
            temp = temp.parent;
        }

        Vector3[] wayPoints = Node2Vector(path);
        
        foundedPath = path;
        Array.Reverse(wayPoints);
        return wayPoints;

    }

    Vector3[] Node2Vector(List<Node> path)
    {
        List<Vector3> wayPoints = new List<Vector3>();
        Vector2 dirOld = Vector2.zero;

        for(int i = 1; i < path.Count; i++)
        {
            Vector2 dirNew = new Vector2(path[i-1].x - path[i].x,path[i-1].y - path[i].y);

            if(dirNew != dirOld)
            {
                wayPoints.Add(path[i].pos);
            }
            dirOld = dirNew;
        }

        return wayPoints.ToArray();
    }


}
