using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevCommuBot.Data.Models.Warnings
{
    [Table("Warning")]
    internal class Warning
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public ulong AuthorId { get; set; } //Moderator id
        public WarningReason Reason { get; set; } = WarningReason.NO_REASON; //Registered Reason
        public string Details { get; set; } = null; //If Moderator wants to add details!
        public DateTime Created { get; set; }
    }
}