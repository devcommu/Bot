using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using DevCommuBot.Data.Models.Users.Tiers;

using Newtonsoft.Json;

namespace DevCommuBot.Data.Models.Users
{
    [Table("User")]
    internal class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public ulong UserId { get; set; }

        public List<UserWarning> Warnings { get; set; } = new();

        public int Points { get; set; } = 0;
        public TiersEnum Tier { get; set; } = TiersEnum.NO_TIER; //Class(Table) or Enum?

        //Next soon?
        public List<StarboardEntry> StarboardEntries = new();

        public bool DisplayPartnerAds { get; set; } = true;

        /// <summary>
        /// The object that represents advantage of boosting
        /// </summary>
        public BoosterAdvantage? BoosterAdvantage { get; set; } = null;
    }
    [JsonObject]
    internal class BoosterAdvantage 
    {
        public ulong? RoleId { get; set; }
        public ulong? VocalId { get; set; }
        public ulong? EmoteId { get; set; }
        public DateTime Since { get; set; }
    }
}