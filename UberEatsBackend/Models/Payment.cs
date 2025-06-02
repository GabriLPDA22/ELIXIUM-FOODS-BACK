using System;

namespace UberEatsBackend.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // "Visa •••• 1234", "PayPal (email)", etc.
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentReference { get; set; }
        public string? FailureReason { get; set; }

        // ✅ ARREGLO: Fechas sin conversión automática
        public DateTime PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ✅ ARREGLO: Eliminada relación OrderId - ahora es Order.PaymentId → Payment.Id
        // public int OrderId { get; set; } // ❌ ELIMINADO
        public Order? Order { get; set; } // ✅ Navigation property (1:1)

        public Payment()
        {
            // ✅ ARREGLO: Asignar fechas UTC sin conversión
            var utcNow = DateTime.UtcNow;
            CreatedAt = utcNow;
            UpdatedAt = utcNow;
            PaymentDate = utcNow;
        }

        // Métodos para cambiar estado
        public void MarkAsCompleted(string? transactionId = null)
        {
            Status = "Completed";
            TransactionId = transactionId;
            PaymentDate = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string? failureReason = null)
        {
            Status = "Failed";
            FailureReason = failureReason;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsRefunded()
        {
            Status = "Refunded";
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
