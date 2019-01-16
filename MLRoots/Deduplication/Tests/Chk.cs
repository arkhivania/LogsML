using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.LightGBM;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using static Microsoft.ML.Transforms.Normalizers.NormalizingEstimator;

namespace MLRoots.Deduplication.Tests
{
    [TestFixture]
    class Chk
    {
        IEnumerable<string> LoadLines(string fileName)
        {
            using (var ar = ZipFile.OpenRead(fileName))
                foreach (var e in ar.Entries
                    .Where(q => q.Length > 0))
                {
                    using (var stream = e.Open())
                    using (var sr = new StreamReader(stream))
                        while (!sr.EndOfStream)
                        {
                            var log_line = sr.ReadLine();
                            if (!string.IsNullOrEmpty(log_line))
                                yield return log_line;
                        }
                }
        }



        [Test]
        [TestCase(@"..\..\..\..\TestsData\lgs\syslog_short.zip")]
        [TestCase(@"..\..\..\..\TestsData\lgs\full.zip")]
        public void Clusterization(string fileName)
        {
            var log_lines = new List<string>();
            Assert.That(File.Exists(fileName), "Input file not found");

            log_lines.AddRange(LoadLines(fileName));
            Assert.That(log_lines.Count > 0, "Log lines not empty");


            var t_b = new TrainBag();

            foreach (var m in log_lines)
                t_b.Push(m);

            var all_size = t_b
                .Bags
                .SelectMany(q => q.CompleteSet)
                .Select(q => (long)q.Length).Sum();

            var compressed = t_b
                .Bags
                .Select(q => (long)q.GetCompressed().Length)
                .Sum();

            TestContext.Write(
                new
                {
                    Compressed = compressed,
                    AllSize = all_size,
                    Ratio = (float)compressed / (float)all_size,
                    RRatio = (float)all_size / (float)compressed
                });
        }
    }
}
