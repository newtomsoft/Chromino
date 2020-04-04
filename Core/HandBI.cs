using Data;
using Data.DAL;
using Data.Models;
using System.Collections.Generic;

namespace ChrominoBI
{
    public class HandBI
    {
        /// <summary>
        /// nombre de chromino dans la main des joueurs en début de partie
        /// </summary>
        public const int StartChrominosNumber = 8;
        private List<ChrominoInHand> ChrominosInHand { get; set; }

        /// <summary>
        /// les différentes Dal utilisées du context 
        /// </summary>
        private readonly ChrominoDal ChrominoDal;

        public HandBI(Context ctx, List<ChrominoInHand> chrominosInHand)
        {
            ChrominosInHand = chrominosInHand;
            ChrominoDal = new ChrominoDal(ctx);
        }


        /// <summary>
        /// indique le chromino non cameleon de la main s'il est seul avec que des cameleons
        /// </summary>
        /// <param name="hand">référence de la liste des chrominos de la main du joueur</param>
        /// <returns>id du chromino non caméléon, 0 sinon</returns>
        public int ChrominoIdIfSingleWithCameleons()
        {
            int notCameleonNumber = 0;
            int indexFound = -1;
            if (ChrominosInHand.Count >= 2)
            {
                for (int i = 0; i < ChrominosInHand.Count; i++)
                {
                    if (!ChrominoDal.IsCameleon(ChrominosInHand[i].ChrominoId))
                    {
                        if (++notCameleonNumber > 1)
                            break;
                        indexFound = i;
                    }
                }
            }
            if (notCameleonNumber == 1)
                return ChrominosInHand[indexFound].ChrominoId;
            else
                return 0;
        }
    }
}
