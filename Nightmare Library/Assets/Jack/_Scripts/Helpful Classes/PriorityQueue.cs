using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue <T>
{

    private List<Element> heap = new List<Element>{ null };

    public int Count { get { return heap.Count; } }

    public class Element
    {
        public T value;
        public int priority;

        public Element(T value, int priority)
        {
            this.value = value;
            this.priority = priority;
        }

        public static bool operator>(Element a, Element b)
        {
            return a.priority > b.priority;
        }
        public static bool operator<(Element a, Element b)
        {
            return a.priority < b.priority;
        }

        public bool Contains(T a)
        {
            return value.Equals(a);
        }
    }

    public PriorityQueue()
    {
        heap = new List<Element>();
    }
    public PriorityQueue(List<Element> queue)
    {
        heap = queue;

        // Fully Heapify
        for (int i = Count / 2 - 1; i >= 0; i--)
            Heapify(i);
    }

    public void Heapify(int i)
    {
        int smallest = i; // Initialize smallest as root
        int l = Left(smallest); // left
        int r = Right(smallest); // right

        // If left child is smaller than root
        if (l < Count && heap[l] < heap[smallest])
            smallest = l;

        // If right child is smaller than smallest so far
        if (r < Count && heap[r] < heap[smallest])
            smallest = r;

        // If smallest is not root
        if (smallest != i)
        {
            Element temp = heap[i];
            heap[i] = heap[smallest];
            heap[smallest] = temp;

            // Recursively heapify the affected sub-tree
            Heapify(smallest);
        }
    }

    public T Extract()
    {
        // Returns the value from the beginning of the queue

        // Return a null value if the heap is too small
        if (heap.Count == 0)
            return default(T);
        
        var v = heap[0].value;

        heap[0] = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);

        Heapify(0);

        return v;
    }
    public void Insert(Element e)
    {
        heap.Add(null);
        UpdateValue(Count-1, e);
    }

    public void UpdateValue(int i, Element e)
    {
        heap[i] = e;
        int p = Parent(i);

        // This would normally compare priorities, but I overloaded the operator in the class defenition
        while(i > 0 && heap[i] < heap[p])
        {
            Swap(i, p);
            i = p;
            p = Parent(i);
        }

        // For good measure
        Heapify(i);
    }

    private void Swap(int i, int j)
    {
        var temp = heap[i];
        heap[i] = heap[j];
        heap[j] = temp;
    }
    public void PrintHeap()
    {
        string s = "";

        for(int i = 0; i < Count; i++)
        {
            s += heap[i].priority + " | Parent: " + Parent(i) + "\n";
        }

        Debug.Log(s);
    }

    public bool Contains(T value)
    {
        foreach(Element e in heap)
        {
            if (e.Contains(value))
                return true;
        }
        return false;
    }


    #region Heap Get Methods

    public int Left(int i)
    {
        return i * 2 + 1;
    }
    public int Right(int i)
    {
        return i * 2 + 2;
    }
    public int Parent(int i)
    {
        if(i == 0)
            return 0;
        
        if(i % 2 == 0)
            return (i - 2) / 2;
        else
            return (i - 1) / 2;
    }

    #endregion

    #region Helper Functions

    public bool Is_Empty()
    {
        return heap.Count <= 0;
    }
    public int Front()
    {
        return heap[0].priority;
    }

    #endregion
}
