using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Triplet
{
    class Program
    {
        static void Main(string[] args)
        {
            var filePath = "C:\\test.txt";
            GetTopTenTriplet(filePath);
        }

        private static void GetTopTenTriplet(string filePath)
        {
            var stp = new Stopwatch();
            stp.Start();

            var fileContent = File.ReadAllText(filePath);
            var chunks = fileContent.Length / Environment.ProcessorCount;
            var chunkSize = chunks > 3 ? chunks : fileContent.Length;
            var triplet = new ConcurrentDictionary<string, int>();
            fileContent.Chunk(chunkSize)
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .ForAll(x => AddTriplet(x, triplet));

            var topTenTriplet = triplet.OrderBy(x => x.Value).TakeLast(10);
            foreach (var x in topTenTriplet)
            {
                Console.WriteLine((x.Key, x.Value));
            }
            stp.Stop();
            Console.WriteLine(stp.ElapsedMilliseconds);
        }

        private static void AddTriplet(char[] str, ConcurrentDictionary<string, int> triplet)
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
