using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using DevCommuBot.Data.Models.Users;
using System;

namespace DevCommuBot.Data.Models.Forums
{
    [Table("ForumEntry")]
    internal class ForumEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public User Author { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
    internal enum ForumRules
    {
        NO_IMAGES,
        NO_FILES,
        NO_LINKS,
        NO_MENTIONS,
        NO_EMOJIS,
        NO_FULLCAPS,
        NO_COMMENTS,
        ALLOW_ADS,
        CLOSED
    }
}
