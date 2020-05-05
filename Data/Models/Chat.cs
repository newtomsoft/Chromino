using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public int GamePlayerId { get; set; }
        public DateTime Date { get; set; }

        [MaxLength(500)]
        public string Message { get; set; }

        public virtual GamePlayer GamePlayer { get; set; }
    }
}
