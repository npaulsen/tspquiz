using System;
using System.Diagnostics;
using static System.Console;

namespace GraphGen
{
    public class Starter
    {
        const string TmpFileName = "output";

        public static void Main(string[] args)
        {
            int numNodes, numEdges;
            try
            {
                numNodes = Math.Max(4, parse(args, 0) ?? 8);
                numEdges = parse(args, 1) ?? 17;
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            {
                var name = System.AppDomain.CurrentDomain.FriendlyName;
                WriteLine($"Usage: {name} [<numnodes>] [<numEdges>]");
                return;
            }

            var gen = new Generator();
            gen.GenerateTspQuiz(numNodes, numEdges, evil: true);

            Plot(gen.Graph, TmpFileName);
            Plot(gen.Graph, TmpFileName + "_sol", gen.Solution);

            ReadLine();
        }

        private static int? parse(string[] args, int i)
           => (args.Length <= i) ? (int?)null : int.Parse(args[i]);

        private static void Plot(Graph g, string file, TspTour t = null)
        {
            g.ToDotFile($"{file}.dot", t);
            var p = Process.Start("dot", $"-Tpng -o {file}.png {file}.dot");
            p.WaitForExit();
            // only on OS X
            if (p.ExitCode == 0)
                Process.Start("open", $"{file}.png");
        }

    }
}

