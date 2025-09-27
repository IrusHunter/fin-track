using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrack.Models
{
    public class Category
    {
        public const int MaxNameLength = 100;
        public const decimal MaxTaxAmount = 99.99M;
        public const decimal MinTaxAmount = 0M;

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(MaxNameLength)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(4,2)")]
        public decimal TaxAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        public override string ToString()
        {
            return $"category {Id}: {Name}, tax: {TaxAmount}%, created at {CreatedAt}, updated at {UpdatedAt}" + (DeletedAt != null ? $", deleted at {DeletedAt}" : "");
        }
    }
}