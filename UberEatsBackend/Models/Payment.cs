using System;

namespace UberEatsBackend.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // card, cash, paypal, etc.
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentReference { get; set; }
        public string? FailureReason { get; set; }

        private DateTime _paymentDate;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public DateTime PaymentDate
        {
            get => _paymentDate;
            set => _paymentDate = value.Kind == DateTimeKind.Unspecified ?
                DateTime.SpecifyKind(value, DateTimeKind.Utc) :
                value.ToUniversalTime();
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => _createdAt = value.Kind == DateTimeKind.Unspecified ?
                DateTime.SpecifyKind(value, DateTimeKind.Utc) :
                value.ToUniversalTime();
        }

        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set => _updatedAt = value.Kind == DateTimeKind.Unspecified ?
                DateTime.SpecifyKind(value, DateTimeKind.Utc) :
                value.ToUniversalTime();
        }

        // Relación con Order
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public Payment()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            PaymentDate = DateTime.UtcNow;
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
