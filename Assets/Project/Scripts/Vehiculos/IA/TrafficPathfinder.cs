using System.Collections.Generic;
using UnityEngine;

public class TrafficPathfinder
{
    public static bool FindPath(TrafficNode start, TrafficNode destination, List<TrafficNode> result)
    {
        result.Clear();

        if (start == null || destination == null) return false;

        List<TrafficNode> openNodes = new List<TrafficNode> { start };
        HashSet<TrafficNode> closedNodes = new HashSet<TrafficNode>();
        Dictionary<TrafficNode, TrafficNode> previousNodes = new Dictionary<TrafficNode, TrafficNode>();
        Dictionary<TrafficNode, float> costs = new Dictionary<TrafficNode, float> { [start] = 0f };

        while (openNodes.Count > 0)
        {
            TrafficNode current = GetBestNode(openNodes, costs, destination);

            if (current == destination)
            {
                BuildPath(current, previousNodes, result);
                return true;
            }

            openNodes.Remove(current);
            closedNodes.Add(current);

            foreach (TrafficNode next in current.NextNodes)
            {
                if (next == null || !next.isActiveAndEnabled || closedNodes.Contains(next)) continue;

                float laneChangeCost = current.IsLaneChange(next) ? 2f : 0f;
                float newCost = costs[current] + GetConnectionLength(current, next) + laneChangeCost;

                if (costs.TryGetValue(next, out float currentCost) && newCost >= currentCost) continue;

                costs[next] = newCost;
                previousNodes[next] = current;

                if (!openNodes.Contains(next))
                    openNodes.Add(next);
            }
        }

        return false;
    }

    private static TrafficNode GetBestNode(List<TrafficNode> nodes, Dictionary<TrafficNode, float> costs, TrafficNode destination)
    {
        TrafficNode bestNode = nodes[0];
        float bestScore = float.PositiveInfinity;

        foreach (TrafficNode node in nodes)
        {
            float remainingDistance = Vector2.Distance(node.Position, destination.Position);
            float score = costs[node] + remainingDistance;

            if (score >= bestScore) continue;

            bestScore = score;
            bestNode = node;
        }

        return bestNode;
    }

    private static float GetConnectionLength(TrafficNode start, TrafficNode end)
    {
        float length = 0f;
        Vector2 previous = start.Position;

        // tengo en cuenta el largo de las curvas.
        for (int i = 1; i <= 16; i++)
        {
            Vector2 point = start.GetConnectionPoint(end, i / 16f);
            length += Vector2.Distance(previous, point);
            previous = point;
        }

        return length;
    }

    private static void BuildPath(TrafficNode current, Dictionary<TrafficNode, TrafficNode> previousNodes, List<TrafficNode> result)
    {
        result.Add(current);

        while (previousNodes.TryGetValue(current, out TrafficNode previous))
        {
            current = previous;
            result.Add(current);
        }

        result.Reverse();
    }
}
