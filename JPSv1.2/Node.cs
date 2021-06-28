using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    public Node parent;

    public bool notWalkable;
    public bool isOpened;
    public bool isClosed;

    public Vector2 pos;

    public int x;
    public int y;

    public int gCost; // cost of target Node distance
    public int hCost; // cost of new Node distance
    public int Fcost { get { return gCost + hCost; } }
    int heapIndex;
    // get these cost by using manhattan or euclidian methods
    // ex) A.x - B.x = X; A.y - B.y =Y;
    // if( X > Y ) Y * 14 + X * 10
    // else X * 14 + Y * 10

    public Node(Vector2 pos, bool notWalkable, int grid_X, int grid_Y)
    {
        this.pos = pos;
        this.notWalkable = notWalkable;
        this.x = grid_X;
        this.y = grid_Y;
    }

    public int HeapIndex
    {
        get { return heapIndex; }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = Fcost.CompareTo(nodeToCompare.Fcost);

        if (compare == 0) { compare = hCost.CompareTo(nodeToCompare.hCost); }
        return -compare;
    }
}

[System.Serializable]
public struct gridNum
{
    public int x;
    public int y;

    public gridNum(int x,int y)
    {
        this.x = x;
        this.y = y;
    }
}
