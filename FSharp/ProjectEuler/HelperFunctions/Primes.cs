using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpHelperFunctions
{
    public class Primes
    {
        public static IEnumerable<int> GetAllPrimesToNum(int nNum)
        {
            var listPrimes = new List<int>();
            for (var i = 2; i <= nNum; i++)
            {
                if (IsPrime(i, listPrimes))
                {
                    listPrimes.Add(i);
                }
            }
            return listPrimes;
        }

        public static int GetNthPrime(int nNth)
        {
            var listPrimes = new List<int>();
            var i = 2;
            while(listPrimes.Count < nNth)
            {
                if (IsPrime(i, listPrimes))
                {
                    listPrimes.Add(i);
                }
                i++;
            }
            return listPrimes.Last();
        }

        public static bool IsPrime(int nNum, IEnumerable<int> enPrimesToNum)
        {
            var nFactorLimit = (int)Math.Sqrt(nNum);
            return enPrimesToNum.TakeWhile(x => x <= nFactorLimit).Any(x => nNum % x == 0) == false;
        }
    }
}
