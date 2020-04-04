using Core;
using Data;
using Data.Core;
using Data.DAL;
using Data.Enumeration;
using Data.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tool;

namespace ChrominoBI
{
    public class BotBI : PlayerBI
    {
        public BotBI(Context ctx, IWebHostEnvironment env, int gameId, int botId) : base(ctx, env, gameId, botId)
        {
        }


        /// <summary>
        /// fait jouer le bot
        /// </summary>
        /// <returns></returns>
        public PlayReturn PlayBot()
        {
            List<ChrominoInHand> chrominosInHand = ChrominoInHandDal.ChrominosByPriority(GameId, PlayerId);
            HandBI handBI = new HandBI(Ctx, chrominosInHand);
            int chrominoIdNotToPlay = handBI.ChrominoIdIfSingleWithCameleons();
            List<GoodPosition> goodPositions = ComputedChrominosDal.RootListByPriority(GameId, PlayerId, chrominoIdNotToPlay);
            bool previouslyDraw = GamePlayerDal.IsPreviouslyDraw(GameId, PlayerId);
            int playersNumber = GamePlayerDal.PlayersNumber(GameId);

            if (goodPositions.Count == 0)
            {
                if ((!previouslyDraw || playersNumber == 1) && TryDrawChromino(out PlayReturn playreturn))
                    return playreturn;
                else
                    return SkipTurn();
            }
            // le bot a un ou des chriminos pouvant être posés
            List<Square> squares = SquareDal.List(GameId);
            List<Position> opponentPotentialPositions = GoodPositionBI.PotentialPositions(squares).ToList();
            List<int> opponentIdWithOneChromino = ChrominoInHandDal.PlayersIdWithOneChromino(GameId, PlayerId);
            List<int> opponentIdWithOneChrominoKnown = ChrominoInHandLastDal.PlayersIdWithOneChrominoKnown(GameId, PlayerId);
            List<ChrominoInGame> goodChrominos = new List<ChrominoInGame>();
            List<ChrominoInGame> badChrominos = new List<ChrominoInGame>();
            List<ChrominoInGame> neitherGoodNorBadChrominos = new List<ChrominoInGame>();
            List<ChrominoInGame> openToOpponentChrominos = new List<ChrominoInGame>();
            foreach (GoodPosition goodPosition in goodPositions)
            {
                ChrominoInGame currentPotentialChromino = ChrominoInGameBI.ChrominoInGameFrom(goodPosition);
                if (opponentIdWithOneChromino.Count == 0) // aucun adversaire a 1 chromino.
                {
                    if (opponentIdWithOneChrominoKnown.Count == 0) // aucun adversaire a 1 chromino connu.
                    {
                        goodChrominos.Add(currentPotentialChromino);
                    }
                    else if (opponentIdWithOneChrominoKnown.Count == 1)// 1 adversaire a 1 chromino connu.
                    {
                        int opponentId = opponentIdWithOneChromino[0];
                        ClassifyChrominos(opponentId, currentPotentialChromino, opponentPotentialPositions, squares, ref goodChrominos, ref badChrominos, ref neitherGoodNorBadChrominos, ref openToOpponentChrominos);
                    }
                    else // plusieurs adversaire a 1 chromino connu.
                    {
                        // todo
                        //v1
                        goodChrominos.Add(currentPotentialChromino);
                    }
                }
                else if (opponentIdWithOneChromino.Count == 1)
                {
                    // 1 seul adversaire n'a plus qu'un chromino
                    int opponentId = opponentIdWithOneChromino[0];
                    ClassifyChrominos(opponentId, currentPotentialChromino, opponentPotentialPositions, squares, ref goodChrominos, ref badChrominos, ref neitherGoodNorBadChrominos, ref openToOpponentChrominos);
                }
                else // plus d'un adversaire a 1 chromino.
                {
                    // v1 : le bot joue
                    //todo : prévoir stratégie de jeu
                    goodChrominos.Add(currentPotentialChromino);
                }
            }
            if (goodChrominos.Count == 0 && openToOpponentChrominos.Count == 0 && (!previouslyDraw || playersNumber == 1) && TryDrawChromino(out PlayReturn playReturn))
                { }
            else if (goodChrominos.Count == 0 && neitherGoodNorBadChrominos.Count > 0)
                playReturn = Play(neitherGoodNorBadChrominos[0]);
            else if (goodChrominos.Count == 0 && openToOpponentChrominos.Count > 0 &&  !IsRisky(openToOpponentChrominos, out int index) )
                playReturn = Play(openToOpponentChrominos[index]);
            else if (goodChrominos.Count != 0)
                playReturn = Play(goodChrominos[0]);
            else
                playReturn = SkipTurn();

            if (playReturn.IsError())
                GoodPositionBI.RemoveBadEntrie(goodChrominos[0], PlayerId);
            else if (playReturn == PlayReturn.Ok)
                new PictureFactoryTool(GameId, Path.Combine(Env.WebRootPath, "image/game"), Ctx).MakeThumbnail();

            if (IsRoundLastPlayer() && GamePlayerDal.IsSomePlayerWon(GameId))
                playReturn = PlayReturn.GameFinish;

            return playReturn;
        }

        private void ClassifyChrominos(int opponentId, ChrominoInGame potentialChromino, List<Position> opponentPotentialPositions, List<Square> squares, ref List<ChrominoInGame> goodChrominos, ref List<ChrominoInGame> badChrominos, ref List<ChrominoInGame> neitherGoodNorBadChrominos, ref List<ChrominoInGame> openToOpponentChrominos)
        {
            List<Square> squaresAfterTry = new ChrominoInGameBI(Ctx, GameId, potentialChromino).GetSquares();
            squaresAfterTry.AddRange(squares);
            List<Position> positionsAfterTry = GoodPositionBI.PotentialPositions(squaresAfterTry).ToList();
            int opponentChrominoId = ChrominoInHandLastDal.IdOf(GameId, opponentId);
            int opponentNumberChromino = ChrominoInHandDal.ChrominosNumber(GameId, opponentId);
            List<Position> positionWhereOpponentCanPlayAfterTry = GoodPositionBI.PositionsOkForChromino(opponentChrominoId, positionsAfterTry);
            int botNumberChrominos = ChrominoInHandDal.ChrominosNumber(GameId, PlayerId);
            int opponentNumberPositionsOk = GoodPositionBI.PositionsOkForChromino(opponentChrominoId, opponentPotentialPositions).Count;
            if (opponentNumberChromino == 1 && opponentNumberPositionsOk > 0)
            {
                // l'adversaire peut finir après le tour du bot s'il ne joue pas
                if (potentialChromino != null && botNumberChrominos == 1) // le bot peut finir ce tour => il peut jouer
                    goodChrominos.Add(potentialChromino);
                else if (false) // todo le bot joue après l'adversaire et il peut finir le coup d'après => il joue
                    #region todo
#pragma warning disable CS0162 // Code inaccessible détecté
                    goodChrominos.Add(potentialChromino);
#pragma warning restore CS0162 // Code inaccessible détecté
                #endregion
                else if (positionWhereOpponentCanPlayAfterTry.Count > 0) // le bot joue pour le bloquer
                    goodChrominos.Add(potentialChromino);
                else
                    neitherGoodNorBadChrominos.Add(potentialChromino);
            }
            else if (opponentNumberChromino == 1 && opponentNumberPositionsOk == 0)
            {
                // l'adversaire ne peut pas jouer en l'état
                if (potentialChromino != null && botNumberChrominos == 1) // le bot peut finir ce tour => il joue
                    goodChrominos.Add(potentialChromino);
                else if (false) // todo le bot peut finir le coup d'après : il joue
                    #region todo
#pragma warning disable CS0162 // Code inaccessible détecté
                    goodChrominos.Add(potentialChromino);
#pragma warning restore CS0162
                #endregion
                else if (positionWhereOpponentCanPlayAfterTry.Count == 0) // l'adversaire ne finira pas après le coup du bot : le bot joue
                    goodChrominos.Add(potentialChromino);
                else if (positionWhereOpponentCanPlayAfterTry.Count > 0) // l'adversaire peut finir si le bot joue ce soup : il ne doit pas le jouer !
                    badChrominos.Add(potentialChromino);
                else
                    neitherGoodNorBadChrominos.Add(potentialChromino);
            }
            else if (opponentNumberChromino > 1 && opponentNumberPositionsOk > 0)
            {
                // l'adversaire peut jouer après le tour du bot s'il ne joue pas mais ne finit pas
                if (potentialChromino != null && botNumberChrominos == 1) // le bot peut finir ce tour => il peut jouer
                    goodChrominos.Add(potentialChromino);
                else if (false) // todo le bot joue après l'adversaire et il peut finir le coup d'après => il joue
                    #region todo
#pragma warning disable CS0162 // Code inaccessible détecté
                    goodChrominos.Add(potentialChromino);
#pragma warning restore CS0162 // Code inaccessible détecté
                #endregion
                else if (positionWhereOpponentCanPlayAfterTry.Count > 0) // le bot joue pour le bloquer
                    goodChrominos.Add(potentialChromino);
                else
                    neitherGoodNorBadChrominos.Add(potentialChromino);
            }
            else
            {
                // l'adversaire ne peut pas jouer après le tour du bot s'il ne joue pas et ne finira pas
                if (potentialChromino != null && botNumberChrominos == 1) // le bot peut finir ce tour => il joue
                    goodChrominos.Add(potentialChromino);
                else if (false) // todo le bot peut finir le coup d'après : il joue
                    #region todo
#pragma warning disable CS0162 // Code inaccessible détecté
                    goodChrominos.Add(potentialChromino);
#pragma warning restore CS0162
                #endregion
                else if (positionWhereOpponentCanPlayAfterTry.Count == 0) // l'adversaire ne pouera pas jouer après le coup du bot : le bot joue
                    goodChrominos.Add(potentialChromino);
                else if (positionWhereOpponentCanPlayAfterTry.Count > 0) // l'adversaire peut jouer si le bot joue ce soup : à calculer...
                    openToOpponentChrominos.Add(potentialChromino);
                else
                    neitherGoodNorBadChrominos.Add(potentialChromino);
            }
        }

        private bool IsRisky(List<ChrominoInGame> openToOpponentChrominos, out int index)
        {
            foreach (ChrominoInGame chromino in openToOpponentChrominos)
            {
                // todo : écrire algo
            }
            index = 0;
            return false;
        }
    }
}
