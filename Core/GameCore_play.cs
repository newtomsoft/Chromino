using Core;
using Data.DAL;
using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Tool;

namespace Data.Core
{
    public partial class GameCore
    {
        /// <summary>
        /// pioche un chromino et indique que le joueur à pioché au coup d'avant
        /// s'il n'y a plus de chrimino dans la pioche, passe le tour
        /// </summary>
        /// <param name="playerId">Id du joueur</param>
        public void DrawChromino(int playerId)
        {
            int chrominoId = ChrominoInGameDal.StackToHand(GameId, playerId);
            if (chrominoId != 0)
            {
                if (PlayerDal.IsBot(playerId))
                    UpdateComputedChrominos(playerId, chrominoId);

                GamePlayerDal.SetPreviouslyDraw(GameId, playerId);
            }
            else
            {
                SkipTurn(playerId);
            }
        }

        /// <summary>
        /// placement d'un autre chrominos de la main du bot dans le jeu
        /// </summary>
        /// <param name="gameId"></param>
        public PlayReturn PlayBot(int botId)
        {
            bool previouslyDraw = GamePlayerDal.IsPreviouslyDraw(GameId, botId);
            int playersNumber = GamePlayerDal.PlayersNumber(GameId);
            if (GamePlayerDal.IsAllPass(GameId) && (!previouslyDraw || playersNumber == 1))
            {
                DrawChromino(botId);
                return PlayReturn.DrawChromino;
            }
            List<Square> squares = SquareDal.List(GameId);
            HashSet<Position> positions = ComputePossiblesPositions(squares);

            // cherche un chromino dans la main du bot correspondant à un possiblePosition
            List<ChrominoInHand> hand;
            if (previouslyDraw)
                hand = new List<ChrominoInHand> { ChrominoInHandDal.GetNewAddedChrominoInHand(GameId, botId) };
            else
                hand = ChrominoInHandDal.ChrominosByPriority(GameId, botId);

            bool firstSearch = true;
            ChrominoInGame goodChromino = null;
            ChrominoInGame previouslyChromino = null;
            ChrominoInGame badChromino = null;
            int indexChrominoToPlay = 0;
            while (goodChromino == null)
            {
                if (firstSearch)
                {
                    firstSearch = false;
                }
                else
                {
                    hand.RemoveRange(0, indexChrominoToPlay + 1);
                    if (hand.Count == 0)
                        break;
                }

                ChrominoInGame currentChrominoToPlay = ComputeChrominoToPlay(hand, previouslyDraw, positions, out indexChrominoToPlay, out _);
                if (currentChrominoToPlay == null)
                    break;
                else // le bot peut jouer
                {
                    previouslyChromino = currentChrominoToPlay;
                    List<int> playerIdWithLastChrominoIdInHand = ChrominoInHandDal.PlayersIdWithOneChrominoKnown(GameId, botId);
                    if (playerIdWithLastChrominoIdInHand != null && playerIdWithLastChrominoIdInHand.Count == 1 && ChrominoInHandDal.ChrominosNumber(GameId, playerIdWithLastChrominoIdInHand[0]) == 1)
                    {
                        // 1 seul adversaire n'a plus qu'un chromino
                        List<Square> squaresAfterTry = ComputeSquares(currentChrominoToPlay);
                        squaresAfterTry.AddRange(squares);
                        HashSet<Position> positionsAfterTry = ComputePossiblesPositions(squaresAfterTry);
                        Position positionWhereOpponentCanPlayAfterTry = PositionWherePlayerCanPlay(playerIdWithLastChrominoIdInHand[0], positionsAfterTry);

                        int numberChrominosInHand = ChrominoInHandDal.ChrominosNumber(GameId, botId);
                        if (PositionWherePlayerCanPlay(playerIdWithLastChrominoIdInHand[0], positions) != null)
                        {
                            // l'adversaire peut finir après le tour du bot
                            if (currentChrominoToPlay != null && numberChrominosInHand == 1)
                            {
                                // le bot peut finir ce tour => il joue
                                goodChromino = currentChrominoToPlay;
                            }
                            else if (false) // todo le bot joue après l'adversaire et il peut finir le coup d'après => il joue
                            {
                                goodChromino = currentChrominoToPlay;
                            }
                            else if (positionWhereOpponentCanPlayAfterTry == null)
                            {
                                // le bot joue pour le bloquer
                                goodChromino = currentChrominoToPlay;
                            }
                        }
                        else
                        {
                            // l'adversaire ne peut pas jouer en l'état
                            if (currentChrominoToPlay != null && numberChrominosInHand == 1)
                            {
                                // le bot peut finir ce tour => il joue
                                goodChromino = currentChrominoToPlay;
                            }
                            else if (false) // todo le bot peut finir le coup d'après : il joue
                            {
                                goodChromino = currentChrominoToPlay;
                            }
                            else if (positionWhereOpponentCanPlayAfterTry == null)
                            {
                                // l'adversaire ne finira pas après le coup du bot : le bot joue
                                goodChromino = currentChrominoToPlay;
                            }
                            else if (positionWhereOpponentCanPlayAfterTry != null)
                            {
                                // l'adversaire peut finir si le bot joue ce soup : il ne doit pas le jouer
                                badChromino = currentChrominoToPlay;
                            }
                        }
                    }
                    else
                    {
                        // plus d'un adversaire a 1 chromino.
                        // v1 : le bot joue
                        //todo : prévoir stratégie de jeu
                        goodChromino = currentChrominoToPlay;
                    }
                }
            }

            if (goodChromino == null && (badChromino != null || !previouslyDraw) || previouslyChromino == null)
            {
                // pas de meilleur chromino et possibilité de piocher ou pas de chromino du tout à jouer
                if (!previouslyDraw || playersNumber == 1)
                {
                    DrawChromino(botId);
                    return PlayReturn.DrawChromino;
                }
                else
                {
                    SkipTurn(botId);
                    return PlayReturn.Ok;
                }
            }
            else
            {
                if (goodChromino == null)
                    goodChromino = previouslyChromino;
                PlayReturn playReturn = Play(goodChromino, botId);

                new PictureFactory(GameId, Path.Combine(Env.WebRootPath, "image/game"), Ctx).MakeThumbnail();
                if (IsRoundLastPlayer(botId) && GamePlayerDal.IsSomePlayerWon(GameId))
                    SetGameFinished();
                return playReturn;
            }
        }

        /// <summary>
        /// Tente de jouer chrominoInGame
        /// </summary>
        /// <param name="chrominoInGame">Chromino à jouer avec positions et orientation</param>
        /// <param name="playerId">Id du joueur</param>
        /// <returns>PlayReturn.Ok si valide</returns>
        public PlayReturn Play(ChrominoInGame chrominoInGame, int playerId)
        {
            int numberInHand = ChrominoInHandDal.ChrominosNumber(GameId, playerId);
            PlayReturn playReturn;
            if (GamePlayerDal.PlayerTurn(GameId).Id != playerId)
                playReturn = PlayReturn.NotPlayerTurn;
            else if (numberInHand == 1 && ChrominoDal.IsCameleon(chrominoInGame.ChrominoId))
                playReturn = PlayReturn.LastChrominoIsCameleon; // interdit de jouer le denier chromino si c'est un caméléon
            else
                playReturn = PlayChromino(chrominoInGame, playerId);
            if (playReturn == PlayReturn.Ok)
            {
                numberInHand--;
                GamePlayerDal.SetPass(GameId, playerId, false);
                int points = ChrominoDal.Details(chrominoInGame.ChrominoId).Points;
                GamePlayerDal.AddPoints(GameId, playerId, points);
                if (Players.Count > 1)
                    PlayerDal.AddPoints(playerId, points);

                if (numberInHand == 1)
                {
                    int chrominoId = ChrominoInHandDal.FirstChromino(GameId, playerId).Id;
                    ChrominoInHandDal.UpdateLastChrominoInHand(GameId, playerId, chrominoId);
                }
                else if (numberInHand == 0)
                {
                    ChrominoInHandDal.DeleteLastChrominoInHand(GameId, playerId);
                    if (GamePlayerDal.PlayersNumber(GameId) > 1)
                    {
                        PlayerDal.IncreaseWin(playerId);
                        GamePlayerDal.SetWon(GameId, playerId);
                    }
                    else
                    {
                        int chrominosInGame = ChrominoInGameDal.ChrominosNumber(GameId);
                        PlayerDal.Details(playerId).SinglePlayerGamesPoints += chrominosInGame switch
                        {
                            8 => 100,
                            9 => 90,
                            10 => 85,
                            11 => 82,
                            _ => 92 - chrominosInGame,
                        };
                        PlayerDal.Details(playerId).SinglePlayerGamesFinished++;
                        Ctx.SaveChanges();
                    }
                    if (IsRoundLastPlayer(playerId) || !IsNextPlayersCanWin(playerId))
                    {
                        SetGameFinished();
                    }
                    else
                    {

                    }
                }
                else
                {
                    int chrominoId = ChrominoInHandDal.LastChrominoIdInHand(GameId, playerId);
                    if (chrominoInGame.ChrominoId == chrominoId)
                        ChrominoInHandDal.DeleteLastChrominoInHand(GameId, playerId);
                }

                if (IsRoundLastPlayer(playerId) && GamePlayerDal.IsSomePlayerWon(GameId))
                    SetGameFinished();
                new PictureFactory(GameId, Path.Combine(Env.WebRootPath, "image/game"), Ctx).MakeThumbnail();
                ChangePlayerTurn();
            }
            return playReturn;
        }

        /// <summary>
        /// joue un chromino dans le jeu
        /// </summary>
        /// <param name="chrominoInGame"></param>
        /// <param name="playerId">Id du joueur</param>
        /// <returns></returns>
        public PlayReturn PlayChromino(ChrominoInGame chrominoInGame, int? playerId = null)
        {
            List<Square> squaresInGame = SquareDal.List(GameId);
            Chromino chromino = ChrominoDal.Details(chrominoInGame.ChrominoId);
            Coordinate offset = new Coordinate(chrominoInGame.Orientation);
            Coordinate firstCoordinate = new Coordinate(chrominoInGame.XPosition, chrominoInGame.YPosition);
            Coordinate secondCoordinate = firstCoordinate + offset;
            Coordinate thirdCoordinate = secondCoordinate + offset;

            int gameId = chrominoInGame.GameId;

            if (playerId != null)
            {
                if (!firstCoordinate.IsFree(ref squaresInGame) || !secondCoordinate.IsFree(ref squaresInGame) || !thirdCoordinate.IsFree(ref squaresInGame))
                    return PlayReturn.NotFree;
                int n1 = SquareDal.GetNumberSameColorsAround(gameId, firstCoordinate, chromino.FirstColor);
                int n2 = SquareDal.GetNumberSameColorsAround(gameId, secondCoordinate, chromino.SecondColor);
                int n3 = SquareDal.GetNumberSameColorsAround(gameId, thirdCoordinate, chromino.ThirdColor);
                if (n1 == -1 || n2 == -1 || n3 == -1)
                    return PlayReturn.DifferentColorsAround;
                if (n1 + n2 + n3 < 2)
                    return PlayReturn.NotTwoOrMoreSameColors;
            }

            //the position is ok
            bool firstSquareOpenRight = false, firstSquareOpenBottom = false, firstSquareOpenLeft = false, firstSquareOpenTop = false;
            bool secondSquareOpenRight = false, secondSquareOpenBottom = false, secondSquareOpenLeft = false, secondSquareOpenTop = false;
            bool thirdSquareOpenRight = false, thirdSquareOpenBottom = false, thirdSquareOpenLeft = false, thirdSquareOpenTop = false;

            if (chrominoInGame.Orientation == Orientation.Horizontal || chrominoInGame.Orientation == Orientation.HorizontalFlip)
            {
                secondSquareOpenRight = true;
                secondSquareOpenLeft = true;
            }
            else
            {
                secondSquareOpenTop = true;
                secondSquareOpenBottom = true;
            }
            if (chrominoInGame.Orientation == Orientation.Horizontal)
            {
                firstSquareOpenRight = true;
                thirdSquareOpenLeft = true;
            }
            else if (chrominoInGame.Orientation == Orientation.HorizontalFlip)
            {
                firstSquareOpenLeft = true;
                thirdSquareOpenRight = true;
            }
            else if (chrominoInGame.Orientation == Orientation.Vertical)
            {
                firstSquareOpenTop = true;
                thirdSquareOpenBottom = true;
            }
            else if (chrominoInGame.Orientation == Orientation.VerticalFlip)
            {
                firstSquareOpenBottom = true;
                thirdSquareOpenTop = true;
            }

            List<Square> squares = new List<Square>
            {
                new Square { GameId = gameId, X = firstCoordinate.X, Y = firstCoordinate.Y, Color = chromino.FirstColor, OpenRight = firstSquareOpenRight, OpenBottom = firstSquareOpenBottom, OpenLeft = firstSquareOpenLeft, OpenTop = firstSquareOpenTop },
                new Square { GameId = gameId, X = secondCoordinate.X, Y = secondCoordinate.Y, Color = chromino.SecondColor, OpenRight = secondSquareOpenRight, OpenBottom = secondSquareOpenBottom, OpenLeft = secondSquareOpenLeft, OpenTop = secondSquareOpenTop },
                new Square { GameId = gameId, X = thirdCoordinate.X, Y = thirdCoordinate.Y, Color = chromino.ThirdColor, OpenRight = thirdSquareOpenRight, OpenBottom = thirdSquareOpenBottom, OpenLeft = thirdSquareOpenLeft, OpenTop = thirdSquareOpenTop }
            };

            SquareDal.Add(squares);
            byte move = GameDal.Details(gameId).Move;
            chrominoInGame.Move = move;
            chrominoInGame.PlayerId = playerId;
            ChrominoInGameDal.Add(chrominoInGame);
            ChrominoInHandDal.DeleteInHand(gameId, chromino.Id);
            GameDal.IncreaseMove(gameId);
            UpdateComputedChrominos(0, 0, chromino.Id);
            return PlayReturn.Ok;
        }
    }
}