using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrack.Models
{
    public class Transaction
    {
        public const int MaxNameLength = 100;

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Sum { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SumAfterTax { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public override string ToString()
        {
            return $"transaction {Id}: {Name}, sum: {Sum}, sum after tax: {SumAfterTax}, category id: {CategoryId}, created at {CreatedAt}, updated at {UpdatedAt}";
        }
    }
}