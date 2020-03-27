using Data.DAL;
using Data.Enumeration;
using Data.Models;
using System.Collections.Generic;
using System.IO;
using Tool;

namespace Data.Core
{
    public partial class GameCore
    {
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
            HashSet<Coordinate> coordinates = FreeAroundSquares(squares);
            List<Position> positions = ComputePossiblesPositions(coordinates, squares);

            // cherche un chromino dans la main du bot correspondant à un possiblePosition
            List<ChrominoInHand> hand;
            if (previouslyDraw)
                hand = new List<ChrominoInHand> { GameChrominoDal.GetNewAddedChrominoInHand(GameId, botId) };
            else
                hand = GameChrominoDal.ChrominosByPriority(GameId, botId);

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
                    List<int> playerIdWithLastChrominoIdInHand = GameChrominoDal.PlayersIdWithOneChrominoKnown(GameId, botId);
                    if (playerIdWithLastChrominoIdInHand != null && playerIdWithLastChrominoIdInHand.Count == 1 && GameChrominoDal.InHand(GameId, playerIdWithLastChrominoIdInHand[0]) == 1)
                    {
                        // 1 seul adversaire n'a plus qu'un chromino
                        List<Square> squaresAfterTry = ComputeSquares(currentChrominoToPlay);
                        squaresAfterTry.AddRange(squares);
                        HashSet<Coordinate> coordinatesAfterTry = FreeAroundSquares(squaresAfterTry);
                        List<Position> positionsAfterTry = ComputePossiblesPositions(coordinatesAfterTry, squaresAfterTry);
                        Position positionWhereOpponentCanPlayAfterTry = PositionWherePlayerCanPlay(playerIdWithLastChrominoIdInHand[0], positionsAfterTry);

                        int numberChrominosInHand = GameChrominoDal.InHand(GameId, botId);
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
            int numberInHand = GameChrominoDal.InHand(GameId, playerId);
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
                    int chrominoId = GameChrominoDal.FirstChromino(GameId, playerId).Id;
                    GameChrominoDal.UpdateLastChrominoInHand(GameId, playerId, chrominoId);
                }
                else if (numberInHand == 0)
                {
                    GameChrominoDal.DeleteLastChrominoInHand(GameId, playerId);
                    if (GamePlayerDal.PlayersNumber(GameId) > 1)
                    {
                        PlayerDal.IncreaseWin(playerId);
                        GamePlayerDal.SetWon(GameId, playerId);
                    }
                    else
                    {
                        int chrominosInGame = GameChrominoDal.InGame(GameId);
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
                    int chrominoId = GameChrominoDal.LastChrominoIdInHand(GameId, playerId);
                    if (chrominoInGame.ChrominoId == chrominoId)
                        GameChrominoDal.DeleteLastChrominoInHand(GameId, playerId);
                }

                if (IsRoundLastPlayer(playerId) && GamePlayerDal.IsSomePlayerWon(GameId))
                    SetGameFinished();
                new PictureFactory(GameId, Path.Combine(Env.WebRootPath, "image/game"), Ctx).MakeThumbnail();
                ChangePlayerTurn();
            }
            return playReturn;
        }

        /// <summary>
        /// put a chrominoInGame in game (playerId = 0 : first chromino in center)
        /// </summary>
        /// <param name="chrominoInGame"></param>
        /// <param name="playerId">null for first chromino</param>
        /// <returns></returns>
        public PlayReturn PlayChromino(ChrominoInGame chrominoInGame, int? playerId)
        {
            List<Square> squaresInGame = SquareDal.List(GameId);
            Chromino chromino = new ChrominoDal(Ctx).Details(chrominoInGame.ChrominoId);
            SquareDal.ComputeOffset(chrominoInGame.Orientation, out int offsetX, out int offsetY);
            Coordinate firstCoordinate = new Coordinate(chrominoInGame.XPosition, chrominoInGame.YPosition);
            Coordinate secondCoordinate = new Coordinate(firstCoordinate.X + offsetX, firstCoordinate.Y + offsetY);
            Coordinate thirdCoordinate = new Coordinate(firstCoordinate.X + 2 * offsetX, firstCoordinate.Y + 2 * offsetY);

            int gameId = chrominoInGame.GameId;

            if (playerId > 0 && (!IsFree(ref squaresInGame, firstCoordinate) || !IsFree(ref squaresInGame, secondCoordinate) || !IsFree(ref squaresInGame, thirdCoordinate)))
            {
                return PlayReturn.NotFree;
            }
            else
            {
                int n1, n2, n3;
                if (playerId == 0) // le premier chromino posé par aucun joueur
                {
                    n1 = 2; n2 = 2; n3 = 2;
                }
                else
                {
                    n1 = 0; n2 = 0; n3 = 0;
                }
                // todo not the best method for validate first chromino position...

                if (playerId > 0 && ((n1 = SquareDal.GetNumberSameColorsAround(gameId, firstCoordinate, chromino.FirstColor)) == -1 || (n2 = SquareDal.GetNumberSameColorsAround(gameId, secondCoordinate, chromino.SecondColor)) == -1 || (n3 = SquareDal.GetNumberSameColorsAround(gameId, thirdCoordinate, chromino.ThirdColor)) == -1))
                {
                    return PlayReturn.DifferentColorsAround;
                }
                else if (playerId > 0 && (n1 + n2 + n3 < 2))
                {
                    return PlayReturn.NotTwoOrMoreSameColors;
                }
                else
                {
                    //the position is ok :)
                    bool firstSquareOpenRight = false, firstSquareOpenBottom = false, firstSquareOpenLeft = false, firstSquareOpenTop = false;
                    bool secondSquareOpenRight = false, secondSquareOpenBottom = false, secondSquareOpenLeft = false, secondSquareOpenTop = false;
                    bool thirdSquareOpenRight = false, thirdSquareOpenBottom = false, thirdSquareOpenLeft = false, thirdSquareOpenTop = false;
                    switch (chrominoInGame.Orientation)
                    {
                        case Orientation.Horizontal:
                            firstSquareOpenRight = true;
                            secondSquareOpenRight = true;
                            secondSquareOpenLeft = true;
                            thirdSquareOpenLeft = true;
                            break;
                        case Orientation.HorizontalFlip:
                            firstSquareOpenLeft = true;
                            secondSquareOpenRight = true;
                            secondSquareOpenLeft = true;
                            thirdSquareOpenRight = true;
                            break;
                        case Orientation.Vertical:
                            firstSquareOpenTop = true;
                            secondSquareOpenTop = true;
                            secondSquareOpenBottom = true;
                            thirdSquareOpenBottom = true;
                            break;
                        case Orientation.VerticalFlip:
                            firstSquareOpenBottom = true;
                            secondSquareOpenTop = true;
                            secondSquareOpenBottom = true;
                            thirdSquareOpenTop = true;
                            break;
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
                    GameChrominoDal.Add(chrominoInGame);
                    GameChrominoDal.DeleteInHand(gameId, chromino.Id);
                    GameDal.IncreaseMove(gameId);
                    return PlayReturn.Ok;
                }
            }
        }
    }
}