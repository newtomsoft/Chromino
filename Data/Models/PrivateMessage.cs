using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class PrivateMessage
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public DateTime Date { get; set; }

        [MaxLength(500)]
        public string Message { get; set; }

        public virtual Player Sender { get; set; }
        public virtual Player Recipient { get; set; }
    }
}
