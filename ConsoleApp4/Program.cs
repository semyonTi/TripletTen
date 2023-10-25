using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Triplet
{
    class Program
    {
        static void Main(string[] args)
        {
            var filePath = "C:\\test1.txt";
            long fileSize;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fileSize = fileStream.Length;
            }
            //bufferSize зависит от размера файла, и количества памяти
            var bufferSize = 1024 * 1024;
            var codersWords = CreateMapLetters();
            Bench(() => ProcessFileInParalle(filePath, bufferSize, codersWords, fileSize));
        }

        static Dictionary<char, int> CreateMapLetters()
        {
            var count = 0;
            var codersWords = new Dictionary<char, int>();

            for (var letter = 'a'; letter <= 'z'; letter++)
            {
                codersWords.Add(letter, count);
                count++;
            }

            for (var letters = 'A'; letters <= 'Z'; letters++)
            {
                codersWords.Add(letters, count);
                count++;
            }
            return codersWords;
        }

        static void Bench(Action action)
        {
            var stp = new Stopwatch();
            stp.Start();
            action();
            stp.Stop();
            Console.WriteLine(stp.ElapsedMilliseconds);
        }

        static void ProcessFileInParalle(string filePath, int bufferSize, Dictionary<char, int> codersWords, long fileSize)
        {
            var allCountTriplets = 52 * 52 * 52;
            var allTriplet = new int[allCountTriplets];
            var partitioner = Partitioner.Create(0, fileSize, bufferSize);
            Parallel.ForEach(partitioner, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                range =>
                {
                    var localArrayLetters = new int[allCountTriplets];

                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.Seek(range.Item1, SeekOrigin.Begin);

                        while (fileStream.Position < range.Item2)
                        {
                            var buffer = new byte[bufferSize];
                            var bytesRead = fileStream.Read(buffer, 0, buffer.Length);

                            if (bytesRead == 0)
                            {
                                continue;
                            }
                            else
                            {
                                var tripletStart = 0;
                                var result = Encoding.UTF8.GetString(buffer, 0, bytesRead).Replace("\0", "");
                                for (var tripletEnd = 3; tripletEnd <= result.Length; tripletEnd++, tripletStart++)
                                {
                                    var triplet = result[tripletStart..tripletEnd];
                                    if (triplet.All(x => codersWords.ContainsKey(x)))
                                    {
                                        var codersTriplet = codersWords[result[tripletStart]] * 52 * 52
                                        + codersWords[result[tripletStart + 1]] * 52
                                        + codersWords[result[tripletStart + 2]];
                                        localArrayLetters[codersTriplet]++;
                                    }
                                }
                            }
                        }
                    }
                    lock (allTriplet)
                    {
                        for (var i = 0; i < localArrayLetters.Length; i++)
                        {
                            allTriplet[i] += localArrayLetters[i];
                        }
                    }
                });

            var topTenTriplets = allTriplet
                .Select((value, index) => new { value, index })
                .OrderByDescending(item => item.value)
                .Take(10)
                .ToArray();

            for (var i = 0; i < 10; i++)
            {
                Console.Write(topTenTriplets[i].value);
                var first = topTenTriplets[i].index / (52 * 52);
                var second = (topTenTriplets[i].index - first * 52 * 52) / 52;
                var third = topTenTriplets[i].index - (first * 52 * 52 + second * 52);
                var leterOne = codersWords.First(x => x.Value == first);
                var leterTwo = codersWords.First(x => x.Value == second);
                var leterThree = codersWords.First(x => x.Value == third);
                Console.WriteLine((leterOne.Key, leterTwo.Key, leterThree.Key));
            }
        }
    }
}




