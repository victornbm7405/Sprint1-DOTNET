namespace MottuProjeto.ML
{
    public class MotoRiskModelService
    {
        private readonly TelemetryRiskService _telemetry;
        public MotoRiskModelService(TelemetryRiskService telemetry) => _telemetry = telemetry;

        public TelemetryResponse Predict(float tempC, float vib, float battPct)
        {
            return _telemetry.Predict(new TelemetryRequest
            {
                TempC = tempC,
                Vib = vib,
                BattPct = battPct
            });
        }
    }
}
