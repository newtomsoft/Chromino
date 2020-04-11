using ChrominoBI;
using Core;
using Data.DAL;
using Data.Enumeration;
using Data.Models;
using Data.ViewModel;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tool;

namespace Data.Core
{
    public partial class GameBI
    {
        /// <summary>
        /// DbContext du jeu
        /// </summary>
        private readonly Context Ctx;

        /// <summary>
        /// variables d'environnement
        /// </summary>
        private readonly IWebHostEnvironment Env;

        /// <summary>
        /// Id du jeu
        /// </summary>
        private int GameId { get; set; }

        /// <summary>
        /// Liste des joueurs du jeu
        /// </summary>
        public List<Player> Players { get; set; }

        /// <summary>
        /// List des GamePlayer du jeu (jointure entre Game et Player)
        /// </summary>
        public List<GamePlayer> GamePlayers { get; set; }

        private GoodPositionBI GoodPositionBI { get; set; }

        /// <summary>
        /// les différentes Dal utilisées du context 
        /// </summary>
        private readonly ChrominoInGameDal ChrominoInGameDal;
        private readonly ChrominoInHandDal ChrominoInHandDal;
        private readonly ChrominoDal ChrominoDal;
        private readonly SquareDal SquareDal;
        private readonly PlayerDal PlayerDal;
        private readonly GamePlayerDal GamePlayerDal;
        private readonly GameDal GameDal;

        public GameBI(Context ctx, IWebHostEnvironment env, int gameId)
        {
            Env = env;
            Ctx = ctx;
            GameId = gameId;
            GameDal = new GameDal(ctx);
            ChrominoInGameDal = new ChrominoInGameDal(ctx);
            ChrominoInHandDal = new ChrominoInHandDal(ctx);
            ChrominoDal = new ChrominoDal(ctx);
            SquareDal = new SquareDal(ctx);
            PlayerDal = new PlayerDal(ctx);
            GamePlayerDal = new GamePlayerDal(ctx);
            GoodPositionBI = new GoodPositionBI(ctx, GameId);
            Players = GamePlayerDal.Players(gameId);
            GamePlayers = new List<GamePlayer>();
            foreach (Player player in Players)
                GamePlayers.Add(GamePlayerDal.Details(gameId, player.Id));
        }

        /// <summary>
        /// Commence une partie
        /// </summary>
        /// <param name="playerNumber"></param>
        public void BeginGame(int playerNumber)
        {
            if (playerNumber == 1)
                GameDal.SetStatus(GameId, GameStatus.SingleInProgress);
            else
                GameDal.SetStatus(GameId, GameStatus.InProgress);
            ChrominoInGame chrominoInGame = ChrominoInGameDal.FirstToGame(GameId);
            PlayerBI playerBI = new PlayerBI(Ctx, Env, GameId, 0);
            playerBI.Play(chrominoInGame);

            foreach (GamePlayer currentGamePlayer in GamePlayers)
                FillHand(currentGamePlayer);
            GoodPositionBI.UpdateAllPlayersWholeGame();

            new PictureFactoryTool(GameId, Path.Combine(Env.WebRootPath, "image/game"), Ctx).MakeThumbnail();
            playerBI.ChangePlayerTurn();
        }

        public GameVM GameViewModel(int playerId, bool isAdmin)
        {
            Player playerTurn = GamePlayerDal.PlayerTurn(GameId);
            List<Player> players = GamePlayerDal.Players(GameId);
            if (isAdmin || players.Where(x => x.Id == playerId).FirstOrDefault() != null || GamePlayerDal.IsAllBots(GameId))
            {
                // je joueur est Admin, ou est dans la partie ou c'est une partie entre bots
                int chrominosInStackNumber = ChrominoInGameDal.InStack(GameId);
                string playerPseudo = PlayerDal.Pseudo(playerId);
                Dictionary<string, int> pseudosChrominos = new Dictionary<string, int>();
                List<string> pseudos = new List<string>();
                foreach (Player player in players)
                {
                    pseudosChrominos.Add(player.UserName, ChrominoInHandDal.ChrominosNumber(GameId, player.Id));
                    pseudos.Add(player.UserName);
                }
                Dictionary<string, Chromino> pseudos_lastChrominos = new Dictionary<string, Chromino>();
                foreach (var pseudo_chromino in pseudosChrominos)
                {
                    if (pseudo_chromino.Value == 1)
                        pseudos_lastChrominos.Add(pseudo_chromino.Key, ChrominoInHandDal.FirstChromino(GameId, GamePlayerDal.PlayerId(GameId, pseudo_chromino.Key)));
                }
                List<Chromino> identifiedPlayerChrominos;
                if (!GamePlayerDal.IsPlayerIdIn(GameId, playerId)) // si le joueur n'est pas dans la partie, il regarde la main du joueur dont c'est le tour de jouer
                    identifiedPlayerChrominos = ChrominoDal.PlayerChrominos(GameId, playerTurn.Id);
                else
                    identifiedPlayerChrominos = ChrominoDal.PlayerChrominos(GameId, playerId);
                if (GameDal.IsFinished(GameId) && !GamePlayerDal.GetViewFinished(GameId, playerId))
                {
                    GamePlayerDal.SetViewFinished(GameId, playerId);
                    if (GamePlayerDal.GetWon(GameId, playerId) == null)
                        GamePlayerDal.SetWon(GameId, playerId, false);
                }
                GamePlayer gamePlayerTurn = GamePlayerDal.Details(GameId, playerTurn.Id);
                GamePlayer gamePlayerIdentified = GamePlayerDal.Details(GameId, playerId);
                List<Square> squares = SquareDal.List(GameId);
                List<int> botsId = PlayerDal.BotsId();
                bool noTips = PlayerDal.Details(playerId).NoTips;
                List<ChrominoInGame> chrominosInGamePlayed = ChrominoInGameDal.List(GameId);
                Game game = GameDal.Details(GameId);
                bool opponenentsAreBots = GamePlayerDal.IsOpponenentsAreBots(GameId, playerId);
                GameVM gameViewModel = new GameVM(game, playerPseudo, playerId, squares, chrominosInStackNumber, pseudosChrominos, identifiedPlayerChrominos, playerTurn, gamePlayerTurn, gamePlayerIdentified, botsId, pseudos_lastChrominos, chrominosInGamePlayed, pseudos, opponenentsAreBots, noTips);
                return gameViewModel;
            }
            else return null;
        }

        /// <summary>
        /// marque la partie terminée
        /// </summary>
        public void SetGameFinished()
        {
            if (GamePlayerDal.PlayersNumber(GameId) == 1)
                GameDal.SetStatus(GameId, GameStatus.SingleFinished);
            else
                GameDal.SetStatus(GameId, GameStatus.Finished);
        }

        /// <summary>
        /// rempli la main du ou de tous les joueurs, de chrominos
        /// </summary>
        /// <param name="gamePlayer">le joueur concerné</param>
        private void FillHand(GamePlayer gamePlayer)
        {
            for (int i = 0; i < HandBI.StartChrominosNumber; i++)
                ChrominoInHandDal.FromStack(GameId, gamePlayer.PlayerId);
        }
    }
}