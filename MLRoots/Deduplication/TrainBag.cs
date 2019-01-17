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
        public long PredictionTests { get; private set; } = 0;
        public long AllCount { get; private set; } = 0;
        public long PredictionErrorsCount { get; private set; } = 0;
        public long LastTrainPredictionErrors { get; private set; } = 0;
        public long LastTrainPredictionCount { get; private set; } = 0;
        public long LastTrainDirectFireCount { get; private set; } = 0;

        readonly Random trainRandom = new Random(0);
        private readonly bool usePrediction;
        const double LThres = 0.85;        

        PredictionEngine<LogRow, PointPrediction> prediction;

        public List<OneItemBag> OneItemBags { get; } = new List<OneItemBag>();

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

        public TrainBag(bool usePrediction)
        {
            this.usePrediction = usePrediction;
        }

        public void Push(string message)
        {
            AllCount++;

            if (prediction != null)
            {
                LastTrainPredictionCount++;
                PredictionTests++;

                var p_l = prediction
                    .Predict(new LogRow { Content = message })
                    .PredictedLabel;

                var item_bag = OneItemBags[(int)p_l - 1];
                if (IsTheSame(item_bag.BaseMessage, message))
                {
                    item_bag.AddMessage(message);
                    PredictedCount++;

                    return;
                }
                else
                {
                    PredictionErrorsCount++;
                    LastTrainPredictionErrors++;
                }
            }

            int finded = -1;

            for(int oiIndex = 0; oiIndex < OneItemBags.Count; ++oiIndex)
            {
                if (IsTheSame(OneItemBags[oiIndex].BaseMessage, message))
                    finded = oiIndex;                
            }

            if(finded >= 0)
            {
                var oib = OneItemBags[finded];
                oib.AddMessage(message);
                LastTrainDirectFireCount++;
                UpdateTrain();

                int rIndex = finded;
                while (rIndex > 0 && oib.CompleteCount > OneItemBags[rIndex - 1].CompleteCount)
                {
                    OneItemBags.RemoveAt(rIndex);
                    OneItemBags.Insert(rIndex - 1, oib);
                    rIndex--;
                }
                return;
            }

            var noib = new OneItemBag(message, (uint)OneItemBags.Count + 1);
            OneItemBags.Add(noib);
            UpdateTrain();
        }

        void Train()
        {
            if (OneItemBags.Count < 2)
                return;

            var t_rows = (from q2 in OneItemBags
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

        private void UpdateTrain()
        {
            if (!usePrediction)
                return;

            if (OneItemBags.Count > 10 && prediction == null
                            || (prediction != null && LastTrainDirectFireCount > 1000))
            {
                Train();
                LastTrainPredictionCount = 0;
                LastTrainPredictionErrors = 0;
                LastTrainDirectFireCount = 0;

                if (PredictionTests != 0)
                {
                    Trace.TraceInformation($"Predicted: {PredictedCount} / {PredictionTests} ({PredictedCount * 100L / PredictionTests} %)");

                    Trace.TraceInformation(new
                    {
                        GroupsCount = OneItemBags.Count,
                        MaxInGroup = OneItemBags.Select(q => q.CompleteCount).Max(),
                        GarbageGroups = OneItemBags.Where(q => q.CompleteCount < 10).Count()
                    }.ToString());
                }
            }
        }
    }
}
