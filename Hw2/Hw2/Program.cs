using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Hw2
{
    class Program
    {
        static private int n = 1000000;

        static void Main(string[] args)
        {
            bool failed;
            double primeNum = getPrimeNumber();

            double[] hashArray = new double[n * 100];
            for (int i = 0; i < n * 100; i++)
            {
                hashArray[i] = i;
            }

            Random random = new Random();
            double a = random.NextDouble() % primeNum;
            double b = random.NextDouble() % primeNum;
            double x = 0;
            int hashedValue = (int)(((a * x + b) % primeNum) % n);
            hashArray[hashedValue] = x;

            Console.WriteLine(primeNum);
            Console.ReadLine();
        }

        static double getShiftedHashValue(long x, long l, long a)
        {
            // hashes x universally into l bits using the random odd seed a.
            return (a * x) >> (int)(64 - l);
        }

        private static long getPrimeNumber()
        {
            long largeNum = (long)Math.Pow(2, 60);
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
