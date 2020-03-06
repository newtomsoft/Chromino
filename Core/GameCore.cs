using Data.DAL;
using Data.Enumeration;
using Data.Models;
using Data.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace Data.Core
{
    public class GameCore
    {
        /// <summary>
        /// nombre de chromino dans la main des joueurs en début de partie
        /// </summary>
        private const int BeginGameChrominoInHand = 8;

        /// <summary>
        /// DbContext du jeu
        /// </summary>
        private readonly Context Ctx;

        /// <summary>
        /// les différentes Dal du contexte utilisées
        /// </summary>
        private readonly GameChrominoDal GameChrominoDal;
        private readonly ChrominoDal ChrominoDal;
        private readonly SquareDal SquareDal;
        private readonly PlayerDal PlayerDal;
        private readonly GamePlayerDal GamePlayerDal;
        private readonly GameDal GameDal;

        /// <summary>
        /// Id du jeu
        /// </summary>
        private int GameId { get; set; }

        /// <summary>
        /// Liste de carré de la grille du jeu
        /// </summary>
        public List<Square> Squares { get; set; }

        /// <summary>
        /// Liste des joueurs du jeu
        /// </summary>
        public List<Player> Players { get; set; }

        /// <summary>
        /// List des GamePlayer du jeu (jointure entre Game et Player)
        /// </summary>
        public List<GamePlayer> GamePlayers { get; set; }

        public GameCore(Context ctx, int gameId)
        {
            GameId = gameId;
            Ctx = ctx;
            GameDal = new GameDal(ctx);
            GameChrominoDal = new GameChrominoDal(ctx);
            ChrominoDal = new ChrominoDal(ctx);
            SquareDal = new SquareDal(ctx);
            PlayerDal = new PlayerDal(ctx);
            GamePlayerDal = new GamePlayerDal(ctx);
            Players = GamePlayerDal.Players(gameId);
            Squares = SquareDal.List(gameId);
            GamePlayers = new List<GamePlayer>();
            foreach (Player player in Players)
            {
                GamePlayers.Add(GamePlayerDal.Details(gameId, player.Id));
            }
        }

        /// <summary>
        /// Commence une partie seul ou à plusieurs
        /// </summary>
        /// <param name="playerNumber"></param>
        public void BeginGame(int playerNumber)
        {
            if (playerNumber == 1)
                GameDal.SetStatus(GameId, GameStatus.SingleInProgress);
            else
                GameDal.SetStatus(GameId, GameStatus.InProgress);
            GameChrominoDal.FirstRandomToGame(GameId);
            MakePicture();
            FillHandPlayers();
            ChangePlayerTurn();
        }

        /// <summary>
        /// change le joueur dont c'est le tour de jouer
        /// </summary>
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
                                  orderby gp.Id descending
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
                Enumeration.Color? firstColor = OkColorFor(coordinate, out int commonSidesFirstColor);
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
                            Enumeration.Color? secondColor = OkColorFor(secondCoordinate, out int adjacentChrominoSecondColor);
                            Enumeration.Color? thirdColor = OkColorFor(thirdCoordinate, out int adjacentChrominosThirdColor);

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

            // cherche un chromino dans la main du bot correspondant à un possiblePosition
            List<ChrominoInHand> hand;
            bool previouslyDraw = GamePlayerDal.IsPreviouslyDraw(GameId, botId);
            if (previouslyDraw)
                hand = new List<ChrominoInHand> { GameChrominoDal.GetNewAddedChrominoInHand(GameId, botId) };
            else
                hand = GameChrominoDal.ChrominosByPriority(GameId, botId);

            ChrominoInGame goodChrominoInGame = null;
            Coordinate firstCoordinate = null;
            ChrominoInHand goodChrominoInHand;
            if (!previouslyDraw && hand.Count == 1 && ChrominoDal.IsCameleon(hand[0].ChrominoId)) // on ne peut pas poser un cameleon si c'est le dernier de la main
            {
                goodChrominoInGame = null;
            }
            else
            {
                SortHandToFinishedWithoutCameleon(ref hand);
                foreach (ChrominoInHand chrominoInHand in hand)
                {
                    foreach (PossiblesPositions possiblePosition in possiblesPositions)
                    {
                        Chromino chromino = ChrominoDal.Details(chrominoInHand.ChrominoId);
                        if ((chromino.FirstColor == possiblePosition.FirstColor || possiblePosition.FirstColor == Enumeration.Color.Cameleon) && (chromino.SecondColor == possiblePosition.SecondColor || chromino.SecondColor == Enumeration.Color.Cameleon || possiblePosition.SecondColor == Enumeration.Color.Cameleon) && (chromino.ThirdColor == possiblePosition.ThirdColor || possiblePosition.ThirdColor == Enumeration.Color.Cameleon))
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
                        else if ((chromino.FirstColor == possiblePosition.ThirdColor || possiblePosition.ThirdColor == Enumeration.Color.Cameleon) && (chromino.SecondColor == possiblePosition.SecondColor || chromino.SecondColor == Enumeration.Color.Cameleon || possiblePosition.SecondColor == Enumeration.Color.Cameleon) && (chromino.ThirdColor == possiblePosition.FirstColor || possiblePosition.FirstColor == Enumeration.Color.Cameleon))
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
            if (goodChrominoInGame == null) // pas de chromino à jouer
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
            else // chromino à jouer trouver
            {
                goodChrominoInGame.XPosition = firstCoordinate.X;
                goodChrominoInGame.YPosition = firstCoordinate.Y;
                PlayReturn playReturn = Play(goodChrominoInGame, botId);
                MakePicture();
                if (IsRoundLastPlayer(botId) && GamePlayerDal.IsSomePlayerWon(GameId))
                    SetGameFinished();
                return playReturn;
            }
        }

        /// <summary>
        /// Passe le tour
        /// Indique que la partie est terminée si c'est au dernier joueur du tour de jouer, 
        /// Indique que la partie est terminée s'il n'y a plus de chromino dans la pioche et que tous les joueurs ont passé
        /// </summary>
        /// <param name="playerId"></param>
        public void PassTurn(int playerId)
        {
            GamePlayerDal.SetPass(GameId, playerId, true);
            if (GameChrominoDal.InStack(GameId) == 0 && GamePlayerDal.IsAllPass(GameId) || (IsRoundLastPlayer(playerId) && GamePlayerDal.IsSomePlayerWon(GameId)))
                SetGameFinished();
            ChangePlayerTurn();
        }

        /// <summary>
        /// pioche un chromino et indique que le joueur à pioché au coup d'avant
        /// s'il n'y a plus de chrimino dans la pioche, passe le tour
        /// </summary>
        /// <param name="playerId">Id du joueur</param>
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
                playReturn = PlayReturn.NotPlayerTurn;
            else if (GameChrominoDal.PlayerNumberChrominos(GameId, playerId) == 1 && ChrominoDal.IsCameleon(chrominoInGame.ChrominoId))
                playReturn = PlayReturn.LastChrominoIsCameleon; // interdit de jouer le denier chromino si c'est un caméléon
            else
                playReturn = SquareDal.PlayChromino(chrominoInGame, playerId);
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
                    if (IsRoundLastPlayer(playerId))
                    {
                        SetGameFinished();
                    }
                }
                if (IsRoundLastPlayer(playerId) && GamePlayerDal.IsSomePlayerWon(GameId))
                    SetGameFinished();

                MakePicture();
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
        public Enumeration.Color? OkColorFor(Coordinate coordinate, out int adjacentChrominos)
        {
            HashSet<Enumeration.Color> colors = new HashSet<Enumeration.Color>();
            Enumeration.Color? rightColor = coordinate.GetRightColor(Squares);
            Enumeration.Color? bottomColor = coordinate.GetBottomColor(Squares);
            Enumeration.Color? leftColor = coordinate.GetLeftColor(Squares);
            Enumeration.Color? topColor = coordinate.GetTopColor(Squares);
            adjacentChrominos = 0;
            if (rightColor != null)
            {
                adjacentChrominos++;
                colors.Add((Enumeration.Color)rightColor);
            }
            if (bottomColor != null)
            {
                adjacentChrominos++;
                colors.Add((Enumeration.Color)bottomColor);
            }
            if (leftColor != null)
            {
                adjacentChrominos++;
                colors.Add((Enumeration.Color)leftColor);
            }
            if (topColor != null)
            {
                adjacentChrominos++;
                colors.Add((Enumeration.Color)topColor);
            }

            if (colors.Count == 0)
            {
                return Enumeration.Color.Cameleon;
            }
            else if (colors.Count == 1)
            {
                Enumeration.Color theColor = new Enumeration.Color();
                foreach (Enumeration.Color color in colors)
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

        /// <summary>
        /// rempli la main de tous les joueurs
        /// </summary>
        /// <param name="gamePlayer"></param>

        private void FillHandPlayers()
        {
            foreach (GamePlayer gamePlayer in GamePlayers)
            {
                FillHand(gamePlayer);
            }
        }

        /// <summary>
        /// rempli la main du joueur de chrominos
        /// </summary>
        /// <param name="gamePlayer"></param>
        private void FillHand(GamePlayer gamePlayer)
        {
            for (int i = 0; i < BeginGameChrominoInHand; i++)
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

        /// <summary>
        /// return true si l'id du joueur est l'id du dernier dans le tour
        /// </summary>
        /// <param name="playerId">id du joueur courant</param>
        /// <returns></returns>
        private bool IsRoundLastPlayer(int playerId)
        {
            int roundLastPlayerId = GamePlayerDal.RoundLastPlayerId(GameId);
            return playerId == roundLastPlayerId ? true : false;
        }

        /// <summary>
        /// change l'ordre des n chrominos de la main s'il y a n-1 cameleon
        /// afin de jouer les cameleon et finir avec un chromino normal
        /// </summary>
        /// <param name="hand">référence de la liste des chrominos de la main du joueur</param>
        private void SortHandToFinishedWithoutCameleon(ref List<ChrominoInHand> hand)
        {
            if (hand.Count > 1)
            {
                bool forcePlayCameleon = true;
                for (int i = 1; i < hand.Count; i++)
                {
                    if (!ChrominoDal.IsCameleon(hand[i].ChrominoId))
                    {
                        forcePlayCameleon = false;
                        break;
                    }
                }
                if (forcePlayCameleon)
                {
                    ChrominoInHand chrominoAt0 = hand[0];
                    hand.RemoveAt(0);
                    hand.Add(chrominoAt0);
                }
            }
        }

        /// <summary>
        /// créer le visuel du jeu
        /// </summary>
        /// <param name="gameId">Id du jeu</param>
        private void MakePicture()
        {
            //obtention des couleurs de différents carrés de l'image
            List<Square> squares = SquareDal.List(GameId);
            int xMin = squares.Select(g => g.X).Min() - 1;
            int xMax = squares.Select(g => g.X).Max() + 1;
            int yMin = squares.Select(g => g.Y).Min() - 1;
            int yMax = squares.Select(g => g.Y).Max() + 1;
            int columnsNumber = xMax - xMin + 1;
            int linesNumber = yMax - yMin + 1;
            int squaresNumber = columnsNumber * linesNumber;
            var squaresViewModel = new SquareVM[squaresNumber];
            for (int i = 0; i < squaresViewModel.Length; i++)
                squaresViewModel[i] = new SquareVM(Enumeration.Color.None, true, true, true, true);
            foreach (Square square in squares)
            {
                int index = square.Y * columnsNumber + square.X - (yMin * columnsNumber + xMin);
                squaresViewModel[index] = square.SquareViewModel;
            }

            const int imageSquareSize = 30;
            int width = columnsNumber * imageSquareSize;
            int height = linesNumber * imageSquareSize;
            Bitmap bitmap = new Bitmap(width, height);
            
            //construction de l'image
            for (int j = 0; j < linesNumber; j++)
            {
                for (int i = 0; i < columnsNumber; i++)
                {
                    int index = i + j * columnsNumber;
                    Enumeration.Color colorSquare = squaresViewModel[index].Color;
                    Enumeration.Color colorSquareLeft = i != 0 ? squaresViewModel[i - 1 + j * columnsNumber].Color : Enumeration.Color.None;
                    Enumeration.Color colorSquareRight = i != columnsNumber - 1 ? squaresViewModel[i + 1 + j * columnsNumber].Color : Enumeration.Color.None;
                    Enumeration.Color colorSquareTop = j != 0 ? squaresViewModel[i + (j - 1) * columnsNumber].Color : Enumeration.Color.None;
                    Enumeration.Color colorSquareBottom = j != linesNumber - 1 ? squaresViewModel[i + (j + 1) * columnsNumber].Color : Enumeration.Color.None;
                    bool openRight = squaresViewModel[index].OpenRight;
                    bool openBottom = squaresViewModel[index].OpenBottom;
                    bool openLeft = squaresViewModel[index].OpenLeft;
                    bool openTop = squaresViewModel[index].OpenTop;
                    for (int x = i * imageSquareSize; x < (i + 1) * imageSquareSize; x++)
                    {
                        for (int y = j * imageSquareSize; y < (j + 1) * imageSquareSize; y++)
                        {
                            if (x == i * imageSquareSize)
                            {
                                if (colorSquare != Enumeration.Color.None)
                                    bitmap.SetPixel(x, y, openLeft ? System.Drawing.Color.Gray : System.Drawing.Color.Black);
                                else
                                    bitmap.SetPixel(x, y, colorSquareLeft == Enumeration.Color.None ? System.Drawing.Color.Transparent : System.Drawing.Color.Black);
                            }
                            else if (x == (i + 1) * imageSquareSize - 1)
                            {
                                if (colorSquare != Enumeration.Color.None)
                                    bitmap.SetPixel(x, y, openRight ? System.Drawing.Color.Gray : System.Drawing.Color.Black);
                                else
                                    bitmap.SetPixel(x, y, colorSquareRight == Enumeration.Color.None ? System.Drawing.Color.Transparent : System.Drawing.Color.Black);
                            }
                            else if (y == j * imageSquareSize)
                            {
                                if (colorSquare != Enumeration.Color.None)
                                    bitmap.SetPixel(x, y, openTop ? System.Drawing.Color.Gray : System.Drawing.Color.Black);
                                else
                                    bitmap.SetPixel(x, y, colorSquareTop == Enumeration.Color.None ? System.Drawing.Color.Transparent : System.Drawing.Color.Black);
                            }
                            else if (y == (j + 1) * imageSquareSize - 1)
                            {
                                if (colorSquare != Enumeration.Color.None)
                                    bitmap.SetPixel(x, y, openBottom ? System.Drawing.Color.Gray : System.Drawing.Color.Black);
                                else
                                    bitmap.SetPixel(x, y, colorSquareBottom == Enumeration.Color.None ? System.Drawing.Color.Transparent : System.Drawing.Color.Black);
                            }
                            else
                            {
                                bitmap.SetPixel(x, y, ColorToColor(colorSquare));
                            }
                        }
                    }
                }
            }
            string filename = $"game{GameId}.png"; //todo définir chemin
            bitmap.Save(filename, ImageFormat.Png);
        }

        private System.Drawing.Color ColorToColor(Enumeration.Color color)
        {
            return color switch
            {
                Enumeration.Color.Blue => System.Drawing.Color.FromArgb(58, 194, 238),
                Enumeration.Color.Green => System.Drawing.Color.FromArgb(76, 174, 68),
                Enumeration.Color.Purple => System.Drawing.Color.FromArgb(86, 27, 108),
                Enumeration.Color.Red => System.Drawing.Color.FromArgb(250, 44, 46),
                Enumeration.Color.Yellow => System.Drawing.Color.FromArgb(255, 235, 71),
                Enumeration.Color.Cameleon => System.Drawing.Color.AntiqueWhite,
                _ => System.Drawing.Color.Transparent,
            };
        }

    }
}