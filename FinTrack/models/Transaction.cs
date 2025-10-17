using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrack.Models
{
    /// <summary>
    /// Represents a financial transaction in the FinTrack system.
    /// Each transaction has a name, amount, category, and timestamps.
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Maximum allowed length for the transaction name.
        /// </summary>
        public const int MaxNameLength = 100;

        /// <summary>
        /// Primary key identifier for the transaction.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Name of the transaction. Required and limited to <see cref="MaxNameLength"/> characters.
        /// </summary>
        [Required]
        [MaxLength(MaxNameLength)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Transaction amount before applying taxes, stored as a decimal with precision (18,2).
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Sum { get; set; }

        /// <summary>
        /// Transaction amount after applying taxes, stored as a decimal with precision (18,2).
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal SumAfterTax { get; set; }

        /// <summary>
        /// Date and time when the transaction was created, in UTC.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date and time when the transaction was last updated, in UTC.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Foreign key referencing the related category.
        /// </summary>
        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }
        /// <summary>
        /// Navigation property to the category associated with this transaction.
        /// </summary>
        public Category? Category { get; set; } = null!;

        /// <summary>
        /// Returns a string representation of the transaction, including ID, name, sums, category, and timestamps.
        /// </summary>
        /// <returns>String describing the transaction.</returns>
        public override string ToString()
        {
            return $"transaction {Id}: {Name}, sum: {Sum}, sum after tax: {SumAfterTax}, category id: {CategoryId}, created at {CreatedAt}, updated at {UpdatedAt}";
        }
    }
}