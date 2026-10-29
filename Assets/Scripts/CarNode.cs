using System.Collections.Generic;
using UnityEngine;

public class CarNode : MonoBehaviour
{
    [Header("Connected Nodes")]
    public List<CarNode> connectedNodes = new List<CarNode>(4);
    public List<CarNode> forceNode = new List<CarNode>();

    [Header("Detection")]
    public float raycastDistance = 10f;
    public float raycastHeight = 0.5f;
    public bool disableRaycast = false;

    [Header("Direction Ignore")]
    public bool ignoreForward = false;
    public bool ignoreBackward = false;
    public bool ignoreRight = false;
    public bool ignoreLeft = false;

    private void Start()
    {
        DetectConnectedNodes();
        for (int i = 0; i < forceNode.Count; i++)
        {
            if (forceNode[i] != null && !connectedNodes.Contains(forceNode[i]))
            {
                connectedNodes.Add(forceNode[i]);
            }
        }
    }

    private void DetectConnectedNodes()
    {
        connectedNodes.Clear();

        // Four directions relative to this node.
        Vector3[] directions =
        {
            ignoreForward ? Vector3.zero : transform.forward,
            ignoreBackward ? Vector3.zero : -transform.forward,
            ignoreRight ? Vector3.zero : transform.right,
            ignoreLeft ? Vector3.zero : -transform.right
        };

        Vector3 origin = transform.position + Vector3.up * raycastHeight;

        foreach (Vector3 direction in directions)
        {
            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                direction,
                raycastDistance
            );

            // Find the nearest hit, ignoring this node's own colliders.
            System.Array.Sort(hits, (a, b) =>
                a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                // Ignore this node's own colliders.
                CarNode node = hit.collider.GetComponentInParent<CarNode>();

                if (node == this)
                    continue;

                if (node != null && node.disableRaycast)
                    continue;

                if (node == null)
                    break;

                if (!connectedNodes.Contains(node))
                {
                    connectedNodes.Add(node);
                }

                break;
            }
        }

        // Enforce the maximum of four connections.
        if (connectedNodes.Count > 4)
            connectedNodes.RemoveRange(4, connectedNodes.Count - 4);
    }

    private void OnTriggerEnter(Collider other)
    {
        CarNPCMove car = other.GetComponentInParent<CarNPCMove>();
    
        if (car == null)
            return;
    
        // Ignore this node unless it is the car's current target.
        if (car.targetPoint != transform)
            return;
    
        // Don't process the same node twice.
        if (car.CurrentCarNode == this)
            return;
    
        // Save the node the car came from.
        car.PrevCarNode = car.CurrentCarNode;
    
        // Record the node the car has reached.
        car.CurrentCarNode = this;
    
        // Exclude the previous node if other options exist.
        List<CarNode> possibleNodes = new List<CarNode>();
    
        foreach (CarNode node in connectedNodes)
        {
            if (node == null)
                continue;
    
            if (node != car.PrevCarNode)
                possibleNodes.Add(node);
        }
    
        // If this is a dead end, allow returning to the previous node.
        if (possibleNodes.Count == 0 &&
            car.PrevCarNode != null &&
            connectedNodes.Contains(car.PrevCarNode))
        {
            possibleNodes.Add(car.PrevCarNode);
        }
    
        // No route available: destroy the car.
        if (possibleNodes.Count == 0)
        {
            Destroy(car.gameObject);
            return;
        }
    
        // Choose a random connected node.
        CarNode chosenNode =
            possibleNodes[Random.Range(0, possibleNodes.Count)];
    
        car.targetPoint = chosenNode.transform;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.up * raycastHeight;

        Vector3[] directions =
        {
            ignoreForward ? Vector3.zero : transform.forward,
            ignoreBackward ? Vector3.zero : -transform.forward,
            ignoreRight ? Vector3.zero : transform.right,
            ignoreLeft ? Vector3.zero : -transform.right
        };

        Gizmos.color = Color.yellow;

        foreach (Vector3 direction in directions)
        {
            Gizmos.DrawRay(origin, direction * raycastDistance);
        }

        if (connectedNodes == null)
            return;

        Gizmos.color = Color.green;

        foreach (CarNode node in connectedNodes)
        {
            if (node != null)
                Gizmos.DrawLine(transform.position, node.transform.position);
        }
    }
}