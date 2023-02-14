using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevCommuBot.Data.Models.Giveaways
{
    [Table("Giveaway")]
    internal class Giveaway
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Giveaway author's id
        /// </summary>
        public ulong AuthorId { get; set; }

        /// <summary>
        /// Message that contains the giveaway
        /// </summary>
        public ulong MessageId { get; set; }

        /// <summary>
        /// Channel where the giveaway was launched!
        /// </summary>
        public ulong ChannelId { get; set; }

        public string MessageDescription { get; set; }
        public string MessageCondition { get; set; } = "";
        public string PromoteLink { get; set; } = "";

        /// <summary>
        /// Condition to enter in this giveaway
        /// </summary>
        public GiveawayCondition Condition { get; set; } = GiveawayCondition.NO_CONDITION;

        public string WonObject { get; set; } = "";

        /// <summary>
        /// How many member can win
        /// </summary>
        public int AmountOfWinners { get; set; }

        /// <summary>
        /// List of ids of members that won
        /// </summary>
        public List<ulong> WinnersId { get; set; } = new List<ulong>();

        /// <summary>
        /// List of ids of members that entered the giveaway
        /// </summary>
        public List<ulong> Participants { get; set; } = new List<ulong>();

        /// <summary>
        /// <see cref="GiveawayState"/> of the giveaway
        /// </summary>
        public GiveawayState State { get; set; } = GiveawayState.NOT_STARTED;

        /// <summary>
        /// When the author created it
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Whern the giveaway should accept entries
        /// </summary>
        public DateTime StartAt { get; set; }

        /// <summary>
        /// When the giveaway should end
        /// </summary>
        public DateTime EndAt { get; set; }
    }

    internal enum GiveawayState
    {
        /// <summary>
        /// The giveaway has not started yet. Default value.
        /// </summary>
        NOT_STARTED,

        RUNNING,

        /// <summary>
        /// When the giveaway ended naturally, can still be runned again
        /// </summary>
        ENDED,

        /// <summary>
        /// In this case the giveaway should be considerated as no longer existings
        /// TODO: Delete giveaway from table
        /// </summary>
        DELETED
    }

    internal enum GiveawayCondition
    {
        /// <summary>
        /// The giveaway do not require any condition. Default Value.
        /// </summary>
        NO_CONDITION
    }
}