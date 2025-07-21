using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper.AStarPathfinding
{
    public interface INode<T> where T : class, INode<T>
    {
        Vector3 Position { get; }
        float GCost { get; set; }
        float HCost { get; set; }
        float FCost => GCost + HCost;
        T Parent { get; set; }

        HashSet<T> GetNeighbors();
    }

    public static class AStarPathFindingSystem
    {
        public static List<T> FindPath<T>(this T startNode, T targetNode) where T : class, INode<T>
        {
            if (startNode == null || targetNode == null) return null;

            PriorityQueue<T> openSet = new PriorityQueue<T>();
            HashSet<T> closedSet = new HashSet<T>();

            openSet.Enqueue(startNode, 0);

            startNode.GCost = 0;
            startNode.HCost = GetHeuristic(startNode, targetNode);

            while (openSet.Count > 0)
            {
                T currentNode = openSet.Dequeue();
                if (currentNode == targetNode) return RetracePath(startNode, targetNode);

                closedSet.Add(currentNode);

                foreach (T neighbor in currentNode.GetNeighbors())
                {
                    if (closedSet.Contains(neighbor)) continue;

                    float tentativeGCost = currentNode.GCost + 1; // Cost to move to neighbor

                    if (tentativeGCost < neighbor.GCost || !openSet.Contains(neighbor))
                    {
                        neighbor.GCost = tentativeGCost;
                        neighbor.HCost = GetHeuristic(neighbor, targetNode);
                        neighbor.Parent = currentNode;

                        if (!openSet.Contains(neighbor))
                            openSet.Enqueue(neighbor, neighbor.FCost);
                    }
                }
            }

            return null; // No path found
        }

        private static float GetHeuristic<T>(T a, T b) where T : class, INode<T>
        {
            return Mathf.Abs(a.Position.x - b.Position.x) + Mathf.Abs(a.Position.y - b.Position.y) + Mathf.Abs(a.Position.z - b.Position.z);
        }

        private static List<T> RetracePath<T>(T startNode, T endNode) where T : class, INode<T>
        {
            List<T> path = new List<T>();
            T currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }
    }


    public class PriorityQueue<T>
    {
        private List<(T item, float priority)> elements = new List<(T, float)>();

        public int Count => elements.Count;

        public void Enqueue(T item, float priority)
        {
            elements.Add((item, priority));
            elements.Sort((a, b) => a.priority.CompareTo(b.priority));
        }

        public T Dequeue()
        {
            T item = elements[0].item;
            elements.RemoveAt(0);
            return item;
        }

        public bool Contains(T item)
        {
            return elements.Exists(e => EqualityComparer<T>.Default.Equals(e.item, item));
        }
    }
}
