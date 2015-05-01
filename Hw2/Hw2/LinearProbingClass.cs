using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Hw2
{
    public class LinearProbingClass
    {
        static private int n = 100000;
        private static bool readingNumbersFromFile = false;
        private static bool collectLargeNumberBool = false;


        public static void mainfunction()
        {
            File.WriteAllText("C:\\Data\\hw2p1.txt", string.Empty);
            if (!readingNumbersFromFile)
            {
                File.WriteAllText("C:\\Data\\collisionVal.txt", string.Empty);
            }

            double primeNum = getPrimeNumber();
            Stopwatch timer = Stopwatch.StartNew();

            Random random = new Random();
            long a = random.Next(1, Int32.MaxValue);
            double b = random.Next(1, Int32.MaxValue);
            long odd_a = random.Next(1, Int32.MaxValue);
            while (odd_a % 2 == 0)
            {
                odd_a = random.Next(1, Int32.MaxValue);
            }

            double[] numbersReadFromFile = new double[n];

            if (readingNumbersFromFile)
            {
                var lines = File.ReadLines("C:\\Data\\collisionVal.txt");
                int i = 0;
                foreach (var line in lines)
                {
                    if (i > n - 1)
                    {
                        break;
                    }
                    numbersReadFromFile[i++] = Convert.ToDouble(line);
                }
            }

            long x = 0;

            int numElements = 100;

            int hashTableSize = n;
            int hashTableMultiplier = 0;

            for(hashTableSize = n; hashTableSize < 6 * n; hashTableSize = hashTableSize + n)
            {
                hashTableMultiplier += 1;
                int[] hashArrayCount = new int[hashTableSize];
                Stopwatch timePerParse;

                for (numElements = n; numElements < n + 1; numElements = numElements + nextIter(numElements))
                {
                    timePerParse = Stopwatch.StartNew();

                    //List<double>[] hashTable = new List<double>[n];
                    for (int i = 0; i < n; i++)
                    {
                        hashArrayCount[i] = 0;
                    }

                    int longestProbe = 0;
                    double sumSquareProbeLengths = 0;

                    for (int i = 0; i < numElements; i++)
                    {
                        x = generateX(random);

                        if (readingNumbersFromFile)
                        {
                            x = (long)numbersReadFromFile[i];
                        }
                        int hashedValue = 0;
                        hashedValue = GetShiftedHashValue(hashedValue, x, odd_a);
                        //hashedValue = GetModPrimeHashValue(hashedValue, a, x, b, primeNum);
                        //hashedValue = getFavoriteHashValue(hashedValue, a, x, odd_a, b, primeNum);

                        int index = hashedValue;
                        int probeLength = 0;
                        while (hashArrayCount[index] > 0)
                        {
                            index = (index + 1) % hashTableSize;
                            probeLength += 1;
                        }

                        if (probeLength > longestProbe)
                        {
                            longestProbe = probeLength;
                        }
                        sumSquareProbeLengths += probeLength * probeLength;

                        hashArrayCount[index] += 1;

                        hashArrayCount[hashedValue]++;

                        //if (hashTable[hashedValue] == null)
                        //{
                        //    hashTable[hashedValue] = new List<double>();
                        //}

                        //hashTable[hashedValue].Add(x);
                    }

                    timePerParse.Stop();
                    long ticksThisTime = timePerParse.ElapsedTicks;

                    double sumSquared = 0;
                    int largest = 0;
                    int time = (int)ticksThisTime;

                    List<double> collisionNumbers = new List<double>();
                    for (int i = 0; i < n; i++)
                    {
                        sumSquared += hashArrayCount[i] * hashArrayCount[i];
                        if (largest < hashArrayCount[i])
                        {
                            largest = hashArrayCount[i];
                        }

                        //if (collectLargeNumberBool)
                        //{
                        //    CollectLargeNumbers(hashArrayCount, i, hashTable, collisionNumbers);
                        //}
                    }

                    if (collectLargeNumberBool)
                    {
                        Console.WriteLine("# collisions for 10+: " + collisionNumbers.Count);
                        foreach (double c in collisionNumbers)
                        {
                            File.AppendAllText("C:\\Data\\collisionVal.txt", c + "\n");
                        }
                    }

                    Console.WriteLine("N: " + numElements + ": " + sumSquareProbeLengths + " " + longestProbe + " " + time + " - " + timer.ElapsedMilliseconds / 1000 + " " + a + " " + b + " " + odd_a);
                    File.AppendAllText(
                        "C:\\Data\\hw2p2_fav_n_" + hashTableMultiplier + ".txt", "N: " + numElements + ": " + sumSquareProbeLengths + " " + longestProbe + " " + time + " - " + timer.ElapsedMilliseconds / 1000 + " " + a + " " + b + " " + odd_a + "\n");
                }                
            }

            Console.ReadLine();
        }

        private static void CollectLargeNumbers(int[] hashArrayCount, int i, List<double>[] hashTable, List<double> collisionNumbers)
        {
            if (hashArrayCount[i] > 124)
            {
                foreach (double c in hashTable[i])
                {
                    collisionNumbers.Add(c);
                }
            }
        }

        private static int getFavoriteHashValue(int hashedValue, long a, long x, long odd_a, double b, double primeNum)
        {
            hashedValue = (int)(((a * getShiftedHashValue(x, 32, odd_a) + b) % primeNum) % n);
            return hashedValue;
        }

        private static int GetModPrimeHashValue(int hashedValue, long a, long x, double b, double primeNum)
        {
            hashedValue = (int)(((a * x + b) % primeNum) % n);
            return hashedValue;
        }

        private static int GetShiftedHashValue(int hashedValue, long x, long odd_a)
        {
            hashedValue = (int)getShiftedHashValue(x, 32, odd_a) % n;
            return hashedValue;
        }

        private static int nextIter(int numElements)
        {
            if (numElements < 1000)
            {
                return 100;
            }
            int num = 100;
            numElements = numElements / 100;
            for (int i = 0; i < 10000000; i++)
            {
                if (numElements / 10 == 0)
                {
                    return num;
                }

                numElements = numElements / 10;
                num *= 10;
            }

            return num;
        }

        private static long generateX(Random r)
        {
            return (long)(r.Next(1, Int32.MaxValue));
        }

        static long getShiftedHashValue(long x, long l, long a)
        {
            // hashes x universally into l bits using the random odd seed a.
            return (a * x) >> (int)(64 - l);
        }

        private static long getPrimeNumber()
        {
            long largeNum = (long)Math.Pow(2, 33);
            long primeNum = -1;
            System.Console.WriteLine(largeNum);

            int count = 0;
            for (long i = largeNum; i < largeNum + 100000; i++)
            {
                count++;
                if (Integers.IsPrime(i))
                {
                    primeNum = i;
                    break;
                }
                if (count == 100000)
                {
                    throw new Exception();
                }
            }
            return primeNum;
        }
    }
}
