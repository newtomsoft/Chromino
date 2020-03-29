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

        private void RemoveAndUpdateChrominoAreaOnComputedChromino()
        {
            List<Square> squares = SquareDal.List(GameId);
            List<Square> playedSquares = new List<Square> { squares[0], squares[1], squares[2] };

            HashSet<ComputedChromino> computedChrominosToRemove = new HashSet<ComputedChromino>();
            List<ComputedChromino> listComputedChrominosToRemove = new List<ComputedChromino>();
            foreach (var square in playedSquares)
                listComputedChrominosToRemove.AddRange(ComputedChrominoCore.ToDelete(square));
            foreach (var currentChrominoToRemove in listComputedChrominosToRemove)
                computedChrominosToRemove.Add(currentChrominoToRemove);

            HashSet<Position> possiblesPositions = ComputePossiblesPositions(squares, playedSquares);

            foreach (var botId in GamePlayerDal.BotsId(GameId))
            {
                ComputedChrominosDal.Remove(GameId, botId, computedChrominosToRemove);

                List<ChrominoInHand> hand = ChrominoInHandDal.ChrominosByPriority(GameId, botId);
                List<ComputedChromino> chrominosFound = new List<ComputedChromino>();
                foreach (ChrominoInHand chrominoInHand in hand)
                {
                    HashSet<Position> goodPositions = ComputeChrominoToPlay(chrominoInHand, possiblesPositions);
                    foreach (Position position in goodPositions)
                    {
                        ComputedChromino computedChromino = new ComputedChromino
                        {
                            BotId = botId,
                            GameId = GameId,
                            ChrominoId = chrominoInHand.ChrominoId,
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

        private void UpdateFullAreaOnComputedChromino(int botId, int chrominoId)
        {
            List<Square> squares = SquareDal.List(GameId);
            List<Square> playedSquares = new List<Square> { squares[0], squares[1], squares[2] };

            HashSet<Position> positions = ComputePossiblesPositions(squares, playedSquares);
            ChrominoInHand chrominoInHand = ChrominoInHandDal.Details(GameId, chrominoId);
            List<ComputedChromino> chrominosFound = new List<ComputedChromino>();
            HashSet<Position> goodPositions = ComputeChrominoToPlay(chrominoInHand, positions);
            foreach (Position position in goodPositions)
            {
                ComputedChromino computedChromino = new ComputedChromino
                {
                    BotId = botId,
                    GameId = GameId,
                    ChrominoId = chrominoInHand.ChrominoId,
                    Orientation = position.Orientation,
                    X = position.Coordinate.X,
                    Y = position.Coordinate.Y,
                };
                chrominosFound.Add(computedChromino);
            }
            ComputedChrominosDal.Add(chrominosFound);
        }
    }
}