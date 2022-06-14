using System.ComponentModel.DataAnnotations.Schema;

using DevCommuBot.Data.Models.Warnings;

namespace DevCommuBot.Data.Models.Users
{
    [Table("UserWarning")]
    internal class UserWarning
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int WarningId { get; set; }
        public Warning Warning { get; set; }
    }
}