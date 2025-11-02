namespace MottuProjeto.ML;

// Entrada do usuário para prever risco
public class MotoInput
{
    public float TempC { get; set; }    // °C
    public float Vib { get; set; }      // g
    public float BattPct { get; set; }  // 0..100
}
