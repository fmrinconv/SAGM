using QuestPDF.Fluent;

namespace SAGM.Helpers
{
    public interface IReportHelper
    {
        Task<byte[]> GenerateQuoteReportPDFAsync(int QuoteId);

        Task<byte[]> GenerateWorkOrderReportPDFAsync(int WorkOrderId);

        Task<byte[]> GenerateOrderReportPDFAsync(int OrderId);

        Task<byte[]> GenerateRemisionReportPDFAsync(int workOrderDeliveryId);

        
    }
}
