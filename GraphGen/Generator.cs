using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using static System.Console;

namespace GraphGen
{
    class Generator
    {
        const int MinEdgeWeight = 3;
        const int MaxEdgeWeight = 9;

        static private Random _rand = new Random(Environment.TickCount);

        // fields
        Graph _graph;
        List<TspTour> _tours;

        internal Graph Graph => _graph;
        internal TspTour Solution => _tours[0];
        internal bool PrintTourStats { get; set; } = true;

        internal void GenerateTspQuiz(int numNodes, int numEdges, bool evil)
        {
            _graph = GenerateGraph(numNodes, numEdges);
            WriteLine("generated graph");

            while (true)
            {
                RecalcTours();
                if (_tours[0].Cost != _tours[1].Cost)
                    break;
                var edgeIndex = 0;
                while (_tours[0].VisitsEdge(_tours[1][edgeIndex], _tours[1][edgeIndex + 1]))
                    edgeIndex++;
                _graph[_tours[1][edgeIndex], _tours[1][edgeIndex + 1]]++;
                WriteLine($"increased weight of edge {_tours[1][edgeIndex]}" +
                          $"-{_tours[1][edgeIndex + 1]} to reduce # of optima");
            }

            if (evil)
            {
                // Pick edges not in the best tour and make 'em cheaper.
                var deg3 = _graph.Nodes.Where(x => _graph.GetDegreeOf(x) >= 3);
                foreach (var v in deg3)
                {
                    var ws = _graph.Nodes.Where(x => x != v && _graph.HasEdge(v, x) && !_tours[0].VisitsEdge(v, x));
                    foreach (var w in ws.OrderByDescending(x => _graph[v, x]))
                    {
                        var cheapestTourUsingWCost = _tours.First(t => t.VisitsEdge(v, w)).Cost;
                        var newWeight = Math.Max(MinEdgeWeight, _graph[v, w] - cheapestTourUsingWCost + _tours[0].Cost + 1);
                        if (newWeight != _graph[v, w])
                        {
                            WriteLine($"reducing cost of edge {v}-{w} from {_graph[v, w]} to {newWeight}");
                            _graph[v, w] = newWeight;
                            RecalcTours();
                        }
                    }
                }
            }
        }

        private void RecalcTours()
        {
            if (_tours == null) _tours = new List<TspTour>();
            else _tours.Clear();
            Exhaustive_internal(new TspTour(_graph));
            _tours.Sort();
            if (PrintTourStats)
            {
                WriteLine($"Generated {_tours.Count} tours by exhaustive search.");
                WriteLine($"\t best tour: {_tours[0]}");
                int sndC = _tours[1].Cost;
                for (int s = 1; s < _tours.Count && _tours[s].Cost == sndC; s++)
                    WriteLine($"\t tour {s + 1,4}: {_tours[s]}");
                WriteLine($"\tworst tour: {_tours[_tours.Count - 1]}");
            }
        }

        private void Exhaustive_internal(TspTour t)
        {
            if (t.AllVisiteed && _graph.HasEdge(t.LastNode, 0))
            {
                _tours.Add(t.VisitNext(0));
                return;
            }
            foreach (var next in t.UnvisitedNodes.Where(x => _graph.HasEdge(t.LastNode, x)))
            {
                // Assuming symmetric distances, we only want one 
                // directed traversal of a TspTour-Slope
                if (_graph.NumNodes > 3 && next == 2 && !t.IsVisited(1))
                    continue;
                Exhaustive_internal(t.VisitNext(next));
            }
        }

        internal static Graph GenerateGraph(int numN, int numE)
        {
            var rndWeight = new Func<int>(
                () => MinEdgeWeight + _rand.Next((MaxEdgeWeight - MinEdgeWeight + 1)));
            var g = new Graph(numN);
            // Add a cycle with random edge weights
            for (int i = 0; i < numN && i < numE; i++)
                g[i, (i + 1) % numN] = rndWeight();

            var vertices = g.Nodes.ToList();

            while (g.NumEdges < numE)
            {
                // Find a vertex with minimal degree.
                vertices.Sort((a, b) => g.GetDegreeOf(a).CompareTo(g.GetDegreeOf(b)));
                var v = vertices[0];
                var possibleNeighbors = g.NumNodes - 1 - g.GetDegreeOf(v);
                // is the graph complete already?
                if (possibleNeighbors == 0)
                    break;
                var peerCandidates = g.Nodes.Where(x => x != v && !g.HasEdge(v, x));
                // pick a random edge peer for v.
                var w = peerCandidates.Skip(_rand.Next(possibleNeighbors)).First();
                g[v, w] = rndWeight();
            }
            return g;
        }
    }
}
