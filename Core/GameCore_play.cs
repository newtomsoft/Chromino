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
        public PlayReturn DrawChromino(int playerId)
        {
            int chrominoId = ChrominoInGameDal.StackToHand(GameId, playerId);
            if (chrominoId != 0)
            {
                if (PlayerDal.IsBot(playerId))
                    ComputedChrominoCore.UpdateCandidatesFromAllChrominosPlayed(playerId, chrominoId);

                GamePlayerDal.SetPreviouslyDraw(GameId, playerId);
                return PlayReturn.DrawChromino;
            }
            else
            {
                SkipTurn(playerId);
                return PlayReturn.SkipTurn;
            }
        }

        /// <summary>
        /// tente de piocher un chromino et indique que le joueur à pioché au coup d'avant
        /// s'il n'y a plus de chrimino dans la pioche, ne fait rien
        /// </summary>
        /// <param name="playerId">Id du joueur</param>
        public bool TryDrawChromino(int playerId)
        {
            int chrominoId = ChrominoInGameDal.StackToHand(GameId, playerId);
            if (chrominoId != 0)
            {
                if (PlayerDal.IsBot(playerId))
                    ComputedChrominoCore.UpdateCandidatesFromAllChrominosPlayed(playerId, chrominoId);

                GamePlayerDal.SetPreviouslyDraw(GameId, playerId);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// placement d'un autre chrominos de la main du bot dans le jeu
        /// </summary>
        /// <param name="gameId"></param>
        public PlayReturn PlayBot(int botId)
        {
            List<ChrominoInHand> chrominosInHand = ChrominoInHandDal.ChrominosByPriority(GameId, botId);
            int chrominoIdNotToPlay = PlayCameleonToFinishWithoutCameleon(ref chrominosInHand);

            List<ComputedChromino> ComputedChrominos = ComputedChrominosDal.RootListByPriority(GameId, botId, chrominoIdNotToPlay);
            bool previouslyDraw = GamePlayerDal.IsPreviouslyDraw(GameId, botId);
            int playersNumber = GamePlayerDal.PlayersNumber(GameId);




            if (ComputedChrominos.Count == 0)
            {
                ComputedChrominoCore.ResetAndUpdateCandidatesFromAllChrominosPlayed(botId); // todo 3 lignes de patch à supprimer
                ComputedChrominos = ComputedChrominosDal.RootListByPriority(GameId, botId, chrominoIdNotToPlay);
                if (ComputedChrominos.Count == 0)
                {
                    if ((!previouslyDraw || playersNumber == 1) && TryDrawChromino(botId))
                        return PlayReturn.DrawChromino;
                    else
                        return SkipTurn(botId);
                }
            }

            // le bot a un ou des chriminos pouvant être posés

            List<Square> squares = SquareDal.List(GameId);
            HashSet<Position> positions = ComputedChrominoCore.ComputePossiblesPositions(squares);

            List<ChrominoInGame> goodChrominos = new List<ChrominoInGame>();
            List<ChrominoInGame> badChrominos = new List<ChrominoInGame>();
            List<ChrominoInGame> notGoodNotBadchrominos = new List<ChrominoInGame>();
            foreach (ComputedChromino ComputedChromino in ComputedChrominos)
            {
                ChrominoInGame currentChrominoToPlay = ChrominoInGame.From(ComputedChromino);

                List<int> playerIdWithLastChrominoIdInHand = ChrominoInHandDal.PlayersIdWithOneChrominoKnown(GameId, botId);
                if (playerIdWithLastChrominoIdInHand != null && playerIdWithLastChrominoIdInHand.Count == 1 && ChrominoInHandDal.ChrominosNumber(GameId, playerIdWithLastChrominoIdInHand[0]) == 1)
                {
                    // 1 seul adversaire n'a plus qu'un chromino
                    List<Square> squaresAfterTry = ComputeSquares(currentChrominoToPlay);
                    squaresAfterTry.AddRange(squares);
                    HashSet<Position> positionsAfterTry = ComputedChrominoCore.ComputePossiblesPositions(squaresAfterTry);
                    Position positionWhereOpponentCanPlayAfterTry = PositionWherePlayerCanPlay(playerIdWithLastChrominoIdInHand[0], positionsAfterTry);

                    int numberChrominosInHand = ChrominoInHandDal.ChrominosNumber(GameId, botId);
                    if (PositionWherePlayerCanPlay(playerIdWithLastChrominoIdInHand[0], positions) != null)
                    {
                        // l'adversaire peut finir après le tour du bot
                        if (currentChrominoToPlay != null && numberChrominosInHand == 1) // le bot peut finir ce tour => il peut jouer
                            goodChrominos.Add(currentChrominoToPlay);
                        else if (false) // todo le bot joue après l'adversaire et il peut finir le coup d'après => il joue
                            goodChrominos.Add(currentChrominoToPlay);
                        else if (positionWhereOpponentCanPlayAfterTry == null) // le bot joue pour le bloquer
                            goodChrominos.Add(currentChrominoToPlay);
                        else
                            notGoodNotBadchrominos.Add(currentChrominoToPlay);
                    }
                    else
                    {
                        // l'adversaire ne peut pas jouer en l'état
                        if (currentChrominoToPlay != null && numberChrominosInHand == 1) // le bot peut finir ce tour => il joue
                            goodChrominos.Add(currentChrominoToPlay);
                        else if (false) // todo le bot peut finir le coup d'après : il joue
                            goodChrominos.Add(currentChrominoToPlay);
                        else if (positionWhereOpponentCanPlayAfterTry == null) // l'adversaire ne finira pas après le coup du bot : le bot joue
                            goodChrominos.Add(currentChrominoToPlay);
                        else if (positionWhereOpponentCanPlayAfterTry != null) // l'adversaire peut finir si le bot joue ce soup : il ne doit pas le jouer
                            badChrominos.Add(currentChrominoToPlay);
                        else
                            notGoodNotBadchrominos.Add(currentChrominoToPlay);
                    }
                }
                else
                {
                    // plus d'un adversaire a 1 chromino.
                    // v1 : le bot joue
                    //todo : prévoir stratégie de jeu
                    goodChrominos.Add(currentChrominoToPlay);
                }
            }
            PlayReturn playReturn;
            if (goodChrominos.Count == 0 && (!previouslyDraw || playersNumber == 1) && TryDrawChromino(botId))
                playReturn = PlayReturn.DrawChromino;
            else if (goodChrominos.Count == 0 && notGoodNotBadchrominos.Count > 0)
                playReturn = Play(notGoodNotBadchrominos[0], botId);
            else if (goodChrominos.Count != 0)
                playReturn = Play(goodChrominos[0], botId);
            else // le bot doit passer son tour
                playReturn = PlayReturn.SkipTurn;

            if (playReturn.IsError())
                ComputedChrominoCore.ResetAndUpdateCandidatesFromAllChrominosPlayed(botId);
            else if (playReturn == PlayReturn.Ok)
                new PictureFactory(GameId, Path.Combine(Env.WebRootPath, "image/game"), Ctx).MakeThumbnail();

            if (IsRoundLastPlayer(botId) && GamePlayerDal.IsSomePlayerWon(GameId))
                SetGameFinished();

            return playReturn;
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

            if (!IsValidPosition(ref chromino, ref firstCoordinate, ref secondCoordinate, ref thirdCoordinate, ref squaresInGame, playerId, out PlayReturn playReturn))
                return playReturn;           

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
                new Square { GameId = GameId, X = firstCoordinate.X, Y = firstCoordinate.Y, Color = chromino.FirstColor, OpenRight = firstSquareOpenRight, OpenBottom = firstSquareOpenBottom, OpenLeft = firstSquareOpenLeft, OpenTop = firstSquareOpenTop },
                new Square { GameId = GameId, X = secondCoordinate.X, Y = secondCoordinate.Y, Color = chromino.SecondColor, OpenRight = secondSquareOpenRight, OpenBottom = secondSquareOpenBottom, OpenLeft = secondSquareOpenLeft, OpenTop = secondSquareOpenTop },
                new Square { GameId = GameId, X = thirdCoordinate.X, Y = thirdCoordinate.Y, Color = chromino.ThirdColor, OpenRight = thirdSquareOpenRight, OpenBottom = thirdSquareOpenBottom, OpenLeft = thirdSquareOpenLeft, OpenTop = thirdSquareOpenTop }
            };

            SquareDal.Add(squares);
            byte move = GameDal.Details(GameId).Move;
            chrominoInGame.Move = move;
            chrominoInGame.PlayerId = playerId;
            ChrominoInGameDal.Add(chrominoInGame);
            ChrominoInHandDal.DeleteInHand(GameId, chromino.Id);
            GameDal.IncreaseMove(GameId);
            if (playerId != null)
                ComputedChrominoCore.RemoveCandidate((int)playerId, chrominoInGame.ChrominoId);
            ComputedChrominoCore.RemoveAndUpdateCandidatesFromLastChrominoPlayed();
            return PlayReturn.Ok;
        }

        private bool IsValidPosition(ref Chromino chromino, ref Coordinate firstCoordinate, ref Coordinate secondCoordinate, ref Coordinate thirdCoordinate, ref List<Square> squaresInGame, int? playerId, out PlayReturn playReturn)
        {
            playReturn = PlayReturn.Ok;
            if (playerId == null)
                return true;
            else
            {
                if (!firstCoordinate.IsFree(ref squaresInGame) || !secondCoordinate.IsFree(ref squaresInGame) || !thirdCoordinate.IsFree(ref squaresInGame))
                    playReturn = PlayReturn.NotFree;
                int n1 = SquareDal.GetNumberSameColorsAround(GameId, firstCoordinate, chromino.FirstColor);
                int n2 = SquareDal.GetNumberSameColorsAround(GameId, secondCoordinate, chromino.SecondColor);
                int n3 = SquareDal.GetNumberSameColorsAround(GameId, thirdCoordinate, chromino.ThirdColor);
                if (n1 == -1 || n2 == -1 || n3 == -1)
                    playReturn = PlayReturn.DifferentColorsAround;
                if (n1 + n2 + n3 < 2)
                    playReturn = PlayReturn.NotTwoOrMoreSameColors;

                if (playReturn.IsError())
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// indique le chromino non cameleon de la main s'il est seul avec que des cameleons
        /// </summary>
        /// <param name="hand">référence de la liste des chrominos de la main du joueur</param>
        /// <returns>id du chromino non caméléon, 0 sinon</returns>
        private int PlayCameleonToFinishWithoutCameleon(ref List<ChrominoInHand> hand)
        {
            int notCameleonNumber = 0;
            int indexFound = -1;
            if (hand.Count >= 2)
            {
                for (int i = 0; i < hand.Count; i++)
                {
                    if (!ChrominoDal.IsCameleon(hand[i].ChrominoId))
                    {
                        if (++notCameleonNumber > 1)
                            break;
                        indexFound = i;
                    }
                }
            }
            if (notCameleonNumber == 1)
                return hand[indexFound].ChrominoId;
            else
                return 0;
        }
    }
}