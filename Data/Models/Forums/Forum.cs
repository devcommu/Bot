using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using DevCommuBot.Data.Models.Users;

using Discord;

namespace DevCommuBot.Data.Models.Forums
{
    [Table("Forum")]
    internal class Forum
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public ulong ChannelId { get; set; }

        public string MessageDescription { get; set; } = ""; //Will be posted after each post if not null
        public List<User> Moderators { get; set; } = new(); //List of moderators
        public List<ForumEntry> Entries { get; set; } = new();
        public ForumTag ClosedTag { get; set; }
    }

}