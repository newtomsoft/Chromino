namespace Data.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int ContactPlayerId { get; set; }

        public virtual Player Player { get; set; }
        public virtual Player ContactPlayer { get; set; }
    }
}
