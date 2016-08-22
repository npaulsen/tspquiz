using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphGen
{
    class TspTour : IComparable<TspTour>
    {
        public readonly Graph Graph;
        protected bool[] _visited;
        protected int[] _nodes;
        public int Cost { get; private set; } = 0;
        public int Length { get; private set; } = 0;
        public int LastNode { get; private set; } = 0;

        public bool AllVisiteed => Length == _visited.Length - 1;

        public IEnumerable<int> Nodes => _nodes.TakeWhile((ind, node) => ind == 0 || node != 0);

        public IEnumerable<int> UnvisitedNodes
            => Enumerable.Range(0, _visited.Length).Where(v => !IsVisited(v));

        public TspTour(Graph g)
        {
            _visited = new bool[g.NumNodes];
            _visited[0] = true;
            _nodes = new int[g.NumNodes];
            Graph = g;
        }

        // Omitted: bounds check.
        public int this[int position] => _nodes[position];

        public bool IsVisited(int v) => _visited[v];

        public TspTour VisitNext(int node)
        {
            var v2 = _visited.ToArray();
            var n2 = _nodes.ToArray();

            if (node > 0)
            {
                v2[node] = true;
                n2[Length + 1] = node;
            }
            TspTour t = new TspTour(Graph)
            {
                _visited = v2,
                _nodes = n2,
                LastNode = node,
                Cost = Cost + Graph[LastNode, node],
                Length = Length + 1
            };
            return t;
        }

        public bool VisitsEdge(int n, int m)
        {
            if (n == m) throw new ArgumentException();
            if (!_visited[n] || !_visited[m]) return false;
            for (var i = 0; i < Length; i++)
                if (_nodes[i] == n || _nodes[i] == m)
                {
                    var next = _nodes[(i + 1) % Length];
                    if (next == (_nodes[i] == n ? m : n))
                        return true;
                    else if (i > 0) return false;
                    // if i == 0 the edge may be the back-edge.
                }
            throw new Exception("Inconsistent visited flags in tour");
        }

        public int CompareTo(TspTour o) => Cost.CompareTo(o.Cost);

        public override string ToString() => $"[Tour: {String.Join("-", Nodes)}, Length={Cost}]";
    }
}
