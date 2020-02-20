using Data.DAL;
using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.Core
{
    public class GameCore
    {
        private const int MaxChrominoInHand = 8;
        private readonly Context Ctx;
        private readonly GameChrominoDal GameChrominoDal;
        private readonly ChrominoDal ChrominoDal;
        private readonly SquareDal SquareDal;
        private readonly PlayerDal PlayerDal;
        private readonly GamePlayerDal GamePlayerDal;
        private readonly GameDal GameDal;
        private int GameId { get; set; }
        public List<Square> Squares { get; set; }
        public List<Player> Players { get; set; }
        public List<GamePlayer> GamePlayers { get; set; }

        public GameCore(Context ctx, int gameId, List<Player> listPlayers = null)
        {
            Ctx = ctx;
            GameDal = new GameDal(ctx);
            GameChrominoDal = new GameChrominoDal(ctx);
            ChrominoDal = new ChrominoDal(ctx);
            SquareDal = new SquareDal(ctx);
            PlayerDal = new PlayerDal(ctx);
            GamePlayerDal = new GamePlayerDal(ctx);

            if (listPlayers == null)
            {
                Players = GamePlayerDal.Players(gameId);
            }
            else
            {
                Players = listPlayers;
            }

            GameId = gameId;
            Squares = SquareDal.List(GameId);
            GamePlayers = new List<GamePlayer>();
            foreach (Player player in Players)
            {
                GamePlayers.Add(GamePlayerDal.Details(GameId, player.Id));
            }
        }

        public void BeginGame(int playerNumber)
        {
            if (playerNumber == 1)
                GameDal.SetStatus(GameId, GameStatus.SingleInProgress);
            else
                GameDal.SetStatus(GameId, GameStatus.InProgress);
            GameChrominoDal.FirstRandomToGame(GameId);
            FillHandPlayers();
            ChangePlayerTurn();
        }

        public void ChangePlayerTurn()
        {
            if (GamePlayers.Count == 1)
            {
                GamePlayers[0].Turn = true;
            }
            else
            {
                GamePlayer gamePlayer = (from gp in GamePlayers
                                         where gp.Turn == true
                                         select gp).FirstOrDefault();

                if (gamePlayer == null)
                {
                    gamePlayer = (from gp in GamePlayers
                                  orderby Guid.NewGuid()
                                  select gp).FirstOrDefault();
                }
                gamePlayer.PreviouslyDraw = false;
                bool selectNext = false;
                bool selected = false;
                foreach (GamePlayer currentGamePlayer in GamePlayers)
                {
                    if (selectNext)
                    {
                        currentGamePlayer.Turn = true;
                        selected = true;
                        break;
                    }
                    if (gamePlayer.Id == currentGamePlayer.Id)
                        selectNext = true;
                }
                if (!selected)
                    GamePlayers[0].Turn = true;

                gamePlayer.Turn = false;
            }
            GameDal.UpdateDate(GameId);
            Ctx.SaveChanges();
        }

        /// <summary>
        /// placement d'un autre chrominos de la main du bot dans le jeu
        /// </summary>
        /// <param name="gameId"></param>
        public PlayReturn PlayBot(int botId)
        {
            bool isPreviouslyDraw = GamePlayerDal.IsPreviouslyDraw(GameId, botId);
            int playersNumber = GamePlayerDal.PlayersNumber(GameId);
            if (GamePlayerDal.IsAllPass(GameId) && (!isPreviouslyDraw || playersNumber == 1))
            {
                DrawChromino(botId);
                return PlayReturn.DrawChromino;
            }

            HashSet<Coordinate> coordinates = FreeAroundChrominos();
            // prendre ceux avec un seul coté en commun avec un chrominos
            // calculer orientation avec les couleurs imposées ou pas
            List<PossiblesPositions> possiblesPositions = new List<PossiblesPositions>();
            foreach (Coordinate coordinate in coordinates)
            {
                Color? firstColor = OkColorFor(coordinate, out int commonSidesFirstColor);
                if (firstColor != null)
                {
                    //cherche placement possible d'un square
                    foreach (Orientation orientation in (Orientation[])Enum.GetValues(typeof(Orientation)))
                    {
                        Coordinate secondCoordinate = coordinate.GetNextCoordinate(orientation);
                        Coordinate thirdCoordinate = secondCoordinate.GetNextCoordinate(orientation);
                        if (SquareDal.IsFree(GameId, secondCoordinate) && SquareDal.IsFree(GameId, thirdCoordinate))
                        {
                            //calcul si chromino posable et dans quelle position
                            Color? secondColor = OkColorFor(secondCoordinate, out int adjacentChrominoSecondColor);
                            Color? thirdColor = OkColorFor(thirdCoordinate, out int adjacentChrominosThirdColor);

                            if (secondColor != null && thirdColor != null && commonSidesFirstColor + adjacentChrominoSecondColor + adjacentChrominosThirdColor >= 2)
                            {
                                PossiblesPositions possibleSpace = new PossiblesPositions
                                {
                                    Coordinate = coordinate,
                                    Orientation = orientation,
                                    FirstColor = firstColor,
                                    SecondColor = secondColor,
                                    ThirdColor = thirdColor,
                                };
                                possiblesPositions.Add(possibleSpace);
                            }
                        }
                    }
                }
            }

            // cherche un chromino dans la main du bot correspondant à un possiblesPosition
            List<ChrominoInHand> hand;
            bool previouslyDraw = GamePlayerDal.IsPreviouslyDraw(GameId, botId);
            if (previouslyDraw)
                hand = new List<ChrominoInHand> { GameChrominoDal.GetNewAddedChrominoInHand(GameId, botId) };
            else
                hand = GameChrominoDal.ChrominosByPriority(GameId, botId);

            ChrominoInGame goodChrominoInGame = null;
            Coordinate firstCoordinate = null;
            ChrominoInHand goodChrominoInHand;
            if (!previouslyDraw && hand.Count == 1 && ChrominoDal.IsCameleon(hand[0].ChrominoId)) // on ne peut pas poser un cameleon si c'est le dernier de le main
            {
                goodChrominoInGame = null;
            }
            else
            {
                if (hand.Count == 2 && ChrominoDal.IsCameleon(hand[1].ChrominoId))
                {
                    // s'il reste un camelon dans les 2 derniers de la main, on va chercher à le poser en priorité pour ne pas finir avec un caméléon
                    // -> inversion des 2 chrominos dans la main du bot
                    // todo? meilleur tactique de jeu ? a voir...
                    ChrominoInHand chrominoInHand0 = hand[0];
                    hand.RemoveAt(0);
                    hand.Add(chrominoInHand0);
                }
                foreach (ChrominoInHand chrominoInHand in hand)
                {
                    foreach (PossiblesPositions possiblePosition in possiblesPositions)
                    {
                        Chromino chromino = ChrominoDal.Details(chrominoInHand.ChrominoId);
                        if ((chromino.FirstColor == possiblePosition.FirstColor || possiblePosition.FirstColor == Color.Cameleon) && (chromino.SecondColor == possiblePosition.SecondColor || chromino.SecondColor == Color.Cameleon || possiblePosition.SecondColor == Color.Cameleon) && (chromino.ThirdColor == possiblePosition.ThirdColor || possiblePosition.ThirdColor == Color.Cameleon))
                        {
                            goodChrominoInHand = chrominoInHand;
                            firstCoordinate = possiblePosition.Coordinate;
                            goodChrominoInGame = new ChrominoInGame()
                            {
                                Orientation = possiblePosition.Orientation,
                                ChrominoId = chromino.Id,
                                GameId = GameId,
                            };
                            break;
                        }
                        else if ((chromino.FirstColor == possiblePosition.ThirdColor || possiblePosition.ThirdColor == Color.Cameleon) && (chromino.SecondColor == possiblePosition.SecondColor || chromino.SecondColor == Color.Cameleon || possiblePosition.SecondColor == Color.Cameleon) && (chromino.ThirdColor == possiblePosition.FirstColor || possiblePosition.FirstColor == Color.Cameleon))
                        {
                            goodChrominoInHand = chrominoInHand;
                            firstCoordinate = possiblePosition.Coordinate.GetNextCoordinate(possiblePosition.Orientation).GetNextCoordinate(possiblePosition.Orientation);
                            goodChrominoInGame = new ChrominoInGame()
                            {
                                Orientation = possiblePosition.Orientation switch
                                {
                                    Orientation.Horizontal => Orientation.HorizontalFlip,
                                    Orientation.HorizontalFlip => Orientation.Horizontal,
                                    Orientation.Vertical => Orientation.VerticalFlip,
                                    _ => Orientation.Vertical,
                                },
                                ChrominoId = chromino.Id,
                                GameId = GameId,
                            };
                            break;
                        }
                    }
                    if (goodChrominoInGame != null)
                        break;
                }
            }
            if (goodChrominoInGame == null)
            {
                if (!isPreviouslyDraw || playersNumber == 1)
                {
                    DrawChromino(botId);
                    return PlayReturn.DrawChromino;
                }
                else
                {
                    PassTurn(botId);
                    return PlayReturn.Ok;
                }
            }
            else
            {
                goodChrominoInGame.XPosition = firstCoordinate.X;
                goodChrominoInGame.YPosition = firstCoordinate.Y;
                PlayReturn playReturn = Play(goodChrominoInGame, botId);
                return playReturn;
            }
        }

        public void PassTurn(int playerId)
        {
            GamePlayerDal.SetPass(GameId, playerId, true);
            if (GameChrominoDal.InStack(GameId) == 0 && GamePlayerDal.IsAllPass(GameId))
                SetGameFinished();
            // todo : avertir tous les joueurs que la partie est nulle
            ChangePlayerTurn();
        }

        public void DrawChromino(int playerId)
        {
            if (GameChrominoDal.StackToHand(GameId, playerId) == 0)
                PassTurn(playerId);
            else
                GamePlayerDal.SetPreviouslyDraw(GameId, playerId);
        }

        public PlayReturn Play(ChrominoInGame chrominoInGame, int playerId)
        {
            if (!PlayerDal.IsBot(playerId))
            {
                Coordinate coordinate = chrominoInGame.Orientation switch
                {
                    Orientation.HorizontalFlip => new Coordinate(chrominoInGame.XPosition + 2, chrominoInGame.YPosition),
                    Orientation.Vertical => new Coordinate(chrominoInGame.XPosition, chrominoInGame.YPosition + 2),
                    _ => new Coordinate(chrominoInGame.XPosition, chrominoInGame.YPosition),
                };
                chrominoInGame.XPosition = coordinate.X;
                chrominoInGame.YPosition = coordinate.Y;
            }
            PlayReturn playReturn;
            if (GamePlayerDal.PlayerTurn(GameId).Id != playerId)
            {
                playReturn = PlayReturn.NotPlayerTurn;
            }
            else if (GameChrominoDal.PlayerNumberChrominos(GameId, playerId) == 1 && ChrominoDal.IsCameleon(chrominoInGame.ChrominoId))
            {
                playReturn = PlayReturn.LastChrominoIsCameleon; // interdit de jouer le denier chromino si c'est un caméléon
            }
            else
            {
                playReturn = SquareDal.PlayChromino(chrominoInGame, playerId);
            }
            if (playReturn == PlayReturn.Ok)
            {
                GamePlayerDal.SetPass(GameId, playerId, false);
                int points = ChrominoDal.Details(chrominoInGame.ChrominoId).Points;
                GamePlayerDal.AddPoints(GameId, playerId, points);
                if (Players.Count > 1)
                    PlayerDal.AddPoints(playerId, points);

                int numberInHand = GameChrominoDal.InHand(GameId, playerId);
                if (numberInHand == 0)
                {
                    SetGameFinished();
                    if (GamePlayerDal.PlayersNumber(GameId) > 1)
                    {
                        PlayerDal.IncreaseWin(playerId);
                        GamePlayerDal.SetWon(GameId, playerId);
                    }
                    else
                    {
                        int chrominosInGame = GameChrominoDal.InGame(GameId);
                        PlayerDal.Details(playerId).PointsSinglePlayerGames += chrominosInGame switch
                        {
                            8 => 100,
                            9 => 90,
                            10 => 85,
                            11 => 82,
                            _ => 92 - chrominosInGame,
                        };
                        PlayerDal.Details(playerId).FinishedSinglePlayerGames++;
                    }
                    Ctx.SaveChanges();
                    // todo gérer fin de partie en laissant jouer les joueur du tour et voir s'ils peuvent finir
                }
                ChangePlayerTurn();
            }
            return playReturn;
        }

        /// <summary>
        /// retourne la couleur possible à cet endroit
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="adjacentChrominos"></param>
        /// <returns>Cameleon si toute couleur peut se poser ; null si aucune possible</returns>
        public Color? OkColorFor(Coordinate coordinate, out int adjacentChrominos)
        {
            HashSet<Color> colors = new HashSet<Color>();
            Color? rightColor = coordinate.GetRightColor(Squares);
            Color? bottomColor = coordinate.GetBottomColor(Squares);
            Color? leftColor = coordinate.GetLeftColor(Squares);
            Color? topColor = coordinate.GetTopColor(Squares);
            adjacentChrominos = 0;
            if (rightColor != null)
            {
                adjacentChrominos++;
                colors.Add((Color)rightColor);
            }
            if (bottomColor != null)
            {
                adjacentChrominos++;
                colors.Add((Color)bottomColor);
            }
            if (leftColor != null)
            {
                adjacentChrominos++;
                colors.Add((Color)leftColor);
            }
            if (topColor != null)
            {
                adjacentChrominos++;
                colors.Add((Color)topColor);
            }

            if (colors.Count == 0)
            {
                return Color.Cameleon;
            }
            else if (colors.Count == 1)
            {
                Color theColor = new Color();
                foreach (Color color in colors)
                {
                    theColor = color;
                }
                return theColor;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// liste (sans doublons) des coordonées libres ajdacentes à des squares occupés.
        /// les coordonées des derniers squares occupés sont remontées en premiers.
        /// </summary>
        /// <returns></returns>
        public HashSet<Coordinate> FreeAroundChrominos()
        {
            List<Square> squares = SquareDal.List(GameId);
            HashSet<Coordinate> coordinates = new HashSet<Coordinate>();
            foreach (Square square in squares)
            {
                Coordinate rightCoordinate = square.GetRight();
                Coordinate bottomCoordinate = square.GetBottom();
                Coordinate leftCoordinate = square.GetLeft();
                Coordinate topCoordinate = square.GetTop();
                if (SquareDal.IsFree(GameId, rightCoordinate))
                    coordinates.Add(rightCoordinate);
                if (SquareDal.IsFree(GameId, bottomCoordinate))
                    coordinates.Add(bottomCoordinate);
                if (SquareDal.IsFree(GameId, leftCoordinate))
                    coordinates.Add(leftCoordinate);
                if (SquareDal.IsFree(GameId, topCoordinate))
                    coordinates.Add(topCoordinate);
            }
            return coordinates;
        }

        private void FillHandPlayers()
        {
            foreach (GamePlayer gamePlayer in GamePlayers)
            {
                FillHand(gamePlayer);
            }
        }

        private void FillHand(GamePlayer gamePlayer)
        {
            for (int i = 0; i < MaxChrominoInHand; i++)
            {
                GameChrominoDal.StackToHand(GameId, gamePlayer.PlayerId);
            }
        }

        /// <summary>
        /// marque la partie terminée
        /// </summary>
        private void SetGameFinished()
        {
            if (GamePlayerDal.PlayersNumber(GameId) == 1)
                GameDal.SetStatus(GameId, GameStatus.SingleFinished);
            else
                GameDal.SetStatus(GameId, GameStatus.Finished);
        }
    }
}
