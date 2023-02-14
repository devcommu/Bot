using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using DevCommuBot.Data.Models.Users;

namespace DevCommuBot.Data.Models.Forums
{
    [Table("ForumEntry")]
    internal class ForumEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Post's author
        /// </summary>
        public User Author { get; set; }

        /// <summary>
        /// Is the post locked?
        /// </summary>
        public bool IsLocked { get; set; } = false;

        /// <summary>
        /// If post solved indeed
        /// </summary>
        public ulong? SolvingMessageId { get; set; } = null;

        /// <summary>
        /// When was it posted
        /// </summary>
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

        /// <summary>
        /// Discord invite link will be allowed here
        /// </summary>
        ALLOW_ADS,

        /// <summary>
        /// No one should be able to talk after that
        /// </summary>
        CLOSED
    }
}