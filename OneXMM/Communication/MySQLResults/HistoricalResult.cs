using System;


namespace OneXMM.Communication.MySQLResults
{
    public struct HistoricalResult
    {
        public string Symbol { get; set; }
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }

        public override string ToString()
        {
            return $"{Symbol}_{Timestamp.ToString("ddMMMyy HH:mm:ss")}: {Value}";
        }
    }
}
