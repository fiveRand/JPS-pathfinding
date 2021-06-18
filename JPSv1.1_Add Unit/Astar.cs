using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;


public class Astar : MonoBehaviour
{
    public List<Node> pathFindedList = new List<Node>();

    public GameObject startingObject;
    public GameObject targetObject;
    Node startNode;
    Node endNode;

    Node[,] grid;

    public Vector2Int gridWorldSize;
    public float NodeDiameter;
    public LayerMask wallLayer;

    Heap<Node> openSet;
    List<Node> closedSet;
    float nodeRadius { get { return NodeDiameter / 2; } }

    int gridSizeX { get { return Mathf.RoundToInt(gridWorldSize.x / NodeDiameter); } }
    int gridSizeY { get { return Mathf.RoundToInt(gridWorldSize.y / NodeDiameter); } }

    int gridSize { get { return gridSizeX * gridSizeY; } }
    Vector2 bottomLeftGrid { get { return (Vector2)transform.position - ((Vector2.right) * gridWorldSize.x / 2 + Vector2.up * gridWorldSize.y / 2); } }

    private void Start()
    {


        grid = new Node[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 pos = new Vector2((x * NodeDiameter), (y * NodeDiameter)) + bottomLeftGrid;
                bool notWalkable = Physics2D.OverlapCircle(pos, nodeRadius + 0.1f, wallLayer);
                grid[x, y] = new Node(pos, notWalkable, x, y);
            }
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            UnityEngine.Debug.Log("A* start");
            AStarPathFind(startingObject.transform.position, targetObject.transform.position);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            UnityEngine.Debug.Log("JPS start");
            JumpPointSearch(startingObject.transform.position, targetObject.transform.position);
        }

        if(Input.GetKeyDown(KeyCode.Z))
        {
            Check();
        }
    }

    void Check()
    {
        for(int x = 0; x < 10; x++)
        {
            for(int y = 0; y < 10; y++)
            {
                if (x > 0 && x < 10 && y > 0 && y < 10)
                {
                    UnityEngine.Debug.Log(x);
                    UnityEngine.Debug.Log(y);
                }
            }
        }
    }

    public void JumpPointSearch(Vector3 startPos, Vector3 targetPos)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        startNode = GetNodeFromVector(startPos); // 1. Create Start Node
        endNode = GetNodeFromVector(targetPos);


         openSet = new Heap<Node>(gridSize);
         closedSet = new List<Node>();

        startNode.gCost = 0;
        startNode.hCost = 0;

        openSet.Add(startNode);

        while (openSet.Count > 0) // 4 - 2. if OpenSet is Empty Stop Searching
        {
            Node curNode = openSet.RemoveFirst(); // 4-1. Get lowest F value Node
            closedSet.Add(curNode);

            if (curNode == endNode)
            {
                sw.Stop();
                UnityEngine.Debug.Log("Path Founded : " + sw.ElapsedMilliseconds + " ms");

                backTrace(curNode);
                return;
            }

            identifySuccessor(curNode);
        }
    }

    void identifySuccessor(Node curNode)
    {
        List<Node> neighboursList = getNeighbour(curNode);
        for (int i = 0; i < neighboursList.Count; i++)
        {
            Node nbour = neighboursList[i];

            Node jumpNode = Jump(curNode, nbour);

            if (jumpNode == null || closedSet.Contains(jumpNode)) continue;

            if (jumpNode != null)
            {
                int jGcost = curNode.gCost + GetDistanceNodeManHattan(curNode, jumpNode);

                if (!openSet.Contains(jumpNode) || jGcost < jumpNode.hCost)
                {

                    jumpNode.gCost = jGcost;
                    jumpNode.hCost = GetDistanceNodeManHattan(jumpNode, endNode);
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
                if (isNotPassable(x, y + dy) == false)
                    Neighbour.Add(GetNode(x, y + dy));
                if (isNotPassable(x + dx, y) == false)
                    Neighbour.Add(GetNode(x + dx, y));
                if (isNotPassable(x + dx, y + dy) == false)
                    Neighbour.Add(GetNode(x + dx, y + dy));

                if (isNotPassable(x - dx, y + dy) == false && isNotPassable(x - dx, y))
                    Neighbour.Add(GetNode(x - dx, y + dy));
                if (isNotPassable(x + dx, y - dy) == false && isNotPassable(x, y - dy))
                    Neighbour.Add(GetNode(x + dx, y - dy));
            }
            else
            {
                if (dx == 0) // Ver
                {
                    if (isNotPassable(x, y + dy) == false)
                        Neighbour.Add(GetNode(x, y + dy));

                    if (isNotPassable(x + 1, y + dy) == false && isNotPassable(x + 1, y))
                        Neighbour.Add(GetNode(x + 1, y + dy));
                    if (isNotPassable(x - 1, y + dy) == false && isNotPassable(x - 1, y))
                        Neighbour.Add(GetNode(x - 1, y + dy));
                }
                else // Hor
                {
                    if (isNotPassable(x + dx, y) == false)
                        Neighbour.Add(GetNode(x + dx, y));

                    if (isNotPassable(x + dx, y + 1) == false && isNotPassable(x, y + 1))
                        Neighbour.Add(GetNode(x + dx, y + 1));
                    if (isNotPassable(x + dx, y - 1) == false && isNotPassable(x, y - 1))
                        Neighbour.Add(GetNode(x + dx, y - 1));
                }
            }
        }
        else
        {


            // Hor, Ver
            if (isNotPassable(x + 1, y) == false)
                Neighbour.Add(GetNode(x + 1, y));
            if (isNotPassable(x - 1, y) == false)
                Neighbour.Add(GetNode(x - 1, y));
            if (isNotPassable(x, y + 1) == false)
                Neighbour.Add(GetNode(x, y + 1));
            if (isNotPassable(x, y - 1) == false)
                Neighbour.Add(GetNode(x, y - 1));

            // Diagonal

            if (isNotPassable(x - 1, y - 1) == false)
                Neighbour.Add(GetNode(x - 1, y - 1));
            if (isNotPassable(x + 1, y + 1) == false)
                Neighbour.Add(GetNode(x + 1, y + 1));
            if (isNotPassable(x - 1, y + 1) == false)
                Neighbour.Add(GetNode(x - 1, y + 1));
            if (isNotPassable(x + 1, y - 1) == false)
                Neighbour.Add(GetNode(x + 1, y - 1));

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
            if (isNotPassable(nbour.x - dx, nbour.y + dy) == false && isNotPassable(nbour.x - dx, nbour.y) ||
                    isNotPassable(nbour.x + dx, nbour.y - dy) == false && isNotPassable(nbour.x, nbour.y - dy))
            {
                return nbour;
            }

            if (Jump(nbour, GetNode(nbour.x + dx, nbour.y)) != null ||
                Jump(nbour, GetNode(nbour.x, nbour.y + dy)) != null)
            {
                return nbour;
            }
        }
        else
        {
            if (dx != 0)
            {
                if (isNotPassable(nbour.x + dx, nbour.y + 1) == false && isNotPassable(nbour.x, nbour.y + 1) ||
                    isNotPassable(nbour.x + dx, nbour.y - 1) == false && isNotPassable(nbour.x, nbour.y - 1))
                {
                    return nbour;
                }

            }
            else
            {
                if (isNotPassable(nbour.x + 1, nbour.y + dy) == false && isNotPassable(nbour.x + 1, nbour.y) ||
                    isNotPassable(nbour.x - 1, nbour.y + dy) == false && isNotPassable(nbour.x - 1, nbour.y))
                {
                    return nbour;
                }
            }
        }
        return Jump(nbour, GetNode(nbour.x + dx, nbour.y + dy));
    }

    int calculateGscore(Node newNode, Node oldNode)
    {
        int x = newNode.x - oldNode.x;
        int y = newNode.y - oldNode.y;

        if (x == 0 || y == 0) return Mathf.Abs(10 * Mathf.Max(x, y));
        else return Mathf.Abs(14 * Mathf.Max(x, y));
    }

    List<Node> jpsPath = new List<Node>();
    List<Node> backTrace(Node node)
    {
        List<Node> path = new List<Node>();
        Node temp = node;

        while (temp != null)
        {
            path.Add(temp);
            temp = temp.parent;
        }
        jpsPath = path;
        return path;

    }

    void AStarPathFind(Vector3 startPos, Vector3 targetPos)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Node startNode = GetNodeFromVector(startPos);
        Node endNode = GetNodeFromVector(targetPos);

        Heap<Node> openSet = new Heap<Node>(gridSize);
        List<Node> closedSet = new List<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node curNode = openSet.RemoveFirst();
            closedSet.Add(curNode);

            if (curNode == endNode)
            {
                sw.Stop();
                // TestRetracePath(startNode, endNode);
                RetracePath(startNode, endNode);
                UnityEngine.Debug.Log("Path Founded : " + sw.ElapsedMilliseconds + " ms");
                return;
            }

            foreach (Node nbour in GetNeighbour(curNode))
            {
                if (nbour.notWalkable || closedSet.Contains(nbour)) continue;

                int nBourGcost = curNode.gCost + GetDistanceNodeManHattan(curNode, nbour);

                if (!openSet.Contains(nbour) || nBourGcost < nbour.gCost)
                {
                    nbour.gCost = nBourGcost;
                    nbour.hCost = GetDistanceNodeManHattan(nbour, endNode);
                    nbour.parent = curNode;

                    if (!openSet.Contains(nbour))
                    {
                        openSet.Add(nbour);
                    }
                }
            }
        }
    }
    public Node GetNode(int x, int y)
    {
        if (isInBoundary(x, y))
        {
            return grid[x, y];
        }
        else
        {
            return null;
        }

    }


    public bool isNotPassable(int x, int y)
    {
        if (isInBoundary(x, y))
        {
            return grid[x, y].notWalkable;
        }
        else
        {
            return true;
        }
    }


    bool isInBoundary(int x, int y)
    {
        if (x >= 0 && y >= 0 && gridSizeX > x && gridSizeY > y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    void RetracePath(Node start, Node target)
    {

        List<Node> path = new List<Node>();

        Node curNode = target;

        while (curNode != start)
        {
            path.Add(curNode);
            curNode = curNode.parent;
        }


        path.Reverse();
        pathFindedList = path;
    }

    Queue<Node> quPath;
    void TestRetracePath(Node start, Node target)
    {
        Queue<Node> quPath = new Queue<Node>();

        Node curNode = target;

        while (curNode != start)
        {
            quPath.Enqueue(curNode);
            curNode = curNode.parent;
        }

        this.quPath = quPath;
    }

    public Node GetNodeFromVector(Vector3 worldPos)
    {

        if (worldPos.x > gridWorldSize.x / 2 || worldPos.x < -gridWorldSize.x / 2) { return null; }
        if (worldPos.y > gridWorldSize.y / 2 || worldPos.y < -gridWorldSize.y / 2) { return null; }

        float GridPercentageOfX = (worldPos.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float GridPercentageOfY = (worldPos.y + gridWorldSize.y / 2) / gridWorldSize.y;

        int x = Mathf.RoundToInt((gridSizeX) * GridPercentageOfX);
        int y = Mathf.RoundToInt((gridSizeY) * GridPercentageOfY);

        return grid[x, y];
    }

    int GetDistanceNodeManHattan(Node nodeStart, Node nodeEnd)
    {

        int X = Mathf.Abs(nodeStart.x - nodeEnd.x);
        int Y = Mathf.Abs(nodeStart.y - nodeEnd.y);
        if (X == 0 || Y == 0) //  Horizontal || Vertical
        {
            return 14 * X + 10 * (Y - X);
        }
        else // Diagonal
        {
            return 14 * Y + 10 * (X - Y);
        }
        /*
        if (X > Y) return 14 * Y + 10 * (X - Y);
        else return 14 * X + 10 * (Y - X);
        */
    }
    List<Node> GetNeighbour(Node n)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = n.x + x;
                int checkY = n.y + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) // if Neighbour in bound. add it
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector2(gridWorldSize.x, gridWorldSize.y));

        if (grid != null)
        {
            Node startNode = GetNodeFromVector(startingObject.transform.position);
            Node endNode = GetNodeFromVector(targetObject.transform.position);
            foreach (Node n in grid)
            {
                if (n.notWalkable == true) { Gizmos.color = Color.black; }
                else { Gizmos.color = Color.white; }



                if (startNode == n) { Gizmos.color = Color.red; }
                if (endNode == n) { Gizmos.color = Color.green; }

                if (jpsPath.Contains(n))
                {
                    Gizmos.color = Color.yellow;
                }

                if (pathFindedList.Contains(n))
                {
                    Gizmos.color = Color.blue;
                }


                Gizmos.DrawCube(n.pos, Vector2.one * (nodeRadius));
            }
        }
    }
}