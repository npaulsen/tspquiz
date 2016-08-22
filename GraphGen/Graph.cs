using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GraphGen
{
    class Graph
    {
        #region private & protected fields
        private const int NoEdge = -1;

        protected int[,] _dists;
        protected int[] _degs;
        #endregion

        public int NumNodes { get; protected set; }

        public int NumEdges { get; protected set; }

        public IEnumerable<int> Nodes => Enumerable.Range(0, NumNodes);

        public Graph(int numNodes)
        {
            this.NumNodes = numNodes;
            this.NumEdges = 0;
            _dists = new int[NumNodes, NumNodes];
            for (int x = 0; x < NumNodes; x++)
                for (int y = 0; y < NumNodes; y++)
                    _dists[x, y] = NoEdge;
            _degs = new int[NumNodes];
        }

        public bool HasEdge(int x, int y) => this[x, y] != NoEdge;

        public int GetDegreeOf(int x) => _degs[x];

        public int this[int x, int y]
        {
            get { return _dists[x, y]; }
            set
            {
                if (value != _dists[x, y])
                {
                    if (_dists[x, y] == NoEdge)
                    {
                        NumEdges++;
                        _degs[x]++;
                        _degs[y]++;
                    }
                    else if (value == NoEdge)
                    {
                        NumEdges--;
                        _degs[x]--;
                        _degs[y]--;
                    }
                    _dists[x, y] = _dists[y, x] = value;
                }

            }
        }

        public void ToDotFile(string filename = "tmp.dot", TspTour tour = null)
        {
            using (var writer = new StreamWriter(filename))
            {
                writer.WriteLine($"digraph G {{{Environment.NewLine}penwidth=2;");
                if (tour?.Length > 1)
                {
                    for (int i = 0; i < tour.Length; i++)
                    {
                        var n = tour[i];
                        var m = tour[(i + 1) % tour.Length];
                        writer.WriteLine($"{n} -> {m} [label=\"{_dists[n, m]}\",color=red,penwidth=3];");
                    }
                }

                for (int n = 0; n < NumNodes; n++)
                    for (int m = n + 1; m < NumNodes; m++)
                    {
                        if (_dists[n, m] == NoEdge) continue;
                        if (tour == null || !tour.VisitsEdge(n, m))
                            writer.WriteLine($"{n} -> {m} [dir=none,label=\"{_dists[n, m]}\"];");
                    }
                writer.WriteLine("}");
            }
        }
    }
}

