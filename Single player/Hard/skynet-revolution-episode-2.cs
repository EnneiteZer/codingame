using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        var game = new Game();

        game.InitOnce();

        // game loop
        while (true)
        {
            string output = game.PlayTurn();
            Console.WriteLine(output);
        }
    }
}

public class Game
{
    const int NumberOfTurnsSimulation = 2;

    Dictionary<int, Node> _nodes = new Dictionary<int, Node>();

    public void InitOnce()
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int N = int.Parse(inputs[0]); // the total number of nodes in the level, including the gateways
        int L = int.Parse(inputs[1]); // the number of links
        int E = int.Parse(inputs[2]); // the number of exit gateways


        for (int i = 0; i < L; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int N1 = int.Parse(inputs[0]); // N1 and N2 defines a link between these nodes
            int N2 = int.Parse(inputs[1]);

            bool n1, n2;
            Node node1, node2;

            n1 = _nodes.TryGetValue(N1, out node1);
            n2 = _nodes.TryGetValue(N2, out node2);

            if (n1 == false)
            {
                node1 = new Node(N1);
                _nodes.Add(N1, node1);
            }
            if (n2 == false)
            {
                node2 = new Node(N2);
                _nodes.Add(N2, node2);
            }

            node1.Neighbors.Add(node2);
            node2.Neighbors.Add(node1);
        }

        for (int i = 0; i < E; i++)
        {
            int EI = int.Parse(Console.ReadLine()); // the index of a gateway node
            _nodes[EI].IsGateway = true;
        }
    }

    public string PlayTurn()
    {
        string output = "";
        int SI = int.Parse(Console.ReadLine()); // The index of the node on which the Skynet agent is positioned this turn


        var currentNode = _nodes[SI];
        Tuple<Node, Node> link = null;

        // if gateway next to node => cut link
        if (currentNode.Neighbors.Any(n => n.IsGateway))
        {
            var gateway = currentNode.Neighbors.First(n => n.IsGateway);

            link = new Tuple<Node, Node>(currentNode, gateway);
        }
        // if no gateway next to node => time to seek next double link gateway and cut the link
        else
        {
            // find the next node connected to 2 gateways
            var nextDouble = findNearestDoubleConnectedNode(currentNode);

            // if no double connected found, random cut
            if (nextDouble == null)
            {
                // cut random link
                foreach (var node in _nodes.Values)
                {
                    var gateway = node.Neighbors.FirstOrDefault(n => n.IsGateway);

                    if (gateway != null)
                    {
                        link = new Tuple<Node, Node>(node, gateway);
                    }
                }
                Console.Error.WriteLine("random cut");
            }
            // else cut one of the link of the double connected
            else
            {
                var gateway = nextDouble.Neighbors.First(n => n.IsGateway);
                link = new Tuple<Node, Node>(nextDouble, gateway);
            }
        }

        output = cut(link);
        return output;
    }

    private Node findNearestDoubleConnectedNode(Node start)
    {
        var doubleConnected = _nodes.Values.Where(n => n.Neighbors.Count(neigh => neigh.IsGateway) == 2).ToList();

        doubleConnected.ForEach(d => Console.Error.WriteLine(d.Number));

        Node next = null;
        if (doubleConnected.Count > 0)
        {
            var paths = new List<List<Node>>();

            doubleConnected.ForEach(x => paths.Add(
                getPathToNode(start, x)));

            // determine the most priority path

            // first check if there is a path with no node not connected to gateway
            List<Node> priority = null;
            foreach (var path in paths)
            {
                path.ForEach(n => Console.Error.Write(n.Number + " "));
                Console.Error.WriteLine();
                if (path.Any(node => node.Neighbors.Count(n => n.IsGateway) == 0) == false)
                {
                    priority = path;
                    break;
                }
            }

            // if no priority, take the shorter
            if (priority == null)
                priority = paths.OrderBy(p => p.Count).First();

            next = priority.First();
        }

        return next;
    }

    private List<Node> getPathToNode(Node start, Node end)
    {
        var explored = new List<(Node, Node)>();
        var toExplore = new Queue<(Node, Node)>();

        toExplore.Enqueue((start, null));

        var tmp = new List<(Node, Node)>();

        // bfs
        bool ok = false;
        while (true)
        {
            tmp.Clear();

            while (toExplore.Count > 0)
            {
                var cpl = toExplore.Dequeue();

                foreach (var neigh in cpl.Item1.Neighbors)
                {
                    if (neigh.IsGateway == false)
                    {
                        var newCpl = (neigh, cpl.Item1);

                        if (neigh.Equals(end))
                        {
                            explored.Add((neigh, cpl.Item1));
                            ok = true;
                            break;
                        }
                        else if (explored.Any(e => e.Item1.Equals(neigh)) == false)
                        {
                            toExplore.Enqueue(newCpl);
                        }
                    }
                }

                explored.Add(cpl);
            }

            if (ok == false)
                tmp.ForEach(n => toExplore.Enqueue(n));
            else
                break;
        }

        List<Node> path = new List<Node>();

        var exp = explored.First(c => c.Item1.Equals(end));

        while (exp.Item2 != null)
        {
            path.Add(exp.Item1);
            exp = explored.First(c => c.Item1.Equals(exp.Item2));
        }

        return path;
    }


    private string cut(Tuple<Node, Node> link)
    {
        link.Item1.Neighbors.Remove(link.Item2);
        link.Item2.Neighbors.Remove(link.Item1);
        return $"{link.Item1.Number} {link.Item2.Number}";
    }
}

public class Link
{
    public int N1;
    public int N2;

    public Link(int n1, int n2)
    {
        N1 = n1;
        N2 = n2;
    }
}

public class Node
{
    public int Number;

    public bool IsGateway = false;
    public List<Node> Neighbors = new List<Node>();

    public Node(int number)
    {
        Number = number;
    }
}