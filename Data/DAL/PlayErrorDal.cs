using Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace Data.DAL
{
    public class PlayErrorDal
    {
        private readonly Context Ctx;

        public PlayErrorDal(Context context)
        {
            Ctx = context;
        }

        public List<PlayError> List()
        {
            List<PlayError> playErrors = (from p in Ctx.PlayErrors
                                          select p).ToList();

            return playErrors;
        }
    }
}
