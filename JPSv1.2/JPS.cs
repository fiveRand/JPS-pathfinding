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
    Stack<Node> ClearParentAfterFinishes = new Stack<Node>();
    public bool CountResponseTime = false;
    Stopwatch sw = new Stopwatch();
    private void Start()
    {
        grid = GetComponent<Grid>();
        manager = GetComponent<pathQuestManager>();
    }
    public Vector3[] TestFindPath(Vector3 startPos, Vector3 targetPos)
    {
        if (CountResponseTime)
        {
            sw.Start();
        }

        Node startNode = grid.GetNodeFromVector(startPos); // 1. Create Start Node
        Node endNode = grid.GetNodeFromVector(targetPos);


        Heap<Node> openSet = new Heap<Node>(grid.gridSize);
        List<Node> closedSet = new List<Node>();

        bool pathSuccess = false;
        openSet.Add(startNode);

        if (startNode.notWalkable == false && endNode.notWalkable == false)
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
                        sw.Reset();
                    }

                    pathSuccess = true;
                    break;
                }

                identifySuccessor(curNode,endNode,openSet,closedSet);
            }


        }

        if (pathSuccess)
        {
            Vector3[] wayPoints = backTrace(startNode, endNode);

            for(int i =0; i < ClearParentAfterFinishes.Count; i++)
            {
                Node n = ClearParentAfterFinishes.Pop();
                n.parent = null;
            }
            ClearParentAfterFinishes.Clear();

            return wayPoints;
        }
        else
        {
            UnityEngine.Debug.Log("Failed Finding");
            return null;
        }
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        if (CountResponseTime)
        {
            sw.Start();
        }

        Node startNode = grid.GetNodeFromVector(startPos); // 1. Create Start Node
        Node endNode = grid.GetNodeFromVector(targetPos);


        Heap<Node> openSet = new Heap<Node>(grid.gridSize);
        List<Node> closedSet = new List<Node>();

        bool pathSuccess = false;
        openSet.Add(startNode);



        if (startNode.notWalkable == false && endNode.notWalkable == false)
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
                        sw.Reset();
                    }

                    pathSuccess = true;
                    break;
                }

                identifySuccessor(curNode, endNode, openSet, closedSet);
            }


        }
        else
        {
            UnityEngine.Debug.LogError("Cannot go to Destination");
        }

        if (pathSuccess)
        {
            Vector3[] wayPoints = backTrace(startNode, endNode);

            for (int i = 0; i < ClearParentAfterFinishes.Count; i++)
            {
                Node n = ClearParentAfterFinishes.Pop();
                n.parent = null;
            }
            ClearParentAfterFinishes.Clear();

            manager.FinishedProcessingPath(wayPoints, pathSuccess);
        }
        else
        {
             UnityEngine.Debug.Log("Failed Finding");
        }
    }
    void identifySuccessor(Node curNode,Node endNode, Heap<Node> openSet, List<Node> closedSet)
    {
        List<Node> neighboursList = getNeighbour(curNode); // Error here.

        for (int i =0; i < neighboursList.Count; i++)
        {

            Node nbour = neighboursList[i];
            
            Node jumpNode = Jump(curNode, nbour,endNode);
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
                    else
                    {
                        openSet.UpdateItem(jumpNode);
                    }
                }
            }
            ClearParentAfterFinishes.Push(nbour);
            ClearParentAfterFinishes.Push(jumpNode);
        }
        ClearParentAfterFinishes.Push(curNode);

    }
    List<Node> getNeighbour(Node curNode) // Dispose if thing is done.
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
    Node Jump(Node curNode, Node nbour, Node endNode)
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

            if (Jump(nbour, grid.GetNode(nbour.x + dx, nbour.y), endNode) != null || 
                Jump(nbour, grid.GetNode(nbour.x, nbour.y + dy), endNode) != null )
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
        return Jump(nbour, grid.GetNode(nbour.x + dx, nbour.y + dy), endNode);
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
        foundedPath = path;


        Vector3[] wayPoints = Node2Vector(path);
        Array.Reverse(wayPoints);

        return wayPoints;

    }

    Vector3[] Node2Vector(List<Node> path)
    {
        List<Vector3> wayPoints = new List<Vector3>();

        for(int i = 0; i < path.Count; i++)
        {
            wayPoints.Add(path[i].pos);
        }
        return wayPoints.ToArray();
    }


}
