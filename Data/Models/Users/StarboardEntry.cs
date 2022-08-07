using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevCommuBot.Data.Models.Users
{
    [Table("StarboardEntry")]
    internal class StarboardEntry
    {
        [Key]
        public int Id { get; set; }
        public ulong AuthorId { get; set; }
        public string MessageContent { get; set; } = null;
        public int Score { get; set; } //Score it gets with reactions

        public DateTime ArrivedTime { get; set; }
    }
}
