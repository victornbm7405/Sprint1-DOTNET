namespace MottuProjeto.ML;

// Entrada do usuário no POST do endpoint de ML
public class TelemetryInput
{
    public float TempC { get; set; }   // temperatura do motor (°C)
    public float Vib { get; set; }     // vibração (g), ex.: 0.35
    public float BattPct { get; set; } // bateria em % (0..100)
}
