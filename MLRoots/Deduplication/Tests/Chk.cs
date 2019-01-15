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

        class LogRow
        {
            public string Content { get; set; }
            public uint Label { get; set; }
        }

        public class PointPrediction
        {
            [KeyType]
            public uint PredictedLabel { get; set; }
        }

        LogRow Encode(string t, int label)
        {
            return new LogRow
            {
                Label = (uint)label,
                Content = t
            };
        }

        [Test]
        [TestCase(@"..\..\..\..\TestsData\lgs\syslog_short.zip")]
        public void Clusterization(string fileName)
        {
            var log_lines = new List<string>();
            Assert.That(File.Exists(fileName), "Input file not found");

            log_lines.AddRange(LoadLines(fileName));
            Assert.That(log_lines.Count > 0, "Log lines not empty");

            var r = new Random(0);
            var learn_p = new HashSet<string>();

            while (learn_p.Count != 150
                && learn_p.Count < log_lines.Count / 2)
            {
                var i = r.Next(log_lines.Count - 1);
                learn_p.Add(log_lines[i]);
            }

            var train_rows = learn_p
                .Select((q, index) => Encode(q, index + 1))
                .ToArray();

            var mlContext = new MLContext(0);
            var train_DataView = mlContext
                .CreateDataView(train_rows);            

            var trainer = mlContext.Transforms.Conversion
                .MapValueToKey("Label", "Label")
                .Append(mlContext.Transforms.Text
                .FeaturizeText("Content", "Features"))
                .Append(
                    mlContext
                    .MulticlassClassification
                    .Trainers.LightGbm());

            var txt_model = trainer.Fit(train_DataView);
            var predictionEngine = txt_model
                .CreatePredictionEngine<LogRow, PointPrediction>(mlContext);

            for (int p = 0; p < 1000; ++p)
            {
                var check_row = new LogRow { Content = log_lines[r.Next(log_lines.Count - 1)] };
                var pred_index = (int)predictionEngine.Predict(check_row).PredictedLabel;
                if (pred_index >= train_rows.Length)
                    pred_index = train_rows.Length - 1;

                var predicted_l = train_rows[pred_index - 1];
            }
        }
    }
}
