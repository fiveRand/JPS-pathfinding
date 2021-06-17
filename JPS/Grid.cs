using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public GameObject startingObject;
    public GameObject targetObject;

    public JPS jps;
    Node[,] nodeGrid;

    public Vector2Int gridWorldSize;
    public float NodeDiameter;
    public LayerMask wallLayer;

    float nodeRadius { get { return NodeDiameter / 2; } }

    int gridSizeX { get { return Mathf.RoundToInt(gridWorldSize.x / NodeDiameter); } }
    int gridSizeY { get { return Mathf.RoundToInt(gridWorldSize.y / NodeDiameter); } }

    public int gridSize { get { return gridSizeX * gridSizeY; } }
    Vector2 bottomLeftGrid { get { return (Vector2)transform.position - ((Vector2.right) * gridWorldSize.x / 2 + Vector2.up * gridWorldSize.y / 2); } }


    private void Start()
    {
        jps = GetComponent<JPS>();
        nodeGrid = new Node[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 pos = new Vector2((x * NodeDiameter), (y * NodeDiameter)) + bottomLeftGrid;
                bool notWalkable = Physics2D.OverlapCircle(pos, nodeRadius + 0.1f, wallLayer);
                nodeGrid[x, y] = new Node(pos, notWalkable, x, y);
            }
        }
    }

    void GridUpdate()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 pos = new Vector2((x * NodeDiameter), (y * NodeDiameter)) + bottomLeftGrid;
                bool notWalkable = Physics2D.OverlapCircle(pos, nodeRadius + 0.1f, wallLayer);
                nodeGrid[x, y] = new Node(pos, notWalkable, x, y);
            }
        }
    }

    private void Update()
    {
        // GridUpdate();
        jps.JumpPointSearch(startingObject.transform.position, targetObject.transform.position);
    }
    public Node GetNodeFromVector(Vector3 worldPos)
    {

        if (worldPos.x > gridWorldSize.x / 2 || worldPos.x < -gridWorldSize.x / 2) { return null; }
        if (worldPos.y > gridWorldSize.y / 2 || worldPos.y < -gridWorldSize.y / 2) { return null; }

        float GridPercentageOfX = (worldPos.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float GridPercentageOfY = (worldPos.y + gridWorldSize.y / 2) / gridWorldSize.y;

        int x = Mathf.RoundToInt((gridSizeX) * GridPercentageOfX);
        int y = Mathf.RoundToInt((gridSizeY) * GridPercentageOfY);

        return nodeGrid[x, y];
    }

    public Node GetNode(int x,int y)
    {
        if (isInBoundary(x, y))
        {
            return nodeGrid[x, y];
        }
        else
        {
            return null;
        }

    }


    public bool isNotPassable(int x, int y)
    {
        if(isInBoundary(x,y))
        {
            return nodeGrid[x, y].notWalkable;
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

    public void CalculateNode(Node nodeStart, Node nodeEnd)
    {
        int manhattan = Mathf.Abs(nodeEnd.x - nodeStart.x) * 10 + Mathf.Abs(nodeEnd.y - nodeStart.y) * 10;

        nodeStart.hCost = manhattan;

        Node parent = nodeStart.parent;

        nodeStart.gCost = parent.gCost + calculateGscore(nodeStart, parent);
    }
    int calculateGscore(Node newNode, Node oldNode)
    {
        int x = newNode.x - oldNode.x;
        int y = newNode.y - oldNode.y;

        if (x == 0 || y == 0) return Mathf.Abs(10 * Mathf.Max(x, y));
        else return Mathf.Abs(14 * Mathf.Max(x, y));
    }

    public int GetDistanceNodeManHattan(Node nodeStart, Node nodeEnd)
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector2(gridWorldSize.x, gridWorldSize.y));

        if (nodeGrid != null)
        {
            Node startNode = GetNodeFromVector(startingObject.transform.position);
            Node endNode = GetNodeFromVector(targetObject.transform.position);
            foreach (Node n in nodeGrid)
            {
                if (n.notWalkable == true) { Gizmos.color = Color.black; }
                else { Gizmos.color = Color.white; }


                if(jps.foundedPath != null)
                {
                    if (jps.foundedPath.Contains(n)) { Gizmos.color = Color.red ; }
                }

                if (startNode == n) { Gizmos.color = Color.yellow; }
                if (endNode == n) { Gizmos.color = Color.magenta; }

                Gizmos.DrawCube(n.pos, Vector2.one * (nodeRadius));
            }
        }
    }
}
