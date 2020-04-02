using Data;
using Data.Core;
using Data.DAL;
using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;

namespace Core
{
    public class ComputedChrominoCore
    {
        /// <summary>
        /// Id du jeu
        /// </summary>
        private int GameId { get; set; }

        /// <summary>
        /// les différentes Dal utilisées du context 
        /// </summary>
        private readonly ChrominoInHandDal ChrominoInHandDal;
        private readonly ChrominoDal ChrominoDal;
        private readonly SquareDal SquareDal;
        private readonly GamePlayerDal GamePlayerDal;
        private readonly ComputedChrominosDal ComputedChrominosDal;

        public ComputedChrominoCore(Context ctx, int gameId)
        {
            GameId = gameId;
            ChrominoInHandDal = new ChrominoInHandDal(ctx);
            ChrominoDal = new ChrominoDal(ctx);
            SquareDal = new SquareDal(ctx);
            GamePlayerDal = new GamePlayerDal(ctx);
            ComputedChrominosDal = new ComputedChrominosDal(ctx);
        }

        public void RemoveCandidate(int? botId, int chrominoId)
        {
            ComputedChrominosDal.Remove(GameId, botId, chrominoId);
        }

        public void RemoveAndUpdateCandidatesFromChrominosPlayed(bool remove = true, int botId = 0, bool allChrominos = false, int chrominoId = 0)
        {
            List<Square> squares = SquareDal.List(GameId);
            int chrominoNumber;
            if (allChrominos)
                chrominoNumber = squares.Count / 3;
            else
                chrominoNumber = 1;

            for (int iSquare = (chrominoNumber - 1) * 3; iSquare >= 0; iSquare -= 3)
            {
                List<Position> positionsToDelete;
                if (remove)
                    positionsToDelete = PositionsToDeleteAroundSquares(squares, iSquare);
                else
                    positionsToDelete = new List<Position>();

                List<Position> candidatesPositions = CandidatesPositionsAroundSquares(squares, iSquare);

                List<int> botsId;
                if (botId == 0)
                    botsId = GamePlayerDal.BotsId(GameId);
                else
                    botsId = new List<int> { botId };

                foreach (var currentBotId in botsId)
                {
                    List<int> chrominosId = new List<int>();
                    if (chrominoId == 0)
                        foreach (ChrominoInHand chrominoInHand in ChrominoInHandDal.ChrominosByPriority(GameId, currentBotId))
                            chrominosId.Add(chrominoInHand.ChrominoId);
                    else
                        chrominosId.Add(chrominoId);
                    
                    if (remove)
                        ComputedChrominosDal.Remove(GameId, currentBotId, positionsToDelete, chrominoId);

                    List<ComputedChromino> chrominosFound = new List<ComputedChromino>();
                    foreach (int currentChrominoId in chrominosId)
                    {
                        List<Position> goodPositions = PositionsOkForChromino(currentChrominoId, candidatesPositions);
                        foreach (Position position in goodPositions)
                        {
                            ComputedChromino computedChromino = new ComputedChromino
                            {
                                GameId = GameId,
                                BotId = currentBotId,
                                X = position.Coordinate.X,
                                Y = position.Coordinate.Y,
                                ChrominoId = currentChrominoId,
                                Orientation = position.Orientation,
                                Flip = position.Reversed,
                            };
                            chrominosFound.Add(computedChromino);
                        }
                    }
                    ComputedChrominosDal.Add(chrominosFound);
                }
            }
        }

        public void UpdateCandidatesFromAllGame(int botId, int chrominoId)
        {
            List<Square> squares = SquareDal.List(GameId);
            List<Position> candidatesPositions = CandidatesPositionsAroundAllSquares(squares);
            List<ComputedChromino> chrominosFound = new List<ComputedChromino>();
            List<Position> goodPositions = PositionsOkForChromino(chrominoId, candidatesPositions);
            foreach (Position position in goodPositions)
            {
                ComputedChromino computedChromino = new ComputedChromino
                {
                    GameId = GameId,
                    BotId = botId,
                    X = position.Coordinate.X,
                    Y = position.Coordinate.Y,
                    ChrominoId = chrominoId,
                    Orientation = position.Orientation,
                    Flip = position.Reversed,
                };
                chrominosFound.Add(computedChromino);
            }
            ComputedChrominosDal.Add(chrominosFound);

        }

        [Obsolete("Method to delete in future")]
        public void ResetAndUpdateCandidatesFromAllChrominosPlayed(int botId)
        {
            foreach (ChrominoInHand chrominoInHand in ChrominoInHandDal.ChrominosByPriority(GameId, botId))
                RemoveAndUpdateCandidatesFromChrominosPlayed(true, botId, true, chrominoInHand.ChrominoId);
        }

        //public void UpdateCandidatesFromAllChrominosPlayed(int botId, int chrominoId)
        //{
        //    List<Square> squares = SquareDal.List(GameId);
        //    HashSet<Position> positions = ComputePossiblesPositions(squares);
        //    List<ComputedChromino> chrominosFound = new List<ComputedChromino>();
        //    HashSet<Position> goodPositions = PositionsOkForChromino(chrominoId, positions);
        //    foreach (Position position in goodPositions)
        //    {
        //        ComputedChromino computedChromino = new ComputedChromino
        //        {
        //            BotId = botId,
        //            GameId = GameId,
        //            ChrominoId = chrominoId,
        //            Orientation = position.Orientation,
        //            X = position.Coordinate.X,
        //            Y = position.Coordinate.Y,
        //        };
        //        chrominosFound.Add(computedChromino);
        //    }
        //    ComputedChrominosDal.Add(chrominosFound);
        //}

        /// <summary>
        /// retourne les positions possibles où peuvent être joués des chrominos
        /// </summary>
        /// <param name="occupiedSquares">liste complète des squares occupés</param>
        /// <param name="testedSquares">liste des squares définissant la zone à rechercher</param>
        /// <returns>Flip toujours à false</returns>
        public HashSet<Position> ComputePossiblesPositionsForOpponent(List<Square> occupiedSquares, List<Square> testedSquares = null)
        {
            if (testedSquares == null)
                testedSquares = occupiedSquares;
            HashSet<Coordinate> coordinates = FreeAroundSquares(occupiedSquares, testedSquares);
            HashSet<Position> positions = new HashSet<Position>();

            foreach (Coordinate firstCoordinate in coordinates)
            {
                ColorCh? firstColor = firstCoordinate.OkColorFor(occupiedSquares, out int commonSidesFirstColor);
                if (firstColor != null)
                {
                    //cherche placement possible d'un square
                    foreach (Orientation orientation in (Orientation[])Enum.GetValues(typeof(Orientation)))
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            bool flip = i == 0 ? false : true;
                            Coordinate offset = new Coordinate(orientation, flip);
                            Coordinate secondCoordinate = firstCoordinate + offset;
                            Coordinate thirdCoordinate = secondCoordinate + offset;

                            if (secondCoordinate.IsFree(occupiedSquares) && thirdCoordinate.IsFree(occupiedSquares))
                            {
                                //calcul si chromino posable et dans quelle position
                                ColorCh? secondColor = secondCoordinate.OkColorFor(occupiedSquares, out int adjacentChrominoSecondColor);
                                ColorCh? thirdColor = thirdCoordinate.OkColorFor(occupiedSquares, out int adjacentChrominosThirdColor);

                                if (secondColor != null && thirdColor != null && commonSidesFirstColor + adjacentChrominoSecondColor + adjacentChrominosThirdColor >= 2)
                                {
                                    Position possibleSpace = new Position
                                    {
                                        Coordinate = firstCoordinate,
                                        Reversed = flip,
                                        Orientation = orientation,
                                        FirstColor = firstColor,
                                        SecondColor = secondColor,
                                        ThirdColor = thirdColor,
                                    };
                                    positions.Add(possibleSpace);
                                }
                            }
                        }
                    }
                }
            }
            return positions;
        }

        ///// <summary>
        ///// Indique si avec les chrominos passés en paramètre, il est possible de jouer un de ces chrominos et le renvoie
        ///// </summary>
        ///// <param name="hand">liste des chrominos à tester</param>
        ///// <param name="previouslyDraw">indique si le joueur vient de piocher avant de tester</param>
        ///// <param name="positions">liste des positions où chercher à placer le chromino</param>
        ///// <param name="indexInHand">index du chromino convenant</param>
        ///// <param name="position">position si convenant, null sinon</param>
        ///// <returns>null si aucun chromino ne couvient</returns>
        //public ChrominoInGame ComputeChrominoToPlay(List<ChrominoInHand> hand, bool previouslyDraw, HashSet<Position> positions, out int indexInHand, out Position position)
        //{
        //    ChrominoInGame goodChromino = null;
        //    Coordinate firstCoordinate;
        //    ChrominoInHand goodChrominoInHand;
        //    indexInHand = -1;
        //    position = null;
        //    if (!previouslyDraw && hand.Count == 1 && ChrominoDal.IsCameleon(hand[0].ChrominoId)) // on ne peut pas poser un cameleon si c'est le dernier de la main
        //    {
        //        goodChromino = null;
        //    }
        //    else
        //    {
        //        SortHandToFinishedWithoutCameleon(ref hand);
        //        foreach (ChrominoInHand chrominoInHand in hand)
        //        {
        //            indexInHand++;
        //            foreach (Position currentPosition in positions)
        //            {
        //                Chromino chromino = ChrominoDal.Details(chrominoInHand.ChrominoId);
        //                if ((chromino.FirstColor == currentPosition.FirstColor || currentPosition.FirstColor == ColorCh.Cameleon) && (chromino.SecondColor == currentPosition.SecondColor || chromino.SecondColor == ColorCh.Cameleon || currentPosition.SecondColor == ColorCh.Cameleon) && (chromino.ThirdColor == currentPosition.ThirdColor || currentPosition.ThirdColor == ColorCh.Cameleon))
        //                {
        //                    goodChrominoInHand = chrominoInHand;
        //                    firstCoordinate = currentPosition.Coordinate;
        //                    goodChromino = new ChrominoInGame()
        //                    {
        //                        ChrominoId = chromino.Id,
        //                        GameId = GameId,
        //                        XPosition = firstCoordinate.X,
        //                        YPosition = firstCoordinate.Y,
        //                        Orientation = currentPosition.Orientation,
        //                        Flip = false,
        //                    };
        //                    position = currentPosition;
        //                    break;
        //                }
        //                else if ((chromino.FirstColor == currentPosition.ThirdColor || currentPosition.ThirdColor == ColorCh.Cameleon) && (chromino.SecondColor == currentPosition.SecondColor || chromino.SecondColor == ColorCh.Cameleon || currentPosition.SecondColor == ColorCh.Cameleon) && (chromino.ThirdColor == currentPosition.FirstColor || currentPosition.FirstColor == ColorCh.Cameleon))
        //                {
        //                    goodChrominoInHand = chrominoInHand;
        //                    Coordinate offset = new Coordinate(currentPosition.Orientation);
        //                    firstCoordinate = currentPosition.Coordinate + 2 * offset;

        //                    goodChromino = new ChrominoInGame()
        //                    {
        //                        GameId = GameId,
        //                        ChrominoId = chromino.Id,
        //                        XPosition = firstCoordinate.X,
        //                        YPosition = firstCoordinate.Y,
        //                        Orientation = currentPosition.Orientation,
        //                        Flip = true,
        //                    };
        //                    position = currentPosition;
        //                    break;
        //                }
        //            }
        //            if (goodChromino != null)
        //                break;
        //        }
        //    }
        //    return goodChromino;
        //}
        public HashSet<Position> PositionsOkForOpponentChromino(int chrominoId, HashSet<Position> positions)
        {
            Chromino chromino = ChrominoDal.Details(chrominoId);
            HashSet<Position> goodPositions = new HashSet<Position>();
            foreach (Position currentPosition in positions)
            {
                if ((chromino.FirstColor == currentPosition.FirstColor || currentPosition.FirstColor == ColorCh.Cameleon) && (chromino.SecondColor == currentPosition.SecondColor || chromino.SecondColor == ColorCh.Cameleon || currentPosition.SecondColor == ColorCh.Cameleon) && (chromino.ThirdColor == currentPosition.ThirdColor || currentPosition.ThirdColor == ColorCh.Cameleon))
                    goodPositions.Add(currentPosition);

                if (chromino.FirstColor != chromino.ThirdColor && (chromino.FirstColor == currentPosition.ThirdColor || currentPosition.ThirdColor == ColorCh.Cameleon) && (chromino.SecondColor == currentPosition.SecondColor || chromino.SecondColor == ColorCh.Cameleon || currentPosition.SecondColor == ColorCh.Cameleon) && (chromino.ThirdColor == currentPosition.FirstColor || currentPosition.FirstColor == ColorCh.Cameleon))
                {
                    currentPosition.Reversed = true;
                    goodPositions.Add(currentPosition);
                }
            }
            return goodPositions;
        }

        public List<Position> PositionsOkForChromino(int chrominoId, List<Position> positions)
        {
            Chromino chromino = ChrominoDal.Details(chrominoId);
            List<Position> goodPositions = new List<Position>();
            foreach (Position currentPosition in positions)
            {
                if ((chromino.FirstColor == currentPosition.FirstColor || currentPosition.FirstColor == ColorCh.Cameleon) && (chromino.SecondColor == currentPosition.SecondColor || chromino.SecondColor == ColorCh.Cameleon || currentPosition.SecondColor == ColorCh.Cameleon) && (chromino.ThirdColor == currentPosition.ThirdColor || currentPosition.ThirdColor == ColorCh.Cameleon))
                    goodPositions.Add(currentPosition);

                if (chromino.FirstColor != chromino.ThirdColor && (chromino.FirstColor == currentPosition.ThirdColor || currentPosition.ThirdColor == ColorCh.Cameleon) && (chromino.SecondColor == currentPosition.SecondColor || chromino.SecondColor == ColorCh.Cameleon || currentPosition.SecondColor == ColorCh.Cameleon) && (chromino.ThirdColor == currentPosition.FirstColor || currentPosition.FirstColor == ColorCh.Cameleon))
                {
                    Position position = new Position(currentPosition) { Reversed = true };
                    goodPositions.Add(position);
                }
            }
            return goodPositions;
        }

        /// <summary>
        /// liste (sans doublons) des coordonées libres ajdacentes aux squares occupés.
        /// les coordonées des derniers squares occupés sont remontées en premiers.
        /// </summary>
        /// <param name="squares">liste de squares</param>
        /// <returns></returns>
        public HashSet<Coordinate> FreeAroundSquares(List<Square> occupiedSquares, List<Square> testedSquares = null)
        {
            if (testedSquares == null)
                testedSquares = occupiedSquares;

            HashSet<Coordinate> coordinates = new HashSet<Coordinate>();
            foreach (Square square in testedSquares)
            {
                Coordinate rightCoordinate = square.GetRight();
                Coordinate bottomCoordinate = square.GetBottom();
                Coordinate leftCoordinate = square.GetLeft();
                Coordinate topCoordinate = square.GetTop();
                if (rightCoordinate.IsFree(occupiedSquares))
                    coordinates.Add(rightCoordinate);
                if (bottomCoordinate.IsFree(occupiedSquares))
                    coordinates.Add(bottomCoordinate);
                if (leftCoordinate.IsFree(occupiedSquares))
                    coordinates.Add(leftCoordinate);
                if (topCoordinate.IsFree(occupiedSquares))
                    coordinates.Add(topCoordinate);
            }
            return coordinates;
        }

        ///// <summary>
        ///// change l'ordre des n chrominos de la main s'il y a n-1 cameleon
        ///// afin de jouer les cameleon et finir avec un chromino normal
        ///// </summary>
        ///// <param name="hand">référence de la liste des chrominos de la main du joueur</param>
        //private void SortHandToFinishedWithoutCameleon(ref List<ChrominoInHand> hand)
        //{
        //    if (hand.Count > 1)
        //    {
        //        bool forcePlayCameleon = true;
        //        for (int i = 1; i < hand.Count; i++)
        //        {
        //            if (!ChrominoDal.IsCameleon(hand[i].ChrominoId))
        //            {
        //                forcePlayCameleon = false;
        //                break;
        //            }
        //        }
        //        if (forcePlayCameleon)
        //        {
        //            ChrominoInHand chrominoAt0 = hand[0];
        //            hand.RemoveAt(0);
        //            hand.Add(chrominoAt0);
        //        }
        //    }
        //}
        private List<Position> PositionsToDeleteAroundSquares(List<Square> squares, int indexSquare)
        {
            List<Position> positions = new List<Position>();
            List<Square> lastSquares = new List<Square> { squares[indexSquare], squares[1 + indexSquare], squares[2 + indexSquare] };
            int x = lastSquares[1].X;
            int y = lastSquares[1].Y;
            Coordinate offset = lastSquares[2].Coordinate - lastSquares[1].Coordinate;
            Coordinate offset2 = lastSquares[1].Coordinate - lastSquares[0].Coordinate;
            if (offset != offset2)
                throw new Exception("chromino en erreur");

            Orientation orientation = offset.GetOrientation();
            if (orientation == Orientation.Horizontal)
            {
                positions.Add(new Position { Coordinate = new Coordinate(x - 3, y - 1), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 4, y), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 3, y), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 3, y + 1), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 2, y + 1), Orientation = Orientation.Horizontal, Reversed = false });

                positions.Add(new Position { Coordinate = new Coordinate(x - 1, y - 3), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y - 3), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 1, y - 3), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 2, y - 2), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 1, y - 2), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y - 2), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 1, y - 2), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 2, y - 2), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 2, y - 1), Orientation = Orientation.Vertical, Reversed = false });

                positions.Add(new Position { Coordinate = new Coordinate(x - 2, y - 1), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 1, y - 1), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y - 1), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 1, y - 1), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 2, y), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 1, y), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 1, y), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 2, y), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 1, y + 1), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y + 1), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 1, y + 1), Orientation = Orientation.Horizontal, Reversed = false });

                positions.Add(new Position { Coordinate = new Coordinate(x - 2, y - 1), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 1, y - 1), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y - 1), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 1, y - 1), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 2, y), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 1, y), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 1, y), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 2, y), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 1, y + 1), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y + 1), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 1, y + 1), Orientation = Orientation.Vertical, Reversed = false });
            }
            else
            {
                positions.Add(new Position { Coordinate = new Coordinate(x - 2, y - 2), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 3, y - 1), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 2, y - 1), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 3, y), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 2, y), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 3, y + 1), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 2, y + 1), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 2, y + 2), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 1, y + 2), Orientation = Orientation.Horizontal, Reversed = false });

                positions.Add(new Position { Coordinate = new Coordinate(x, y - 4), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 1, y - 3), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y - 3), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 1, y - 3), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 1, y - 2), Orientation = Orientation.Vertical, Reversed = false });

                positions.Add(new Position { Coordinate = new Coordinate(x - 1, y - 2), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y - 2), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 1, y - 1), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y - 1), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 1, y - 1), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 1, y), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 1, y), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 1, y + 1), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y + 1), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 1, y + 1), Orientation = Orientation.Horizontal, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y + 2), Orientation = Orientation.Horizontal, Reversed = false });

                positions.Add(new Position { Coordinate = new Coordinate(x - 1, y - 2), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y - 2), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 1, y - 1), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y - 1), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 1, y - 1), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 1, y), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 1, y), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x - 1, y + 1), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y + 1), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x + 1, y + 1), Orientation = Orientation.Vertical, Reversed = false });
                positions.Add(new Position { Coordinate = new Coordinate(x, y + 2), Orientation = Orientation.Vertical, Reversed = false });
            }
            return positions;
        }

        /// <summary>
        /// retourne les positions possibles avec les couleurs remplissants les conditions
        /// </summary>
        /// <param name="squares">liste des squares occupés du jeu</param>
        /// <returns>Flip touijours false</returns>
        public List<Position> CandidatesPositionsAroundSquares(List<Square> squares, int indexSquare)
        {
            List<Square> lastSquares = new List<Square> { squares[indexSquare], squares[1 + indexSquare], squares[2 + indexSquare] };
            List<Position> positionsFilter1 = new List<Position>();
            int x = lastSquares[1].X;
            int y = lastSquares[1].Y;

            Coordinate absOffset = (lastSquares[2].Coordinate - lastSquares[1].Coordinate).Abs();
            Coordinate absOffset2 = (lastSquares[1].Coordinate - lastSquares[0].Coordinate).Abs();
            if (absOffset != absOffset2)
                throw new Exception("Dernier chromino posé en erreur");

            Orientation orientation = absOffset.GetOrientation();
            if (orientation == Orientation.Horizontal)
            {
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 3, y - 1), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 1, y - 1), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x, y - 1), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x + 1, y - 1), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 4, y), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 3, y + 1), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 2, y + 1), Orientation = Orientation.Horizontal, Reversed = false });

                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 1, y - 3), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x, y - 3), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x + 1, y - 3), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 2, y - 2), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x + 2, y - 1), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 2, y), Orientation = Orientation.Vertical, Reversed = false });

                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 2, y - 1), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x + 2, y), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 1, y + 1), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x, y + 1), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x + 1, y + 1), Orientation = Orientation.Horizontal, Reversed = false });

                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 2, y - 1), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x + 2, y), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 1, y + 1), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x, y + 1), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x + 1, y + 1), Orientation = Orientation.Vertical, Reversed = false });
            }
            else
            {
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 2, y - 2), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x, y - 2), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 3, y - 1), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 3, y), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 3, y + 1), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 2, y + 2), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 1, y + 2), Orientation = Orientation.Horizontal, Reversed = false });

                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x, y - 4), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 1, y - 3), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x + 1, y - 3), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x + 1, y - 2), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 1, y - 1), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 1, y), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 1, y + 1), Orientation = Orientation.Vertical, Reversed = false });

                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 1, y - 2), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x + 1, y - 1), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x + 1, y), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x + 1, y + 1), Orientation = Orientation.Horizontal, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x, y + 2), Orientation = Orientation.Horizontal, Reversed = false });

                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x - 1, y - 2), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x + 1, y - 1), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x + 1, y), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x + 1, y + 1), Orientation = Orientation.Vertical, Reversed = false });
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x, y + 2), Orientation = Orientation.Vertical, Reversed = false });
            }

            List<Position> candidatesPositions = new List<Position>();
            foreach (var position in positionsFilter1)
            {
                Coordinate firstCoordinate = position.Coordinate;
                ColorCh? firstColor = firstCoordinate.OkColorFor(squares, out int commonSidesFirstColor);
                if (firstCoordinate.IsFree(squares) && firstColor != null)
                {
                    absOffset = new Coordinate(position.Orientation);
                    Coordinate secondCoordinate = firstCoordinate + absOffset;
                    Coordinate thirdCoordinate = secondCoordinate + absOffset;

                    if (secondCoordinate.IsFree(squares) && thirdCoordinate.IsFree(squares))
                    {
                        ColorCh? secondColor = secondCoordinate.OkColorFor(squares, out int adjacentChrominoSecondColor);
                        ColorCh? thirdColor = thirdCoordinate.OkColorFor(squares, out int adjacentChrominosThirdColor);

                        if (secondColor != null && thirdColor != null && commonSidesFirstColor + adjacentChrominoSecondColor + adjacentChrominosThirdColor >= 2)
                        {
                            position.FirstColor = firstColor;
                            position.SecondColor = secondColor;
                            position.ThirdColor = thirdColor;
                            candidatesPositions.Add(position);
                        }
                    }
                }
            }
            return candidatesPositions;
        }

        public List<Position> CandidatesPositionsAroundAllSquares(List<Square> squares)
        {
            HashSet<Position> positionsFilter1 = new HashSet<Position>();
            HashSet<Coordinate> coordinates = FreeAroundSquares(squares);
            foreach (Coordinate currentCoordinate in coordinates)
            {
                foreach (Orientation orientation in (Orientation[])Enum.GetValues(typeof(Orientation)))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        bool inverse = i == 0 ? true : false;
                        Coordinate offset;
                        if (inverse)
                            if (orientation == Orientation.Horizontal)
                                offset = new Coordinate(-1, 0);
                            else
                                offset = new Coordinate(0, -1);
                        else
                            offset = new Coordinate(0, 0);

                        if (currentCoordinate.IsFreeForChromino(offset, squares))
                        {
                            Coordinate coordinate = currentCoordinate + 2 * offset;
                            positionsFilter1.Add(new Position { Coordinate = coordinate, Orientation = orientation, Reversed = false });
                        }
                    }
                }
            }

            List<Position> candidatesPositions = new List<Position>();
            foreach (var position in positionsFilter1)
            {
                Coordinate firstCoordinate = position.Coordinate;
                ColorCh? firstColor = firstCoordinate.OkColorFor(squares, out int commonSidesFirstColor);
                if (firstColor != null)
                {
                    Coordinate absOffset = new Coordinate(position.Orientation);
                    Coordinate secondCoordinate = firstCoordinate + absOffset;
                    Coordinate thirdCoordinate = secondCoordinate + absOffset;
                    ColorCh? secondColor = secondCoordinate.OkColorFor(squares, out int adjacentChrominoSecondColor);
                    ColorCh? thirdColor = thirdCoordinate.OkColorFor(squares, out int adjacentChrominosThirdColor);
                    if (secondColor != null && thirdColor != null && commonSidesFirstColor + adjacentChrominoSecondColor + adjacentChrominosThirdColor >= 2)
                    {
                        position.FirstColor = firstColor;
                        position.SecondColor = secondColor;
                        position.ThirdColor = thirdColor;
                        candidatesPositions.Add(position);
                    }
                }
            }
            return candidatesPositions;
        }
    }
}

