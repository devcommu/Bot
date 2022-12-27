using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevCommuBot.Data.Models.Users
{
    [Table("StarboardEntry")]
    internal class StarboardEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public ulong AuthorId { get; set; }

        /// <summary>
        /// Original Message Id
        /// </summary>
        public ulong MessageId { get; set; }

        public ulong ChannelId { get; set; }

        /// <summary>
        /// id of the message sent in starboard channel
        /// </summary>
        public ulong StarboardMessageId { get; set; }

        public int Score { get; set; } //Score it gets with reactions

        /// <summary>
        /// When did this message reached enough reaction to be posted in starboard
        /// </summary>
        public DateTime ArrivedTime { get; set; }
    }

    internal enum StarboardEntryStatus
    {
        VALID,
        INVALID, //Used when a message was meant to reach starboard without giving it a real meaning (e.g: post asking to be in starboard)
        DELETED //Used when message do not follow rules
    }
}