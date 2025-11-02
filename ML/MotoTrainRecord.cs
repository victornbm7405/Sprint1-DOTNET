using Microsoft.ML.Data;

namespace MottuProjeto.ML;

// Registro do JSON de treino: telemetria + rótulo (risco)
public class MotoTrainRecord
{
    public float TempC { get; set; }
    public float Vib { get; set; }
    public float BattPct { get; set; }

    [ColumnName("Label")]
    public bool Risco { get; set; }
}
