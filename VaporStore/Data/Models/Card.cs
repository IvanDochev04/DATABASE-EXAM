using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using VaporStore.Data.Models.Enums;

namespace VaporStore.Data.Models
{
    public class Card
    {
        public Card()
        {
            this.Purchases = new HashSet<Purchase>();
        }
        [Key]
        public int Id { get; set; }
        [Required]
        public string Number { get; set; }
        [Required]
        [MaxLength(3)]
        public string Cvc { get; set; }
        [Required]
        public CardType Type { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public User User { get; set; }
        public virtual ICollection<Purchase> Purchases { get; set; }
    }
}