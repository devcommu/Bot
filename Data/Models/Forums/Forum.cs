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
        public List<User> Moderators { get; set; } = new();
        public ICollection<ForumRules> Rules { get; set; } = new List<ForumRules>();
        public ForumTag ClosedTag { get; set; }
    }
    internal enum ForumRules {
        NO_IMAGES,
        NO_FILES,
        NO_LINKS,
        NO_MENTIONS,
        NO_EMOJIS,
        NO_FULLCAPS,
        NO_POSTS,
        NO_COMMENTS
    }
    
}
