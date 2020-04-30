using Data;
using Data.BI;
using Data.DAL;
using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using Tool;

namespace Core
{
    public class GoodPositionBI
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
        private readonly GoodPositionDal GoodPositionDal;

        public GoodPositionBI(Context ctx, int gameId)
        {
            GameId = gameId;
            ChrominoInHandDal = new ChrominoInHandDal(ctx);
            ChrominoDal = new ChrominoDal(ctx);
            SquareDal = new SquareDal(ctx);
            GamePlayerDal = new GamePlayerDal(ctx);
            GoodPositionDal = new GoodPositionDal(ctx);
        }

        public void RemovePlayedChromino(int playerId, int chrominoId)
        {
            GoodPositionDal.Remove(GameId, playerId, chrominoId);
        }

        public void RemoveBadEntrie(ChrominoInGame chrominoInGame, int playerId)
        {
            Orientation orientation = chrominoInGame.Orientation;
            Position position = new Position { Coordinate = new Coordinate(chrominoInGame.XPosition, chrominoInGame.YPosition), Orientation = orientation };
            GoodPositionDal.Remove(GameId, playerId, new List<Position> { position }, chrominoInGame.ChrominoId);
        }

        public void Add(int playerId, int chrominoId)
        {
            RemoveAndUpdate(false, playerId, true, chrominoId);
        }

        public void RemoveAndUpdateAllPlayers()
        {
            RemoveAndUpdate(true, 0, false, 0);
        }

        public void UpdateAllPlayersWholeGame()
        {
            RemoveAndUpdate(false, 0, true, 0);
        }

        /// <summary>
        /// retourne les positions potentielles où peuvent être joués des chrominos
        /// </summary>
        /// <param name="occupiedSquares">liste complète des squares occupés</param>
        /// <param name="testedSquares">liste des squares définissant la zone à rechercher</param>
        /// <returns>Flip toujours à false</returns>
        public HashSet<Position> PotentialPositions(List<Square> occupiedSquares, List<Square> testedSquares = null)
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

        public List<Position> PositionsOkForChromino(int chrominoId, List<Position> positions)
        {
            Chromino chromino = ChrominoDal.Details(chrominoId);
            List<Position> positionsOk = new List<Position>();
            foreach (Position currentPosition in positions)
            {
                if ((chromino.FirstColor == currentPosition.FirstColor || currentPosition.FirstColor == ColorCh.Cameleon) && (chromino.SecondColor == currentPosition.SecondColor || chromino.SecondColor == ColorCh.Cameleon || currentPosition.SecondColor == ColorCh.Cameleon) && (chromino.ThirdColor == currentPosition.ThirdColor || currentPosition.ThirdColor == ColorCh.Cameleon))
                    positionsOk.Add(currentPosition);

                if (chromino.FirstColor != chromino.ThirdColor && (chromino.FirstColor == currentPosition.ThirdColor || currentPosition.ThirdColor == ColorCh.Cameleon) && (chromino.SecondColor == currentPosition.SecondColor || chromino.SecondColor == ColorCh.Cameleon || currentPosition.SecondColor == ColorCh.Cameleon) && (chromino.ThirdColor == currentPosition.FirstColor || currentPosition.FirstColor == ColorCh.Cameleon))
                    positionsOk.Add(new Position(currentPosition) { Reversed = true });
            }
            return positionsOk;
        }

        /// <summary>
        /// liste (sans doublons) des coordonées libres ajdacentes aux squares occupés.
        /// les coordonées des derniers squares occupés sont remontées en premiers.
        /// </summary>
        /// <param name="squares">liste de squares</param>
        /// <returns></returns>
        private HashSet<Coordinate> FreeAroundSquares(List<Square> occupiedSquares, List<Square> testedSquares)
        {
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

        private List<Position> PositionsToDeleteAroundSquares(List<Square> squares, int indexSquare)
        {
            List<Square> lastSquares = new List<Square> { squares[indexSquare], squares[1 + indexSquare], squares[2 + indexSquare] };
            int x = lastSquares[1].X;
            int y = lastSquares[1].Y;
            Coordinate offset = lastSquares[2].Coordinate - lastSquares[1].Coordinate;
            Coordinate offset2 = lastSquares[1].Coordinate - lastSquares[0].Coordinate;
            if (offset != offset2)
                throw new Exception("chromino en erreur");

            Orientation orientation = offset.GetOrientation();
            List<Position> positions = new List<Position>();

            #region ajout des positions
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
            #endregion

            return positions;
        }

        /// <summary>
        /// retourne les positions possibles avec les couleurs remplissants les conditions
        /// </summary>
        /// <param name="squares">liste des squares occupés du jeu</param>
        /// <returns>Flip touijours false</returns>
        private List<Position> CandidatesPositionsAroundSquares(List<Square> squares, int indexSquare)
        {
            List<Square> lastSquares = new List<Square> { squares[indexSquare], squares[1 + indexSquare], squares[2 + indexSquare] };

            int x = lastSquares[1].X;
            int y = lastSquares[1].Y;

            Coordinate absOffset = (lastSquares[2].Coordinate - lastSquares[1].Coordinate).Abs();
            Coordinate absOffset2 = (lastSquares[1].Coordinate - lastSquares[0].Coordinate).Abs();
            if (absOffset != absOffset2)
                throw new Exception("Dernier chromino posé en erreur");

            Orientation orientation = absOffset.GetOrientation();
            List<Position> positionsFilter1 = new List<Position>();

            #region ajout des positionsFilter1
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
                positionsFilter1.Add(new Position { Coordinate = new Coordinate(x + 2, y - 2), Orientation = Orientation.Vertical, Reversed = false });
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
            #endregion

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

        private void RemoveAndUpdate(bool remove, int playerId, bool wholeGame, int chrominoId)
        {
            List<Square> squares = SquareDal.List(GameId);
            int chrominoNumber = wholeGame ? squares.Count / 3 : 1;
            for (int iSquare = (chrominoNumber - 1) * 3; iSquare >= 0; iSquare -= 3)
            {
                List<Position> positionsToDelete = remove ? PositionsToDeleteAroundSquares(squares, iSquare) : new List<Position>();
                List<Position> candidatesPositions = CandidatesPositionsAroundSquares(squares, iSquare);
                List<int> playersId = playerId == 0 ? GamePlayerDal.PlayersId(GameId) : new List<int> { playerId };

                GoodPositionComparer goodPositionComparer = new GoodPositionComparer();
                foreach (int currentPlayerId in playersId)
                {
                    if (remove)
                        GoodPositionDal.Remove(GameId, currentPlayerId, positionsToDelete, chrominoId);

                    List<int> chrominosId = new List<int>();
                    if (chrominoId == 0)
                        foreach (ChrominoInHand chrominoInHand in ChrominoInHandDal.ChrominosByPriority(GameId, currentPlayerId))
                            chrominosId.Add(chrominoInHand.ChrominoId);
                    else
                        chrominosId.Add(chrominoId);

                    HashSet<GoodPosition> goodPositions = new HashSet<GoodPosition>(goodPositionComparer);
                    foreach (int currentChrominoId in chrominosId)
                    {
                        List<Position> positionsOk = PositionsOkForChromino(currentChrominoId, candidatesPositions);
                        foreach (Position position in positionsOk)
                        {
                            GoodPosition goodPosition = new GoodPosition
                            {
                                GameId = GameId,
                                PlayerId = currentPlayerId,
                                ChrominoId = currentChrominoId,
                                X = position.Coordinate.X,
                                Y = position.Coordinate.Y,
                                Orientation = position.Orientation,
                                Flip = position.Reversed,
                            };
                            goodPositions.Add(goodPosition);
                        }
                    }
                    GoodPositionDal.Add(goodPositions);
                }
            }
        }
    }
}

