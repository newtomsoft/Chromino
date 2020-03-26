using Data.DAL;
using Data.Enumeration;
using Data.Models;
using Data.ViewModel;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;

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
        /// variables d'environnement
        /// </summary>
        private readonly IWebHostEnvironment Env;

        /// <summary>
        /// les différentes Dal utilisées du context 
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

        public GameCore(Context ctx, IWebHostEnvironment env, int gameId)
        {
            Env = env;
            Ctx = ctx;
            GameId = gameId;
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
            ChrominoInGame chrominoInGame = GameChrominoDal.FirstRandomToGame(GameId);
            PlayChromino(chrominoInGame, null);
            MakeThumbnail();
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
            bool previouslyDraw = GamePlayerDal.IsPreviouslyDraw(GameId, botId);
            int playersNumber = GamePlayerDal.PlayersNumber(GameId);
            if (GamePlayerDal.IsAllPass(GameId) && (!previouslyDraw || playersNumber == 1))
            {
                DrawChromino(botId);
                return PlayReturn.DrawChromino;
            }
            List<Square> squaresInGame = SquareDal.List(GameId);
            HashSet<Coordinate> coordinates = FreeAroundSquares(squaresInGame);
            List<Position> positions = ComputePossiblesPositions(coordinates);

            // cherche un chromino dans la main du bot correspondant à un possiblePosition
            List<ChrominoInHand> hand;
            if (previouslyDraw)
                hand = new List<ChrominoInHand> { GameChrominoDal.GetNewAddedChrominoInHand(GameId, botId) };
            else
                hand = GameChrominoDal.ChrominosByPriority(GameId, botId);

            bool firstSearch = true;
            ChrominoInGame bestChrominoToPlay = null;
            ChrominoInGame previouslyChrominoToPlay = null;
            int indexChrominoToPlay = 0;
            while (bestChrominoToPlay == null)
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

                ChrominoInGame currentChrominoToPlay = ComputeChrominoToPlay(hand, previouslyDraw, positions, out indexChrominoToPlay, out Position position);
                if (currentChrominoToPlay == null)
                    break;
                else // le bot peut jouer
                {
                    previouslyChrominoToPlay = currentChrominoToPlay;
                    List<int> playerIdWithLastChrominoIdInHand = GameChrominoDal.PlayersIdWithOneChrominoKnown(GameId, botId);
                    if (playerIdWithLastChrominoIdInHand != null && playerIdWithLastChrominoIdInHand.Count == 1 && GameChrominoDal.InHand(GameId, playerIdWithLastChrominoIdInHand[0]) == 1)
                    {
                        // 1 seul adversaire n'a plus qu'un chromino
                        List<Square> squaresAfterTry = ComputeSquares(currentChrominoToPlay);
                        squaresAfterTry.AddRange(squaresInGame);
                        HashSet<Coordinate> coordinatesAfterTry = FreeAroundSquares(squaresAfterTry);
                        List<Position> positionsAfterTry = ComputePossiblesPositions(coordinatesAfterTry);
                        Position positionWhereOpponentCanPlayAfterTry = PositionWherePlayerCanPlay(playerIdWithLastChrominoIdInHand[0], positionsAfterTry);

                        int numberChrominosInHand = GameChrominoDal.InHand(GameId, botId);
                        if (PositionWherePlayerCanPlay(playerIdWithLastChrominoIdInHand[0], positions) != null)
                        {
                            // l'adversaire peut finir après le tour du bot
                            if (currentChrominoToPlay != null && numberChrominosInHand == 1)
                            {
                                // le bot peut finir ce tour => il joue
                                bestChrominoToPlay = currentChrominoToPlay;
                            }
                            else if (false) // todo le bot joue après l'adversaire et il peut finir le coup d'après => il joue
                            {
                                bestChrominoToPlay = currentChrominoToPlay;
                            }
                            else if (positionWhereOpponentCanPlayAfterTry == null)
                            {
                                // todo : le bot doit tenter de jouer pour le bloquer
                                bestChrominoToPlay = currentChrominoToPlay;
                            }
                        }
                        else
                        {
                            // l'adversaire ne peut pas jouer en l'état
                            if (currentChrominoToPlay != null && numberChrominosInHand == 1)
                            {
                                // le bot peut finir ce tour => il joue
                                bestChrominoToPlay = currentChrominoToPlay;
                            }
                            else if (false) // todo le bot peut finir le coup d'après : il joue
                            {
                                bestChrominoToPlay = currentChrominoToPlay;
                            }
                            else if (positionWhereOpponentCanPlayAfterTry == null)
                            {
                                // le bot ne finit pas le coup d'après : le bot joue sans faire gagner l'adversaire
                                bestChrominoToPlay = currentChrominoToPlay;
                            }
                        }
                    }
                    else
                    {
                        // plus d'un adversaire a 1 chromino.
                        // v1 : le bot joue
                        //todo : prévoir stratégie de jeu
                        bestChrominoToPlay = currentChrominoToPlay;
                    }
                }
            }

            if (previouslyChrominoToPlay == null) // pas de chromino à jouer
            {
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
                if (bestChrominoToPlay == null)
                    bestChrominoToPlay = previouslyChrominoToPlay;
                PlayReturn playReturn = Play(bestChrominoToPlay, botId);
                MakeThumbnail();
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
        public void SkipTurn(int playerId)
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
                SkipTurn(playerId);
            else
                GamePlayerDal.SetPreviouslyDraw(GameId, playerId);
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

                MakeThumbnail();
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
            Enumeration.Color? rightColor = coordinate.RightColor(Squares);
            Enumeration.Color? bottomColor = coordinate.BottomColor(Squares);
            Enumeration.Color? leftColor = coordinate.LeftColor(Squares);
            Enumeration.Color? topColor = coordinate.TopColor(Squares);
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
        /// liste (sans doublons) des coordonées libres ajdacentes aux squares occupés.
        /// les coordonées des derniers squares occupés sont remontées en premiers.
        /// </summary>
        /// <param name="squares">liste de squares</param>
        /// <returns></returns>
        public HashSet<Coordinate> FreeAroundSquares(List<Square> squares)
        {
            HashSet<Coordinate> coordinates = new HashSet<Coordinate>();
            foreach (Square square in squares)
            {
                Coordinate rightCoordinate = square.GetRight();
                Coordinate bottomCoordinate = square.GetBottom();
                Coordinate leftCoordinate = square.GetLeft();
                Coordinate topCoordinate = square.GetTop();
                if (IsFree(ref squares, rightCoordinate))
                    coordinates.Add(rightCoordinate);
                if (IsFree(ref squares, bottomCoordinate))
                    coordinates.Add(bottomCoordinate);
                if (IsFree(ref squares, leftCoordinate))
                    coordinates.Add(leftCoordinate);
                if (IsFree(ref squares, topCoordinate))
                    coordinates.Add(topCoordinate);
            }
            return coordinates;
        }

        /// <summary>
        /// créer le visuel du jeu
        /// </summary>
        /// <param name="gameId">Id du jeu</param>
        public void MakeThumbnail()
        {
            //obtention des couleurs de différents carrés de l'image
            List<Square> squares = SquareDal.List(GameId);
            if (squares.Count == 0)
                return;

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

            //construction de l'image
            const int imageSquareSize = 32;
            int width = columnsNumber * imageSquareSize;
            int height = linesNumber * imageSquareSize;
            Bitmap thumbnail = new Bitmap(width, height);

            string cameleonFullFileName = Path.Combine(Env.WebRootPath, @"image/Cameleon.png");
            Bitmap cameleonBitmap = new Bitmap(cameleonFullFileName);
            if (cameleonBitmap.Width != imageSquareSize - 2)
                cameleonBitmap = new Bitmap(cameleonBitmap, new Size(imageSquareSize - 2, imageSquareSize - 2));

            for (int j = 0; j < linesNumber; j++)
            {
                for (int i = 0; i < columnsNumber; i++)
                {
                    bool firstCameleonPixel = true;
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
                                    thumbnail.SetPixel(x, y, openLeft ? System.Drawing.Color.Gray : System.Drawing.Color.Black);
                                else
                                    thumbnail.SetPixel(x, y, colorSquareLeft == Enumeration.Color.None ? System.Drawing.Color.Transparent : System.Drawing.Color.Black);
                            }
                            else if (x == (i + 1) * imageSquareSize - 1)
                            {
                                if (colorSquare != Enumeration.Color.None)
                                    thumbnail.SetPixel(x, y, openRight ? System.Drawing.Color.Gray : System.Drawing.Color.Black);
                                else
                                    thumbnail.SetPixel(x, y, colorSquareRight == Enumeration.Color.None ? System.Drawing.Color.Transparent : System.Drawing.Color.Black);
                            }
                            else if (y == j * imageSquareSize)
                            {
                                if (colorSquare != Enumeration.Color.None)
                                    thumbnail.SetPixel(x, y, openTop ? System.Drawing.Color.Gray : System.Drawing.Color.Black);
                                else
                                    thumbnail.SetPixel(x, y, colorSquareTop == Enumeration.Color.None ? System.Drawing.Color.Transparent : System.Drawing.Color.Black);
                            }
                            else if (y == (j + 1) * imageSquareSize - 1)
                            {
                                if (colorSquare != Enumeration.Color.None)
                                    thumbnail.SetPixel(x, y, openBottom ? System.Drawing.Color.Gray : System.Drawing.Color.Black);
                                else
                                    thumbnail.SetPixel(x, y, colorSquareBottom == Enumeration.Color.None ? System.Drawing.Color.Transparent : System.Drawing.Color.Black);
                            }
                            else
                            {
                                if (colorSquare == Enumeration.Color.Cameleon)
                                {
                                    if (firstCameleonPixel)
                                    {
                                        firstCameleonPixel = false;
                                        CopyPasteCameleonBitmap(ref thumbnail, ref cameleonBitmap, x, y);
                                    }
                                }
                                else
                                {
                                    thumbnail.SetPixel(x, y, EnumColorToColor(colorSquare));
                                }
                            }
                        }
                    }
                }
            }
            string thumbnailFullName = Path.Combine(Env.WebRootPath, @"image/game", $"{GameDal.Details(GameId).Guid}.png");
            // augmentation de la taille du canvas si pas assez de squares dans l'image
            const int minColumnsDisplayed = 15;
            const int minLinesDisplayed = minColumnsDisplayed;
            if (columnsNumber < minColumnsDisplayed || linesNumber < minLinesDisplayed)
            {
                int newWidth = Math.Max(minColumnsDisplayed * imageSquareSize, width);
                int newHeight = Math.Max(minLinesDisplayed * imageSquareSize, height);
                Bitmap resizedthumbnail = new Bitmap(newWidth, newHeight);
                Graphics graphics = Graphics.FromImage(resizedthumbnail);
                graphics.FillRectangle(Brushes.Transparent, 0, 0, newWidth, newHeight);
                graphics.DrawImage(thumbnail, (newWidth - thumbnail.Width) / 2, (newHeight - thumbnail.Height) / 2, thumbnail.Width, thumbnail.Height);
                thumbnail = new Bitmap(resizedthumbnail, newWidth / 2, newHeight / 2);
            }
            else
            {
                thumbnail = new Bitmap(thumbnail, width / 2, height / 2);
            }
            thumbnail.Save(thumbnailFullName, ImageFormat.Png);
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
        /// converti le Enumeration.Color en System.Drawing.Color
        /// </summary>
        /// <param name="color">couleur en Enumeration.Color</param>
        /// <returns>couleur en System.Drawing.Color</returns>
        private System.Drawing.Color EnumColorToColor(Enumeration.Color color)
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

        /// <summary>
        /// rempli la zone correspondant à un caméléon par l'image du caméléon
        /// </summary>
        /// <param name="thumbnail">bitmap de la vignette du jeu</param>
        /// <param name="bitmapCameleon">bitmap du caméléon</param>
        /// <param name="x">coordonnée x du coin supérieur gauche de la zone à remplir</param>
        /// <param name="y">coordonnée y du coin supérieur gauche de la zone à remplir</param>
        private void CopyPasteCameleonBitmap(ref Bitmap thumbnail, ref Bitmap bitmapCameleon, int x, int y)
        {
            for (int j = 0; j < bitmapCameleon.Height; j++)
            {
                for (int i = 0; i < bitmapCameleon.Width; i++)
                {
                    thumbnail.SetPixel(x + i, y + j, bitmapCameleon.GetPixel(i, j));
                }
            }
        }

        /// <summary>
        /// return true si l'id du joueur est l'id du dernier joueur dans le tour
        /// </summary>
        /// <param name="playerId">id du joueur courant</param>
        /// <returns></returns>
        private bool IsRoundLastPlayer(int playerId)
        {
            int roundLastPlayerId = GamePlayerDal.RoundLastPlayerId(GameId);
            return playerId == roundLastPlayerId ? true : false;
        }

        /// <summary>
        /// indique si les autres joueurs à suivre dans le tour peuvent potentiellement gagner
        /// </summary>
        /// <param name="playerId">Id du joueur à partir duquel les autres joueurs sont déterminés</param>
        /// <returns></returns>
        private bool IsNextPlayersCanWin(int playerId)
        {
            foreach (int currentPlayerId in GamePlayerDal.NextPlayersId(GameId, playerId))
            {
                if (GameChrominoDal.InHand(GameId, currentPlayerId) == 1)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// retourne la position où le joueur peut jouer parmis la liste passée en paramètre
        /// </summary>
        /// <param name="playerId">Id du joueur</param>
        /// <param name="positions">liste des positions candidates</param>
        /// <returns>Position retenue. null si aucune</returns>
        private Position PositionWherePlayerCanPlay(int playerId, List<Position> positions)
        {
            List<ChrominoInHand> hand = GameChrominoDal.Hand(GameId, playerId);
            ComputeChrominoToPlay(hand, false, positions, out _, out Position position);
            return position;
        }

        private List<Position> ComputePossiblesPositions(HashSet<Coordinate> coordinates)
        {
            List<Square> squaresInGame = SquareDal.List(GameId);
            List<Position> positions = new List<Position>();
            // prendre ceux avec un seul coté en commun avec un chromino
            // calculer orientation avec les couleurs imposées ou pas
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
                        if (IsFree(ref squaresInGame, secondCoordinate) && IsFree(ref squaresInGame, thirdCoordinate))
                        {
                            //calcul si chromino posable et dans quelle position
                            Enumeration.Color? secondColor = OkColorFor(secondCoordinate, out int adjacentChrominoSecondColor);
                            Enumeration.Color? thirdColor = OkColorFor(thirdCoordinate, out int adjacentChrominosThirdColor);

                            if (secondColor != null && thirdColor != null && commonSidesFirstColor + adjacentChrominoSecondColor + adjacentChrominosThirdColor >= 2)
                            {
                                Position possibleSpace = new Position
                                {
                                    Coordinate = coordinate,
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
            return positions;
        }

        /// <summary>
        /// Indique si avec les chrominos passés en paramètre, il est possible de jouer un de ces chrominos et le retourne
        /// </summary>
        /// <param name="hand">liste des chrominos à tester</param>
        /// <param name="previouslyDraw">indique si la joeur vient de piocher avant de tester</param>
        /// <param name="positions">liste des positions où chercher à placer le chromino</param>
        /// <param name="indexInHand">index du chromino convenant</param>
        /// <param name="position">position si convenant, null sinon</param>
        /// <returns>null si aucun chromino ne couvient</returns>
        private ChrominoInGame ComputeChrominoToPlay(List<ChrominoInHand> hand, bool previouslyDraw, List<Position> positions, out int indexInHand, out Position position)
        {
            ChrominoInGame goodChromino = null;
            Coordinate firstCoordinate;
            ChrominoInHand goodChrominoInHand;
            indexInHand = -1;
            position = null;
            if (!previouslyDraw && hand.Count == 1 && ChrominoDal.IsCameleon(hand[0].ChrominoId)) // on ne peut pas poser un cameleon si c'est le dernier de la main
            {
                goodChromino = null;
            }
            else
            {
                SortHandToFinishedWithoutCameleon(ref hand);
                foreach (ChrominoInHand chrominoInHand in hand)
                {
                    indexInHand++;
                    foreach (Position currentPosition in positions)
                    {
                        Chromino chromino = ChrominoDal.Details(chrominoInHand.ChrominoId);
                        if ((chromino.FirstColor == currentPosition.FirstColor || currentPosition.FirstColor == Enumeration.Color.Cameleon) && (chromino.SecondColor == currentPosition.SecondColor || chromino.SecondColor == Enumeration.Color.Cameleon || currentPosition.SecondColor == Enumeration.Color.Cameleon) && (chromino.ThirdColor == currentPosition.ThirdColor || currentPosition.ThirdColor == Enumeration.Color.Cameleon))
                        {
                            goodChrominoInHand = chrominoInHand;
                            firstCoordinate = currentPosition.Coordinate;
                            goodChromino = new ChrominoInGame()
                            {
                                Orientation = currentPosition.Orientation,
                                ChrominoId = chromino.Id,
                                GameId = GameId,
                                XPosition = firstCoordinate.X,
                                YPosition = firstCoordinate.Y,
                            };
                            position = currentPosition;
                            break;
                        }
                        else if ((chromino.FirstColor == currentPosition.ThirdColor || currentPosition.ThirdColor == Enumeration.Color.Cameleon) && (chromino.SecondColor == currentPosition.SecondColor || chromino.SecondColor == Enumeration.Color.Cameleon || currentPosition.SecondColor == Enumeration.Color.Cameleon) && (chromino.ThirdColor == currentPosition.FirstColor || currentPosition.FirstColor == Enumeration.Color.Cameleon))
                        {
                            goodChrominoInHand = chrominoInHand;
                            firstCoordinate = currentPosition.Coordinate.GetNextCoordinate(currentPosition.Orientation).GetNextCoordinate(currentPosition.Orientation);
                            goodChromino = new ChrominoInGame()
                            {
                                Orientation = currentPosition.Orientation switch
                                {
                                    Orientation.Horizontal => Orientation.HorizontalFlip,
                                    Orientation.HorizontalFlip => Orientation.Horizontal,
                                    Orientation.Vertical => Orientation.VerticalFlip,
                                    _ => Orientation.Vertical,
                                },
                                ChrominoId = chromino.Id,
                                GameId = GameId,
                                XPosition = firstCoordinate.X,
                                YPosition = firstCoordinate.Y,
                            };
                            position = currentPosition;
                            break;
                        }
                    }
                    if (goodChromino != null)
                        break;
                }
            }
            return goodChromino;
        }

        private List<Square> ComputeSquares(ChrominoInGame chrominoIG)
        {
            int gameId = chrominoIG.GameId;
            int xOrigin = chrominoIG.XPosition;
            int yOrigin = chrominoIG.YPosition;
            Chromino chromino = ChrominoDal.Details(chrominoIG.ChrominoId);
            List<Square> squares = new List<Square>() { new Square {GameId = gameId, X = xOrigin, Y = yOrigin, Color=chromino.FirstColor } };
            switch (chrominoIG.Orientation)
            {
                case Orientation.Horizontal:
                    squares.Add(new Square { GameId = gameId, X = xOrigin + 1, Y = yOrigin, Color = chromino.SecondColor });
                    squares.Add(new Square { GameId = gameId, X = xOrigin + 2, Y = yOrigin, Color = chromino.ThirdColor });
                    break;
                case Orientation.HorizontalFlip:
                    squares.Add(new Square { GameId = gameId, X = xOrigin - 1, Y = yOrigin, Color = chromino.SecondColor });
                    squares.Add(new Square { GameId = gameId, X = xOrigin - 2, Y = yOrigin, Color = chromino.ThirdColor });
                    break;
                case Orientation.Vertical:
                    squares.Add(new Square { GameId = gameId, X = xOrigin, Y = yOrigin - 1, Color = chromino.SecondColor });
                    squares.Add(new Square { GameId = gameId, X = xOrigin, Y = yOrigin - 2, Color = chromino.ThirdColor });
                    break;
                case Orientation.VerticalFlip:
                    squares.Add(new Square { GameId = gameId, X = xOrigin, Y = yOrigin + 1, Color = chromino.SecondColor });
                    squares.Add(new Square { GameId = gameId, X = xOrigin, Y = yOrigin + 2, Color = chromino.ThirdColor });
                    break;
            }
            return squares;
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

        /// <summary>
        /// indique si à la coordonnée donnée, l'emplacement est libre
        /// </summary>
        /// <param name="squares">ensemble de squares dans lequel porter la recherche</param>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public bool IsFree(ref List<Square> squares, Coordinate coordinate)
        {
            Square result = (from s in squares
                             where s.X == coordinate.X && s.Y == coordinate.Y
                             select s).FirstOrDefault();

            return result == null ? true : false;
        }
    }
}