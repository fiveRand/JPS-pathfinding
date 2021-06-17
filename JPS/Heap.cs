using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Heap<T> where T : IHeapItem<T>
{
    T[] data;
    int curCount;

    public Heap(int maxHeapSize)
    {
        data = new T[maxHeapSize];
    }

    public void Add(T item)
    {
        item.HeapIndex = curCount;
        data[curCount] = item;
        SortUp(item);
        curCount++;
    }

    public T RemoveFirst()
    {
        T firstItem = data[0];
        curCount--;
        data[0] = data[curCount];
        data[0].HeapIndex = 0;
        SortDown(data[0]);
        return firstItem;
    }

    public void UpdateItem(T item) { SortUp(item); }

    public int Count { get { return curCount; } }

    public bool Contains(T item)
    {

        return Equals(data[item.HeapIndex], item);
    }

    void SortDown(T item)
    {
        while(true)
        {
            int childLeftIndex = item.HeapIndex * 2 + 1;
            int childRightIndex = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            if (childLeftIndex < curCount)
            {
                swapIndex = childLeftIndex;

                if (childRightIndex < curCount)
                {
                    if (data[childLeftIndex].CompareTo(data[childRightIndex]) < 0)
                    {
                        swapIndex = childRightIndex;
                    }
                }

                if (item.CompareTo(data[swapIndex]) < 0)
                {
                    Swap(item, data[swapIndex]);
                }
                else return;
            }
            else return;
        }
    }

    void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;

        while(true)
        {
            T parentItem = data[parentIndex];

            if(item.CompareTo(parentItem) > 0)
            {
                Swap(item, parentItem);
            }
            else
            {
                break;
            }

            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    void Swap(T A,T B)
    {
        data[A.HeapIndex] = B;
        data[B.HeapIndex] = A;
        int itemAIndex = A.HeapIndex;
        A.HeapIndex = B.HeapIndex;
        B.HeapIndex = itemAIndex;

    }

}

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex
    { get; set; }

}

