using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using DevCommuBot.Data.Models.Users;

namespace DevCommuBot.Data.Models.Forums
{
    [Table("Forum")]
    internal class Forum
    {
        /// <summary>
        /// Forum's internal ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Forum's main topic
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Forum's name with spaces or other
        /// </summary>
        public string DisplayName { get; set; }

        public ulong ChannelId { get; set; }

        /// <summary>
        /// Message that will be posted after a creation of a post
        /// </summary>
        public string MessageDescription { get; set; } = "";

        public List<User> Moderators { get; set; } = new(); //List of moderators

        /// <summary>
        /// Posts in the forum
        /// </summary>
        public List<ForumEntry> Entries { get; set; } = new();
    }
}