using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VaporStore.Data.Models
{
    public class GameTag
    {
        [Required]
        [ForeignKey(nameof(Game))]
        public int GameId { get; set; }
        [Required]
        public Game Game { get; set; }
        [Required]
        [ForeignKey(nameof(Tag))]
        public int TagId { get; set; }
        [Required]
        public Tag Tag { get; set; }
    }
}