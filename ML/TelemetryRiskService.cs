using System.Collections.Generic;
using Microsoft.ML;

namespace MottuProjeto.ML;

// Serviço extremamente simples: treina 1x em memória e prediz
public class TelemetryRiskService
{
    private readonly MLContext _ml;
    private readonly PredictionEngine<TelemetryTrainData, TelemetryPrediction> _engine;

    public TelemetryRiskService()
    {
        _ml = new MLContext();

        // Dataset mínimo embutido (exemplos fictícios só p/ funcionar)
        var dados = new List<TelemetryTrainData>
        {
            new() { TempC=40, Vib=0.10f, BattPct=90, Risco=false },
            new() { TempC=45, Vib=0.15f, BattPct=80, Risco=false },
            new() { TempC=50, Vib=0.20f, BattPct=70, Risco=false },
            new() { TempC=55, Vib=0.25f, BattPct=60, Risco=false },

            new() { TempC=60, Vib=0.30f, BattPct=55, Risco=true },
            new() { TempC=62, Vib=0.35f, BattPct=50, Risco=true },
            new() { TempC=65, Vib=0.40f, BattPct=45, Risco=true },
            new() { TempC=68, Vib=0.45f, BattPct=40, Risco=true },

            new() { TempC=52, Vib=0.55f, BattPct=85, Risco=true },  // vib alta
            new() { TempC=48, Vib=0.60f, BattPct=88, Risco=true },  // vib alta
            new() { TempC=44, Vib=0.18f, BattPct=18, Risco=true },  // bateria baixa
            new() { TempC=46, Vib=0.22f, BattPct=22, Risco=true },  // bateria baixa
            new() { TempC=35, Vib=0.08f, BattPct=95, Risco=false },
            new() { TempC=38, Vib=0.12f, BattPct=92, Risco=false }
        };

        var dataView = _ml.Data.LoadFromEnumerable(dados);

        // Features: TempC, Vib, BattPct -> classificador binário
        var pipeline = _ml.Transforms.Concatenate("Features",
                                                  nameof(TelemetryTrainData.TempC),
                                                  nameof(TelemetryTrainData.Vib),
                                                  nameof(TelemetryTrainData.BattPct))
                       .Append(_ml.BinaryClassification.Trainers.SdcaLogisticRegression());

        var model = pipeline.Fit(dataView);

        // PredictionEngine simples para uso direto no endpoint
        _engine = _ml.Model.CreatePredictionEngine<TelemetryTrainData, TelemetryPrediction>(model);
    }

    public (bool predicted, float probability, string nivel) Prever(TelemetryInput input)
    {
        var linha = new TelemetryTrainData
        {
            TempC = input.TempC,
            Vib = input.Vib,
            BattPct = input.BattPct
        };

        var pred = _engine.Predict(linha);
        var p = pred.Probability;

        // Classificação de nível bem simples (funcional)
        var nivel =
            p < 0.33f ? "Normal" :
            p < 0.66f ? "Alerta" :
                        "Risco";

        return (pred.Predicted, p, nivel);
    }
}
