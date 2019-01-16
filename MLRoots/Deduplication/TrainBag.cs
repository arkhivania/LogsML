using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace MLRoots.Deduplication
{
    class TrainBag
    {
        public long PredictedCount { get; private set; } = 0;
        public long AllCount { get; private set; } = 0;
        public long PredictionErrorsCount { get; private set; } = 0;

        readonly Random trainRandom = new Random(0);
        long? lastTrainTimeMs;

        const double LThres = 0.85;        

        PredictionEngine<LogRow, PointPrediction> prediction;

        readonly List<OneItemBag> oneItemBags = new List<OneItemBag>();
        public IEnumerable<OneItemBag> Bags => oneItemBags;

        bool IsTheSame(string a, string b)
        {
            var dist_diff = System.Math.Abs(a.Length - b.Length);
            var mid_l = (a.Length + b.Length + 1) / 2;
            if ((double)dist_diff / mid_l > (1.0 - LThres))
                return false;

            var stepsToSame = Fastenshtein.Levenshtein.Distance(a, b);
            var ls2 = (1.0 - (stepsToSame / (double)Math.Max(a.Length, b.Length)));            

            return ls2 > LThres;
        }

        class LogRow
        {
            public string Content { get; set; }
            public uint Label { get; set; }
        }

        class PointPrediction
        {
            [KeyType]
            public uint PredictedLabel { get; set; }
        }

        void Train()
        {
            if (oneItemBags.Count < 2)
                return;

            var t_rows = (from q2 in oneItemBags
                          let lab = q2.Label
                          from t_m in q2.TrainSet
                          select new LogRow { Content = t_m, Label = lab }).ToArray();

            var mlContext = new MLContext(0);
            var train_DataView = mlContext
                .CreateDataView(t_rows);

            var trainer = mlContext.Transforms.Conversion
                .MapValueToKey("Label", "Label")
                .Append(mlContext.Transforms
                .Text.FeaturizeText("Content", "Features"))
                .Append(
                    mlContext
                    .MulticlassClassification
                    .Trainers
                    .LightGbm());

            try
            {
                var txt_model = trainer.Fit(train_DataView);
                this.prediction = txt_model
                    .CreatePredictionEngine<LogRow, PointPrediction>(mlContext);
            }
            catch
            {
                prediction = null;
            }
        }

        public void Push(string message)
        {
            if (prediction != null)
            {
                var p_l = prediction
                    .Predict(new LogRow { Content = message })
                    .PredictedLabel;

                var item_bag = oneItemBags[(int)p_l - 1];
                if (IsTheSame(item_bag.BaseMessage, message))
                {
                    item_bag.AddMessage(message);
                    PredictedCount++;
                    AllCount++;
                    return;
                }
                else
                    PredictionErrorsCount++;
            }

            var ct = Stopwatch.StartNew();

            foreach (var oib in oneItemBags)
            {
                if (IsTheSame(oib.BaseMessage, message))
                {
                    oib.AddMessage(message);
                    AllCount++;
                    return;
                }
            }

            ct.Stop();

            var direct_time = ct.ElapsedMilliseconds;

            var noib = new OneItemBag(message, (uint)oneItemBags.Count + 1);
            oneItemBags.Add(noib);

            long factor = 1 + (lastTrainTimeMs ?? 1) / (direct_time + 1);
            if (trainRandom.Next((int)(factor/20)) == 0)
            {
                var t_time = Stopwatch.StartNew();
                Train();
                t_time.Stop();

                if (AllCount != 0)
                {
                    Trace.TraceInformation($"Predicted: {PredictedCount} / {AllCount} ({PredictedCount * 100L / AllCount} %)");

                    Trace.TraceInformation(new
                    {
                        GroupsCount = oneItemBags.Count,
                        MaxInGroup = oneItemBags.Select(q => q.CompleteCount).Max(),
                        GarbageGroups = oneItemBags.Where(q => q.CompleteCount < 10).Count()
                    }.ToString());
                }

                lastTrainTimeMs = t_time.ElapsedMilliseconds;
            }

            AllCount++;
        }        
    }
}
