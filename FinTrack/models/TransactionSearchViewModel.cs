using FinTrack.Models;
using System.ComponentModel.DataAnnotations;

namespace FinTrack.Models.ViewModels
{
    public class TransactionSearchViewModel
    {
        [DataType(DataType.DateTime)]
        [Display(Name = "Дата від")]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Дата до")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Категорії")]
        public List<int>? CategoryIds { get; set; }

        [Display(Name = "Назва починається з")]
        public string? NameStart { get; set; }

        [Display(Name = "Назва закінчується на")]
        public string? NameEnd { get; set; }

        [Display(Name = "Тип податку категорії")]
        public TaxType? CategoryTaxType { get; set; }

        [Display(Name = "Ім'я користувача")]
        public string? UserNameFilter { get; set; }

        public IEnumerable<Transaction>? Transactions { get; set; }
    }
}