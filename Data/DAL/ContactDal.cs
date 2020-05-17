using Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace Data.DAL
{
    public class ContactDal
    {
        private readonly Context Ctx;
        private readonly PlayerDal PlayerDal;

        public ContactDal(Context context)
        {
            Ctx = context;
            PlayerDal = new PlayerDal(Ctx);
        }

        /// <summary>
        /// Ajoute un contact à un joueur
        /// </summary>
        /// <param name="playerId">id du joueur à qui ajouter le contact</param>
        /// <param name="contactId">id du contact à ajouter</param>
        public void Add(int playerId, int contactId)
        {
            if (PlayerDal.IsBot(playerId) || PlayerDal.IsBot(contactId) || playerId == contactId)
                return;

            Contact contact = (from c in Ctx.Contacts
                               where c.PlayerId == playerId && c.ContactPlayerId == contactId
                               select c).FirstOrDefault();

            if (contact == null)
            {
                Ctx.Contacts.Add(new Contact { PlayerId = playerId, ContactPlayerId = contactId });
                Ctx.SaveChanges();
            }
        }

        public List<int> List(int playerId)
        {
            return (from c in Ctx.Contacts
                    where c.PlayerId == playerId
                    select c.ContactPlayerId).ToList();
        }

        public object ContactsId_Names(int playerId)
        {
            return (from c in Ctx.Contacts
                    join p in Ctx.Players on c.ContactPlayerId equals p.Id
                    where c.PlayerId == playerId
                    select new { id = p.Id, name = p.UserName }).ToList();
        }
    }
}