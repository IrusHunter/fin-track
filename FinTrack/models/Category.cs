using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FinTrack.Models
{
    /// <summary>
    /// Defines the type of tax applied to a financial category.
    /// Determines how the <see cref="Category.TaxAmount"/> is interpreted.
    /// </summary>
    public enum TaxType
    {
        /// <summary>
        /// A general tax applied universally, regardless of category expenses.
        /// Typically represents standard taxation rules.
        /// </summary>
        GeneralTax = 0,

        /// <summary>
        /// A tax specifically associated with expenses.
        /// Applied only to spending-related categories.
        /// </summary>
        ExpenseTax = 1,
    }


    /// <summary>
    /// Represents a financial category in the FinTrack system.
    /// Categories can have transactions associated with them and a tax amount.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Maximum allowed length for the category name.
        /// </summary>
        public const int MaxNameLength = 100;

        /// <summary>
        /// Maximum allowed tax amount for a category.
        /// </summary>
        public const decimal MaxTaxAmount = 99.99M;

        /// <summary>
        /// Minimum allowed tax amount for a category.
        /// </summary>
        public const decimal MinTaxAmount = 0M;

        /// <summary>
        /// Primary key identifier for the category.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Name of the category. Required and limited to <see cref="MaxNameLength"/> characters.
        /// </summary>
        [Required]
        [MaxLength(MaxNameLength)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tax amount associated with the category, stored as a decimal with precision (4,2).
        /// </summary>
        [Column(TypeName = "decimal(4,2)")]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Type of tax applied to the category.
        /// Determines how the <see cref="TaxAmount"/> value is interpreted
        /// (e.g., as a general tax or as an expense-specific tax).
        /// </summary>
        public TaxType TaxType { get; set; }

        /// <summary>
        /// Date and time when the category was created, in UTC.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date and time when the category was last updated, in UTC.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional date and time when the category was deleted, in UTC. Null if not deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Collection of transactions associated with this category.
        /// </summary>
        [JsonIgnore]
        public ICollection<Transaction> Transactions { get; set; } = [];

        /// <summary>
        /// Returns a string representation of the category, including its ID, name, tax, creation and update dates, 
        /// and deletion date if applicable.
        /// </summary>
        /// <returns>String describing the category.</returns>
        public override string ToString()
        {
            return $"category {Id}: {Name}, tax: {TaxAmount}%, created at {CreatedAt}, updated at {UpdatedAt}" + (DeletedAt != null ? $", deleted at {DeletedAt}" : "");
        }
    }
}