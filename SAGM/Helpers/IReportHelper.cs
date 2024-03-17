using QuestPDF.Fluent;

namespace SAGM.Helpers
{
    public interface IReportHelper
    {
        Task<byte[]> GenerateQuoteReportPDFAsync(int QuoteId);
    }
}
