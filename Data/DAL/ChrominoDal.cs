using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.DAL
{
    public class ChrominoDal
    {
        private readonly Context Ctx;

        public ChrominoDal(Context context)
        {
            Ctx = context;
        }

        public Chromino Details(int id)
        {
            var result = (from chrominos in Ctx.Chrominos
                          where chrominos.Id == id
                          select chrominos).FirstOrDefault();

            return result;
        }

        public int Points(int id)
        {
            int points = (from c in Ctx.Chrominos
                          where c.Id == id
                          select c.Points).FirstOrDefault();

            return points;
        }

        /// <summary>
        /// Création des chrominos. Utilisé qu'une seule fois après création de la base de données et servira ensuite pour chaque partie
        /// </summary>
        public void CreateChrominos()
        {
            if (Ctx.Chrominos.Count() != 80)
            {
                List<Chromino> chrominos = new List<Chromino>
                {
                    new Chromino { FirstColor = ColorCh.Purple, SecondColor = ColorCh.Cameleon, ThirdColor = ColorCh.Red, Points = 3},
                    new Chromino { FirstColor = ColorCh.Purple, SecondColor = ColorCh.Cameleon, ThirdColor = ColorCh.Yellow, Points = 3 },
                    new Chromino { FirstColor = ColorCh.Green, SecondColor = ColorCh.Cameleon, ThirdColor = ColorCh.Red, Points = 3 },
                    new Chromino { FirstColor = ColorCh.Green, SecondColor = ColorCh.Cameleon, ThirdColor = ColorCh.Blue, Points = 3 },
                    new Chromino { FirstColor = ColorCh.Yellow, SecondColor = ColorCh.Cameleon, ThirdColor = ColorCh.Blue, Points = 3 }
                };
                foreach (ColorCh firstSquare in (ColorCh[])Enum.GetValues(typeof(ColorCh)))
                {
                    if (IsNotGoodColor(firstSquare))
                        continue;
                    foreach (ColorCh secondSquare in (ColorCh[])Enum.GetValues(typeof(ColorCh)))
                    {
                        if (IsNotGoodColor(secondSquare))
                            continue;
                        foreach (ColorCh thirdSquare in (ColorCh[])Enum.GetValues(typeof(ColorCh)))
                        {
                            if (IsNotGoodColor(thirdSquare))
                                continue;
                            if (thirdSquare >= firstSquare)
                            {
                                int points;
                                if (firstSquare == secondSquare && firstSquare == thirdSquare)
                                    points = 1;
                                else if (firstSquare != secondSquare && firstSquare != thirdSquare && secondSquare != thirdSquare)
                                    points = 3;
                                else
                                    points = 2;

                                chrominos.Add(new Chromino { FirstColor = firstSquare, SecondColor = secondSquare, ThirdColor = thirdSquare, Points = points });
                            }
                        }
                    }
                }
                Ctx.Chrominos.AddRange(chrominos);
                Ctx.SaveChanges();
            }
        }

        /// <summary>
        /// liste des chrominos de la main du joueur
        /// </summary>
        /// <param name="gameId">Id du jeu</param>
        /// <param name="playerId">Id du joueur</param>
        /// <returns></returns>
        public List<Chromino> PlayerChrominos(int gameId, int playerId)
        {
            var chrominos = (from c in Ctx.Chrominos
                             join gc in Ctx.ChrominosInHand on c.Id equals gc.ChrominoId
                             where gc.GameId == gameId && gc.PlayerId == playerId
                             select c).ToList();

            return chrominos;
        }

        /// <summary>
        /// indique si le chromino est de type Cameleon
        /// </summary>
        /// <param name="id">Id du Chromino</param>
        /// <returns>true si le chromino est de type Cameleon</returns>
        public bool IsCameleon(int id)
        {
            int idC = (from c in Ctx.Chrominos
                       where c.Id == id && c.SecondColor == ColorCh.Cameleon
                       select c.Id).FirstOrDefault();

            return idC != 0 ? true : false;
        }

        /// <summary>
        /// indique si la couleur passée n'est pas une bonne couleur (pour fabrication des chrominos classiques) 
        /// </summary>
        /// <param name="color"></param>
        /// <returns>true si color est Camelelon ou None</returns>
        private bool IsNotGoodColor(ColorCh color)
        {
            return color == ColorCh.Cameleon || color == ColorCh.None;
        }
    }
}
