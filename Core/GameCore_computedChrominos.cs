using Core;
using Data.DAL;
using Data.Models;
using System.Collections.Generic;

namespace Data.Core
{
    public partial class GameCore
    {

        private void RemoveChrominoOnComputedChromino(int botId, int chrominoId)
        {
            ComputedChrominosDal.Remove(GameId, botId, chrominoId);
        }

        private void RemoveChrominoAreaOnComputedChromino(int botId, int chrominoId)
        {
            List<Square> squares = SquareDal.List(GameId);
            
            List<Square> playedSquares = new List<Square> { squares[0], squares[1], squares[2] };



            HashSet<ComputedChromino> computedChrominosToRemove = new HashSet<ComputedChromino>();
            List<ComputedChromino> listComputedChrominosToRemove = new List<ComputedChromino>();

            foreach (var square in playedSquares)
                listComputedChrominosToRemove.AddRange(ComputedChrominoCore.ToDelete(square));

            foreach (var currentChrominoToRemove in listComputedChrominosToRemove)
                computedChrominosToRemove.Add(currentChrominoToRemove);

            ComputedChrominosDal.Remove(GameId, botId, computedChrominosToRemove);


        }

        private void UpdateChrominoAreaOnComputedChromino(int botId, int chrominoId)
        {



        }

        private void UpdateFullAreaOnComputedChromino(int botId)
        {



        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="botId">Id du bot</param>
        /// <param name="chrominoInHand">last chromino draw for compute only with this chromino</param>
        /// <param name="chrominoId">Id du dernier chromino venant d'être posé dans le jeu</param>
        private void UpdateComputedChrominos(int botId = 0, int drawnChrominoId = 0, int chrominoId = 0)
        {
            if (botId == 0)
            {
                foreach (var currentbotId in GamePlayerDal.BotsId(GameId))
                    UpdateComputedChrominos(currentbotId, drawnChrominoId, chrominoId);
            }
            else
            {
                ComputedChrominosDal.Remove(GameId, botId, chrominoId);

                List<Square> squares = SquareDal.List(GameId);
                List<Square> lastSquares = new List<Square> { squares[0], squares[1], squares[2] };

                HashSet<ComputedChromino> ComputedChrominosToRemove = new HashSet<ComputedChromino>();
                List<ComputedChromino> ListComputedChrominosToRemove = new List<ComputedChromino>();

                foreach (var square in lastSquares)
                    ListComputedChrominosToRemove.AddRange(ComputedChrominoCore.ToDelete(square));

                foreach (var currentChrominoToRemove in ListComputedChrominosToRemove)
                    ComputedChrominosToRemove.Add(currentChrominoToRemove);

                ComputedChrominosDal.Remove(GameId, botId, ComputedChrominosToRemove);
                HashSet<Position> positions = ComputePossiblesPositions(squares, lastSquares);

                List<ChrominoInHand> hand;
                if (drawnChrominoId == 0)
                    hand = ChrominoInHandDal.ChrominosByPriority(GameId, botId);
                else
                {
                    ChrominoInHand chrominoInHand = ChrominoInHandDal.InHandDetails(GameId, drawnChrominoId);
                    hand = new List<ChrominoInHand> { chrominoInHand };
                }

                List<ComputedChromino> chrominosFound = new List<ComputedChromino>();
                foreach (ChrominoInHand currentChrominoInHand in hand)
                {
                    HashSet<Position> goodPositions = ComputeChrominoToPlay(currentChrominoInHand, positions);
                    foreach (Position position in goodPositions)
                    {
                        ComputedChromino computedChromino = new ComputedChromino
                        {
                            BotId = botId,
                            GameId = GameId,
                            ChrominoId = currentChrominoInHand.ChrominoId,
                            Orientation = position.Orientation,
                            X = position.Coordinate.X,
                            Y = position.Coordinate.Y,
                        };
                        chrominosFound.Add(computedChromino);
                    }
                }
                ComputedChrominosDal.Add(chrominosFound);
            }
        }
    }
}