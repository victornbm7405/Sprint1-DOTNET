using Microsoft.ML.Data;
using Microsoft.ML.Data;

namespace MottuProjeto.ML;

// Amostras rotuladas para treino (Label = risco de manutenção)
public class TelemetryTrainData
{
    [LoadColumn(0)]
    public float TempC { get; set; }

    [LoadColumn(1)]
    public float Vib { get; set; }

    [LoadColumn(2)]
    public float BattPct { get; set; }

    [LoadColumn(3), ColumnName("Label")]
    public bool Risco { get; set; }
}
