using Core;
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
        /// pioche un chromino et indique que le joueur à pioché au coup d'avant
        /// s'il n'y a plus de chrimino dans la pioche, passe le tour
        /// </summary>
        /// <param name="playerId">Id du joueur</param>
        public PlayReturn DrawChromino(int playerId)
        {
            if (TryDrawChromino(playerId, out PlayReturn playReturn))
                return playReturn;
            else
                return SkipTurn(playerId);
        }

        /// <summary>
        /// Tente de piocher un chromino et indique que le joueur à pioché au coup d'avant.
        /// S'il n'y a plus de chrimino dans la pioche, ne fait rien
        /// </summary>
        /// <param name="playerId">Id du joueur</param>
        /// <param name="playReturn">PlayReturn.DrawChromino si pioche ok</param>
        /// <returns>true si tentative ok</returns>
        public bool TryDrawChromino(int playerId, out PlayReturn playReturn)
        {
            int chrominoId = ChrominoInHandDal.FromStack(GameId, playerId);
            if (chrominoId != 0)
            {
                ComputedChrominoCore.Add(playerId, chrominoId);
                GamePlayerDal.SetPreviouslyDraw(GameId, playerId);
                playReturn = PlayReturn.DrawChromino;
                return true;
            }
            else
            {
                playReturn = PlayReturn.Ok;
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
            int chrominoIdNotToPlay = ChrominoIdIfSingleWithCameleons(ref chrominosInHand);
            List<ComputedChromino> ComputedChrominos = ComputedChrominosDal.RootListByPriority(GameId, botId, chrominoIdNotToPlay);
            bool previouslyDraw = GamePlayerDal.IsPreviouslyDraw(GameId, botId);
            int playersNumber = GamePlayerDal.PlayersNumber(GameId);

            if (ComputedChrominos.Count == 0)
            {
                if ((!previouslyDraw || playersNumber == 1) && TryDrawChromino(botId, out PlayReturn playreturn))
                    return playreturn;
                else
                    return SkipTurn(botId);
            }
            // le bot a un ou des chriminos pouvant être posés
            List<Square> squares = SquareDal.List(GameId);
            HashSet<Position> positions = ComputedChrominoCore.ComputePossiblesPositionsForOpponent(squares);
            List<ChrominoInGame> goodChrominos = new List<ChrominoInGame>();
            List<ChrominoInGame> badChrominos = new List<ChrominoInGame>();
            List<ChrominoInGame> notGoodNotBadchrominos = new List<ChrominoInGame>();
            foreach (ComputedChromino ComputedChromino in ComputedChrominos)
            {
                ChrominoInGame currentChrominoToPlay = ChrominoInGame.From(ComputedChromino);

                List<int> opponentIdWithOneChrominoInHand = ChrominoInHandDal.OpponentsIdWithOneChromino(GameId, botId);
                if (opponentIdWithOneChrominoInHand != null && opponentIdWithOneChrominoInHand.Count == 1)
                {
                    // 1 seul adversaire n'a plus qu'un chromino
                    int opponentId = opponentIdWithOneChrominoInHand[0];
                    List<Square> squaresAfterTry = ComputeSquares(currentChrominoToPlay);
                    squaresAfterTry.AddRange(squares);
                    HashSet<Position> positionsAfterTry = ComputedChrominoCore.ComputePossiblesPositionsForOpponent(squaresAfterTry);
                    int opponentChrominoId = ChrominoInHandDal.FirstChrominoId(GameId, opponentId);
                    HashSet<Position> positionWhereOpponentCanPlayAfterTry = ComputedChrominoCore.PositionsOkForOpponentChromino(opponentChrominoId, positionsAfterTry);
                    int numberChrominosInHand = ChrominoInHandDal.ChrominosNumber(GameId, botId);
                    if (ComputedChrominoCore.PositionsOkForOpponentChromino(opponentChrominoId, positions).Count != 0)
                    {
                        // l'adversaire peut finir après le tour du bot s'il ne joue pas
                        if (currentChrominoToPlay != null && numberChrominosInHand == 1) // le bot peut finir ce tour => il peut jouer
                            goodChrominos.Add(currentChrominoToPlay);
                        else if (false) // todo le bot joue après l'adversaire et il peut finir le coup d'après => il joue
                            goodChrominos.Add(currentChrominoToPlay);
                        else if (positionWhereOpponentCanPlayAfterTry.Count > 0) // le bot joue pour le bloquer
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
                        else if (positionWhereOpponentCanPlayAfterTry.Count == 0) // l'adversaire ne finira pas après le coup du bot : le bot joue
                            goodChrominos.Add(currentChrominoToPlay);
                        else if (positionWhereOpponentCanPlayAfterTry.Count > 0) // l'adversaire peut finir si le bot joue ce soup : il ne doit pas le jouer !
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

            if (goodChrominos.Count == 0 && (!previouslyDraw || playersNumber == 1) && TryDrawChromino(botId, out PlayReturn playReturn)) { }
            else if (goodChrominos.Count == 0 && notGoodNotBadchrominos.Count > 0)
                playReturn = Play(notGoodNotBadchrominos[0]);
            else if (goodChrominos.Count != 0)
                playReturn = Play(goodChrominos[0]);
            else
                playReturn = SkipTurn(botId);

            if (playReturn.IsError())
                ComputedChrominoCore.RemoveBadEntrie(goodChrominos[0], botId);
            else if (playReturn == PlayReturn.Ok)
                new PictureFactoryTool(GameId, Path.Combine(Env.WebRootPath, "image/game"), Ctx).MakeThumbnail();

            if (IsRoundLastPlayer(botId) && GamePlayerDal.IsSomePlayerWon(GameId))
                SetGameFinished();

            return playReturn;
        }

        /// <summary>
        /// Tente de jouer chrominoInGame
        /// </summary>
        /// <param name="chrominoInGame">Chromino à jouer avec positions et orientation</param>
        /// <returns>PlayReturn.Ok si valide</returns>
        public PlayReturn Play(ChrominoInGame chrominoInGame)
        {
            int? nullablePlayerId = chrominoInGame.PlayerId;
            int numberInHand = 0;
            if (nullablePlayerId != null)
            {
                int playerId = (int)nullablePlayerId;
                numberInHand = ChrominoInHandDal.ChrominosNumber(GameId, playerId);
                if (GamePlayerDal.PlayerTurn(GameId).Id != playerId)
                    return PlayReturn.NotPlayerTurn;
                else if (numberInHand == 1 && ChrominoDal.IsCameleon(chrominoInGame.ChrominoId))
                    return PlayReturn.LastChrominoIsCameleon; // interdit de jouer le denier chromino si c'est un caméléon
            }

            List<Square> squaresInGame = SquareDal.List(GameId);
            Chromino chromino = ChrominoDal.Details(chrominoInGame.ChrominoId);
            List<Coordinate> chrominoCoordinates = ChrominoCoordinates(chrominoInGame);
            if (!IsValidChriminoInGame(ref chrominoInGame, ref squaresInGame, out PlayReturn errorPlayReturn))
                return errorPlayReturn;

            bool firstSquareOpenRight = false, firstSquareOpenBottom = false, firstSquareOpenLeft = false, firstSquareOpenTop = false;
            bool secondSquareOpenRight = false, secondSquareOpenBottom = false, secondSquareOpenLeft = false, secondSquareOpenTop = false;
            bool thirdSquareOpenRight = false, thirdSquareOpenBottom = false, thirdSquareOpenLeft = false, thirdSquareOpenTop = false;

            if (chrominoInGame.Orientation == Orientation.Horizontal)
            {
                secondSquareOpenRight = true;
                secondSquareOpenLeft = true;
                if (chrominoInGame.Flip)
                {
                    firstSquareOpenRight = true;
                    thirdSquareOpenLeft = true;
                }
                else
                {
                    firstSquareOpenLeft = true;
                    thirdSquareOpenRight = true;
                }
            }
            else
            {
                secondSquareOpenTop = true;
                secondSquareOpenBottom = true;
                if (chrominoInGame.Flip)
                {
                    firstSquareOpenTop = true;
                    thirdSquareOpenBottom = true;
                }
                else
                {
                    firstSquareOpenBottom = true;
                    thirdSquareOpenTop = true;
                }
            }

            List<Square> squares = new List<Square>
            {
                new Square { GameId = GameId, X = chrominoCoordinates[0].X, Y = chrominoCoordinates[0].Y, Color = chrominoInGame.Flip ? chromino.ThirdColor : chromino.FirstColor, OpenRight = firstSquareOpenRight, OpenBottom = firstSquareOpenBottom, OpenLeft = firstSquareOpenLeft, OpenTop = firstSquareOpenTop },
                new Square { GameId = GameId, X = chrominoCoordinates[1].X, Y = chrominoCoordinates[1].Y, Color = chromino.SecondColor, OpenRight = secondSquareOpenRight, OpenBottom = secondSquareOpenBottom, OpenLeft = secondSquareOpenLeft, OpenTop = secondSquareOpenTop },
                new Square { GameId = GameId, X = chrominoCoordinates[2].X, Y = chrominoCoordinates[2].Y, Color = chrominoInGame.Flip ? chromino.FirstColor : chromino.ThirdColor, OpenRight = thirdSquareOpenRight, OpenBottom = thirdSquareOpenBottom, OpenLeft = thirdSquareOpenLeft, OpenTop = thirdSquareOpenTop }
            };

            SquareDal.Add(squares);
            byte move = GameDal.Details(GameId).Move;
            chrominoInGame.Move = move;
            ChrominoInGameDal.Add(chrominoInGame);
            ChrominoInHandDal.DeleteInHand(GameId, chromino.Id);
            GameDal.IncreaseMove(GameId);
            ComputedChrominoCore.RemovePlayedChromino(nullablePlayerId, chrominoInGame.ChrominoId);
            ComputedChrominoCore.RemoveAndUpdateAllPlayers();

            if (nullablePlayerId != null)
            {
                int playerId = (int)nullablePlayerId;
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
                        int chrominosInGame = ChrominoInGameDal.Count(GameId);
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
                        SetGameFinished();
                }
                else
                {
                    int chrominoId = ChrominoInHandDal.LastChrominoIdInHand(GameId, playerId);
                    if (chrominoInGame.ChrominoId == chrominoId)
                        ChrominoInHandDal.DeleteLastChrominoInHand(GameId, playerId);
                }

                if (IsRoundLastPlayer(playerId) && GamePlayerDal.IsSomePlayerWon(GameId))
                    SetGameFinished();
                new PictureFactoryTool(GameId, Path.Combine(Env.WebRootPath, "image/game"), Ctx).MakeThumbnail();
                ChangePlayerTurn();
            }
            return PlayReturn.Ok;
        }

        /// <summary>
        /// indique si le chromino est à une position valide
        /// </summary>
        /// <param name="chrominoInGame">Chromino à jouer</param>
        /// <param name="squaresInGame">squares occupés</param>
        /// <param name="playReturn">code PlayReturn d'erreur si retour false</param>
        /// <returns>true si le chromino est à une position valide</returns>
        private bool IsValidChriminoInGame(ref ChrominoInGame chrominoInGame, ref List<Square> squaresInGame, out PlayReturn playReturn)
        {
            Chromino chromino = ChrominoDal.Details(chrominoInGame.ChrominoId);
            playReturn = PlayReturn.Ok;
            if (chrominoInGame.PlayerId == null)
                return true;
            else
            {
                List<Coordinate> chrominoCoordinates = ChrominoCoordinates(chrominoInGame);
                if (!chrominoCoordinates[0].IsFree(squaresInGame) || !chrominoCoordinates[1].IsFree(squaresInGame) || !chrominoCoordinates[2].IsFree(squaresInGame))
                {
                    playReturn = PlayReturn.NotFree;
                }
                else
                {
                    int n0 = SquareDal.GetNumberSameColorsAround(GameId, chrominoCoordinates[0], chrominoInGame.Flip ? chromino.ThirdColor : chromino.FirstColor);
                    int n1 = SquareDal.GetNumberSameColorsAround(GameId, chrominoCoordinates[1], chromino.SecondColor);
                    int n2 = SquareDal.GetNumberSameColorsAround(GameId, chrominoCoordinates[2], chrominoInGame.Flip ? chromino.FirstColor : chromino.ThirdColor);
                    if (n0 == -1 || n1 == -1 || n2 == -1)
                        playReturn = PlayReturn.DifferentColorsAround;
                    if (n0 + n1 + n2 < 2)
                        playReturn = PlayReturn.NotMinTwoSameColors;
                }
                if (playReturn.IsError())
                    return false;
                else
                    return true;
            }
        }

        private List<Coordinate> ChrominoCoordinates(ChrominoInGame chrominoInGame)
        {
            Coordinate offset = new Coordinate(chrominoInGame.Orientation);
            Coordinate first = new Coordinate(chrominoInGame.XPosition, chrominoInGame.YPosition);
            Coordinate second = first + offset;
            Coordinate third = second + offset;
            return new List<Coordinate> { first, second, third };
        }

        /// <summary>
        /// indique le chromino non cameleon de la main s'il est seul avec que des cameleons
        /// </summary>
        /// <param name="hand">référence de la liste des chrominos de la main du joueur</param>
        /// <returns>id du chromino non caméléon, 0 sinon</returns>
        private int ChrominoIdIfSingleWithCameleons(ref List<ChrominoInHand> hand)
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