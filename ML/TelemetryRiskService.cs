using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace MottuProjeto.ML
{
    // ---- POCOs do modelo (deixe só aqui) ----
    public class TelemetryInput
    {
        public float TempC { get; set; }
        public float Vib { get; set; }
        public float BattPct { get; set; }
        public bool Risco { get; set; } // Label no treino sintético
    }

    public class TelemetryPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Predicted { get; set; }
        public float Probability { get; set; }
        public float Score { get; set; }
    }

    // ---- DTOs do endpoint ----
    public class TelemetryRequest
    {
        public float TempC { get; set; }
        public float Vib { get; set; }
        public float BattPct { get; set; }
    }

    public class TelemetryResponse
    {
        public float TempC { get; set; }
        public float Vib { get; set; }
        public float BattPct { get; set; }
        public bool Predicted { get; set; }
        public float Probability { get; set; }
        public string Nivel { get; set; } = "Normal";
    }

    // ---- Serviço ML.NET ----
    public class TelemetryRiskService
    {
        private readonly MLContext _ml;
        private readonly ITransformer _model;

        public TelemetryRiskService()
        {
            _ml = new MLContext(seed: 42);

            var data = new List<TelemetryInput>
            {
                new() { TempC = 40, Vib = 0.10f, BattPct = 90, Risco = false },
                new() { TempC = 45, Vib = 0.15f, BattPct = 80, Risco = false },
                new() { TempC = 50, Vib = 0.20f, BattPct = 70, Risco = false },
                new() { TempC = 55, Vib = 0.25f, BattPct = 60, Risco = false },
                new() { TempC = 58, Vib = 0.28f, BattPct = 55, Risco = true  },
                new() { TempC = 60, Vib = 0.30f, BattPct = 50, Risco = true  },
                new() { TempC = 62, Vib = 0.34f, BattPct = 45, Risco = true  },
                new() { TempC = 65, Vib = 0.38f, BattPct = 40, Risco = true  },
                new() { TempC = 68, Vib = 0.42f, BattPct = 35, Risco = true  },
                new() { TempC = 70, Vib = 0.45f, BattPct = 30, Risco = true  },
            };

            var train = _ml.Data.LoadFromEnumerable(data);

            var pipeline =
                _ml.Transforms.Concatenate("Features",
                        nameof(TelemetryInput.TempC),
                        nameof(TelemetryInput.Vib),
                        nameof(TelemetryInput.BattPct))
                  .Append(_ml.Transforms.NormalizeMinMax("Features"))
                  .Append(_ml.BinaryClassification.Trainers.SdcaLogisticRegression(
                      labelColumnName: nameof(TelemetryInput.Risco),
                      featureColumnName: "Features"));

            _model = pipeline.Fit(train);
        }

        public TelemetryResponse Predict(TelemetryRequest req)
        {
            var engine = _ml.Model.CreatePredictionEngine<TelemetryInput, TelemetryPrediction>(_model);

            var pred = engine.Predict(new TelemetryInput
            {
                TempC = req.TempC,
                Vib = req.Vib,
                BattPct = req.BattPct
            });

            var nivel = pred.Probability >= 0.80f ? "Risco"
                      : pred.Probability >= 0.60f ? "Alerta"
                      : "Normal";

            return new TelemetryResponse
            {
                TempC = req.TempC,
                Vib = req.Vib,
                BattPct = req.BattPct,
                Predicted = pred.Predicted,
                Probability = pred.Probability,
                Nivel = nivel
            };
        }
    }
}
