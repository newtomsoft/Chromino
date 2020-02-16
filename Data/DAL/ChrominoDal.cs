using Data.Core;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Data.Enumeration;

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

        public void CreateChrominos()
        {
            if (!Ctx.Chrominos.Any())
            {
                List<Chromino> chrominos = new List<Chromino>
                {
                    new Chromino { FirstColor = Color.Purple, SecondColor = Color.Cameleon, ThirdColor = Color.Red, Points = 3},
                    new Chromino { FirstColor = Color.Purple, SecondColor = Color.Cameleon, ThirdColor = Color.Yellow, Points = 3 },
                    new Chromino { FirstColor = Color.Green, SecondColor = Color.Cameleon, ThirdColor = Color.Red, Points = 3 },
                    new Chromino { FirstColor = Color.Green, SecondColor = Color.Cameleon, ThirdColor = Color.Blue, Points = 3 },
                    new Chromino { FirstColor = Color.Yellow, SecondColor = Color.Cameleon, ThirdColor = Color.Blue, Points = 3 }
                };
                foreach (Color firstSquare in (Color[])Enum.GetValues(typeof(Color)))
                {
                    if (firstSquare == Color.Cameleon)
                        continue;
                    foreach (Color secondSquare in (Color[])Enum.GetValues(typeof(Color)))
                    {
                        if (secondSquare == Color.Cameleon)
                            continue;
                        foreach (Color thirdSquare in (Color[])Enum.GetValues(typeof(Color)))
                        {
                            if (thirdSquare == Color.Cameleon)
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

        public List<Chromino> PlayerChrominos(int gameId, int playerId)
        {
            var chrominos = (from c in Ctx.Chrominos
                             join gc in Ctx.ChrominosInHand on c.Id equals gc.ChrominoId
                             where gc.GameId == gameId && gc.PlayerId == playerId
                             select c).ToList();

            return chrominos;
        }

        public bool IsCameleon(int id)
        {
            int idC = (from c in Ctx.Chrominos
                      where c.Id == id && c.SecondColor == Color.Cameleon
                      select c.Id).FirstOrDefault();
            
            return idC != 0 ? true : false;
        }
    }
}
