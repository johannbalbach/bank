using Bank.DAL.Models;
using Core.DAL.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace Core.DAL.Models
{
    public class User : BaseDeletableEntity
    {
        [Required]
        public string FullName { get; set; }

        /// <summary>
        /// BANK ACCOUNTS
        /// </summary>
        public ICollection<BaseBankAccount> BankAccounts { get; set; }

        /// <summary>
        /// CARDS
        /// </summary>
        public ICollection<Card> Cards { get; set; }
    }
}
