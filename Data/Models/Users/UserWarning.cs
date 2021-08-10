using DevCommuBot.Data.Models.Warnings;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevCommuBot.Data.Models.Users
{
    [Table("UserWarning")]
    class UserWarning
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int WarningId { get; set; }
        public Warning Warning { get; set; }

    }
}
