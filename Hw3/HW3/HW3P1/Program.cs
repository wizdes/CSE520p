using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW3P1
{
    class Program
    {
        private static Dictionary<int, int> nodeToNumConnections = new Dictionary<int, int>();
        private static Dictionary<int, double> approxNodeToNumConnections = new Dictionary<int, double>();
        private static int numIterations = 25;

        private static Random r = new Random();

        static void Main(string[] args)
        {
            string filename = "sample2.txt";
            string[] stringInput = File.ReadAllLines(filename);
            Dictionary<int, List<int>> mappingDictionary = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> inverseDictionary = new Dictionary<int, List<int>>();
            int numNodes = 0;
            foreach (string s in stringInput)
            {
                if (s.Contains("# Nodes:"))
                {
                    numNodes = int.Parse(s.Split(' ')[2]);
                    continue;
                }

                if (s.Contains("#")) continue;
                string[] mappings = s.Split();
                if (!mappingDictionary.ContainsKey(int.Parse(mappings[0])))
                {
                    mappingDictionary[int.Parse(mappings[0])] = new List<int>();
                }

                if (!inverseDictionary.ContainsKey(int.Parse(mappings[1])))
                {
                    inverseDictionary[int.Parse(mappings[1])] = new List<int>();
                }

                inverseDictionary[int.Parse(mappings[1])].Add(int.Parse(mappings[0]));
                mappingDictionary[int.Parse(mappings[0])].Add(int.Parse(mappings[1]));
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();
            calculateExact(numNodes, mappingDictionary, filename);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            calculateApprox(numNodes, inverseDictionary, filename);

            double error = 0;
            double maxRelativeError = 0;
            for (int i = 0; i < numNodes; i++)
            {
                double squaredError =(1.0 * (approxNodeToNumConnections[i] - nodeToNumConnections[i]) / nodeToNumConnections[i]) * (1.0 * (approxNodeToNumConnections[i] - nodeToNumConnections[i]) / nodeToNumConnections[i]);
                error += squaredError;
                if (maxRelativeError < squaredError)
                {
                    maxRelativeError = squaredError;
                }
            }

            error = Math.Sqrt(error)*1.0/numNodes;
            Console.WriteLine("Error 1 metric: " + error);
            Console.WriteLine("Error 2 metric: " + Math.Sqrt(maxRelativeError));

            Console.Read();
        }

        public static double GetMedian(double[] sourceNumbers)
        {
            //Framework 2.0 version of this method. there is an easier way in F4        
            if (sourceNumbers == null || sourceNumbers.Length == 0)
                return 0D;

            //make sure the list is sorted, but use a new array
            double[] sortedPNumbers = (double[])sourceNumbers.Clone();
            sourceNumbers.CopyTo(sortedPNumbers, 0);
            Array.Sort(sortedPNumbers);

            //get the median
            int size = sortedPNumbers.Length;
            int mid = size / 2;
            double median = (size % 2 != 0) ? (double)sortedPNumbers[mid] : ((double)sortedPNumbers[mid] + (double)sortedPNumbers[mid - 1]) / 2;
            return median;
        }

        private static void calculateApprox(int numNodes, Dictionary<int, List<int>> inverseDictionary, string filename)
        {
            Dictionary<int, List<double>> collectedSum = new Dictionary<int, List<double>>();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < numIterations; i++)
            {
                Dictionary<int, double> randomKey = GenerateSolutions(numNodes, inverseDictionary);
                for (int j = 0; j < numNodes; j++)
                {
                    if (!collectedSum.ContainsKey(j))
                    {
                        collectedSum[j] = new List<double>();
                    }

                    collectedSum[j].Add(randomKey[j]);
                }
            }
            watch.Stop();
            Console.WriteLine("# ms for : "  + numIterations + " : " + watch.ElapsedMilliseconds);

            Dictionary<int, double> median = new Dictionary<int, double>();
            for (int i = 0; i < numNodes; i++)
            {
                median[i] = GetMedian(collectedSum[i].ToArray());
            }

            string answer = "";
            for (int i = 0; i < numNodes; i++)
            {
                answer += i + " " + 1.0 / median[i] + "\n";
                approxNodeToNumConnections.Add(i, 1.0/median[i]);
            }

            Console.Write("Complete.");
            File.WriteAllText(filename.Split('.')[0] + "-approx_solution_" + numIterations + ".txt", answer);
        }

        private static Dictionary<int, double> GenerateSolutions(int numNodes, Dictionary<int, List<int>> inverseDictionary)
        {
            Dictionary<int, double> randomKey = new Dictionary<int, double>();
            List<Tuple<int, double>> sortedElements = new List<Tuple<int, double>>();
            for (int i = 0; i < numNodes; i++)
            {
                randomKey.Add(i, r.NextDouble()%1);
                sortedElements.Add(new Tuple<int, double>(i, randomKey[i]));
            }

            sortedElements = sortedElements.OrderBy(x => x.Item2).ToList();

            ISet<int> touchedElements = new HashSet<int>();

            while (touchedElements.Count < numNodes)
            {
                if (touchedElements.Contains(sortedElements.First().Item1))
                {
                    sortedElements.RemoveAt(0);
                }
                else
                {
                    int nodeVal = sortedElements.First().Item1;
                    double smallestVal = sortedElements.First().Item2;
                    touchedElements.Add(nodeVal);
                    List<int> numberToConsider = new List<int>();

                    if (inverseDictionary.ContainsKey(nodeVal))
                    {
                        numberToConsider.AddRange(inverseDictionary[nodeVal]);
                    }

                    while (numberToConsider.Count != 0)
                    {
                        List<int> newNumberToConsider = new List<int>();

                        foreach (int num in numberToConsider)
                        {
                            if (touchedElements.Contains(num))
                            {
                                continue;
                            }

                            randomKey[num] = smallestVal;
                            if (inverseDictionary.ContainsKey(num))
                            {
                                newNumberToConsider.AddRange(inverseDictionary[num]);                                
                            }

                            touchedElements.Add(num);
                        }

                        numberToConsider = newNumberToConsider;
                    }

                    sortedElements.RemoveAt(0);
                }
            }
            return randomKey;
        }

        private static void calculateExact(int numNodes, Dictionary<int, List<int>> mappingDictionary, string filename)
        {
            for (int i = 0; i < numNodes; i++)
            {
                if ((i*10/numNodes)%10 == 0) Console.WriteLine((i*1000/numNodes) + "%");
                calculateNumConnections(i, mappingDictionary);
            }

            string answer = "";
            for (int i = 0; i < numNodes; i++)
            {
                answer += i + " " + nodeToNumConnections[i] + "\n";
            }

            Console.Write("Complete.");
            File.WriteAllText(filename.Split('.')[0] + "-solution.txt", answer);
        }

        static void calculateNumConnections(int node, Dictionary<int, List<int>> mappingDictionary)
        {
            ISet<int> connectedNumbers = new HashSet<int>();
            List<int> numberToConsider = new List<int>();
            List<int> newNumberToConsider = new List<int>();
            if (mappingDictionary.ContainsKey(node))
            {
                numberToConsider.AddRange(mappingDictionary[node]);
            }

            int numConnections = 0;
            while (numberToConsider.Count != 0)
            {
                foreach (int x in numberToConsider)
                {
                    if (connectedNumbers.Contains(x))
                    {
                        continue;
                    }

                    connectedNumbers.Add(x);
                    numConnections++;
                    if (mappingDictionary.ContainsKey(x))
                    {
                        newNumberToConsider.AddRange(mappingDictionary[x]);                        
                    }
                }

                numberToConsider = newNumberToConsider;
                newNumberToConsider = new List<int>();
            }

            nodeToNumConnections[node] = numConnections + 1;
        }
    }
}
