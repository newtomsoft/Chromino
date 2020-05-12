using System;

namespace Data.Models
{
    public class PrivateMessageLatestRead
    {
        public int Id { get; set; }
        public int RecipientId { get; set; }
        public int SenderId { get; set; }
        public DateTime LatestRead { get; set; }

        public virtual Player Recipient { get; set; }
        public virtual Player Sender { get; set; }
    }
}
