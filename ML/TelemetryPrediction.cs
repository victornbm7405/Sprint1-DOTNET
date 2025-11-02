using Microsoft.ML.Data;

namespace MottuProjeto.ML;

// Saída do classificador binário
public class TelemetryPrediction
{
    [ColumnName("PredictedLabel")]
    public bool Predicted { get; set; }
    public float Probability { get; set; }
    public float Score { get; set; }
}
