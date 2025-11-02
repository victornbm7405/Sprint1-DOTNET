using System.Text.Json;
using Microsoft.ML;

namespace MottuProjeto.ML;

public class MotoRiskModelService
{
    private readonly MLContext _ml;
    private readonly PredictionEngine<MotoTrainRecord, MotoPrediction> _engine;

    public MotoRiskModelService()
    {
        _ml = new MLContext();

        // Caminho do JSON DE TREINO (obrigatório)
        var path = Path.Combine(AppContext.BaseDirectory, "data", "ml", "motosTreino.json");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Arquivo de treino não encontrado: {path}");

        var json = File.ReadAllText(path);
        var dados = JsonSerializer.Deserialize<List<MotoTrainRecord>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        ) ?? throw new InvalidOperationException("Não foi possível desserializar os dados de treino.");

        if (dados.Count == 0)
            throw new InvalidOperationException("Arquivo de treino está vazio.");

        var dataView = _ml.Data.LoadFromEnumerable(dados);

        // Features numéricas simples -> classificador binário
        var pipeline =
            _ml.Transforms.Concatenate("Features",
                nameof(MotoTrainRecord.TempC),
                nameof(MotoTrainRecord.Vib),
                nameof(MotoTrainRecord.BattPct))
            .Append(_ml.BinaryClassification.Trainers.SdcaLogisticRegression());

        var model = pipeline.Fit(dataView);

        _engine = _ml.Model.CreatePredictionEngine<MotoTrainRecord, MotoPrediction>(model);
    }

    public (bool predicted, float probability, string nivel) Prever(MotoInput input)
    {
        var linha = new MotoTrainRecord
        {
            TempC = input.TempC,
            Vib = input.Vib,
            BattPct = input.BattPct
        };

        var pred = _engine.Predict(linha);
        var p = pred.Probability;

        var nivel = p < 0.33f ? "Normal" : p < 0.66f ? "Alerta" : "Risco";

        return (pred.Predicted, p, nivel);
 





   }
}
