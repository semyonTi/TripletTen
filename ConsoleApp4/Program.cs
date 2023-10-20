using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace Triplet
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stp = new Stopwatch();
            stp.Start();
            string filePath = "C:\\test.txt";
            TopTenTriplet(filePath);
            stp.Stop();
            Console.WriteLine(stp.ElapsedMilliseconds);
        }
        public static void TopTenTriplet(string filePath)
        {
            string fileContent = System.IO.File.ReadAllText(filePath);
            var chunks = fileContent.Length / Environment.ProcessorCount;
            var chunkSize = chunks > 3 ? chunks : fileContent.Length;
            ConcurrentDictionary<string, int> triplet = new ConcurrentDictionary<string, int>();
            fileContent.Chunk(chunkSize)
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .ForAll(x => AddTriplet(x, triplet));

            var topTenTriplet = triplet.OrderBy(x => x.Value).TakeLast(10);
            foreach (var x in topTenTriplet)
            {
                Console.WriteLine((x.Key, x.Value));
            }
        }
        public static void AddTriplet(char[] str, ConcurrentDictionary<string, int> triplet)
        {
            var tmp = 0;
            for (int i = 3; i <= str.Length; i++)
            {
                var stopWords = new char[] { ' ', '!', '?', '.' };
                if (str[tmp..i].Any(x => stopWords.Contains(x)))
                {
                    tmp++;
                    continue;
                }
                triplet.AddOrUpdate(new string(str[tmp..i]), key => 1, (key, existingValue) => existingValue + 1);
                tmp++;
            }
        }
    }
}
