using Data;
using Data.Core;
using Data.DAL;
using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;

namespace Core
{
	public class ComputedChrominoCore
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
		private readonly ComputedChrominosDal ComputedChrominosDal;


		public ComputedChrominoCore(Context ctx, int gameId)
		{
			GameId = gameId;
			ChrominoInHandDal = new ChrominoInHandDal(ctx);
			ChrominoDal = new ChrominoDal(ctx);
			SquareDal = new SquareDal(ctx);
			GamePlayerDal = new GamePlayerDal(ctx);
			ComputedChrominosDal = new ComputedChrominosDal(ctx);
		}


		public void RemoveCandidate(int botId, int chrominoId)
		{
			ComputedChrominosDal.Remove(GameId, botId, chrominoId);
		}

		public void RemoveAndUpdateCandidatesFromLastChrominoPlayed()
		{
			List<Square> squares = SquareDal.List(GameId);
			List<Square> playedSquares = new List<Square> { squares[0], squares[1], squares[2] };
			HashSet<Position> possiblesPositions = ComputePossiblesPositions(squares, playedSquares);

			HashSet<ComputedChromino> computedChrominosToRemove = new HashSet<ComputedChromino>();
			foreach (var square in playedSquares)
			{
				foreach (ComputedChromino computedChromino in ToDelete(square))
				{
					computedChrominosToRemove.Add(computedChromino);
				}
			}

			foreach (var botId in GamePlayerDal.BotsId(GameId))
			{
				ComputedChrominosDal.Remove(GameId, botId, computedChrominosToRemove);

				List<ChrominoInHand> hand = ChrominoInHandDal.ChrominosByPriority(GameId, botId);
				List<ComputedChromino> chrominosFound = new List<ComputedChromino>();
				foreach (ChrominoInHand chrominoInHand in hand)
				{
					HashSet<Position> goodPositions = ComputeChrominoToPlay(chrominoInHand, possiblesPositions);
					foreach (Position position in goodPositions)
					{
						ComputedChromino computedChromino = new ComputedChromino
						{
							BotId = botId,
							GameId = GameId,
							ChrominoId = chrominoInHand.ChrominoId,
							Orientation = position.Orientation,
							X = position.Coordinate.X,
							Y = position.Coordinate.Y,
						};
						chrominosFound.Add(computedChromino);
					}
				}
				ComputedChrominosDal.Add(chrominosFound);
			}
		}

		public void ResetAndUpdateCandidatesFromAllChrominosPlayed(int botId)
		{
			ComputedChrominosDal.Remove(GameId, botId);
			foreach (ChrominoInHand chrominoInHand in ChrominoInHandDal.ChrominosByPriority(GameId, botId))
				UpdateCandidatesFromAllChrominosPlayed(botId, chrominoInHand.ChrominoId);
		}


		public void UpdateCandidatesFromAllChrominosPlayed(int botId, int chrominoId)
		{
			List<Square> squares = SquareDal.List(GameId);

			HashSet<Position> positions = ComputePossiblesPositions(squares);
			ChrominoInHand chrominoInHand = ChrominoInHandDal.Details(GameId, chrominoId);
			List<ComputedChromino> chrominosFound = new List<ComputedChromino>();
			HashSet<Position> goodPositions = ComputeChrominoToPlay(chrominoInHand, positions);
			foreach (Position position in goodPositions)
			{
				ComputedChromino computedChromino = new ComputedChromino
				{
					BotId = botId,
					GameId = GameId,
					ChrominoId = chrominoInHand.ChrominoId,
					Orientation = position.Orientation,
					X = position.Coordinate.X,
					Y = position.Coordinate.Y,
				};
				chrominosFound.Add(computedChromino);
			}
			ComputedChrominosDal.Add(chrominosFound);
		}



		/// <summary>
		/// retourne les positions possibles où peuvent être joués des chrominos
		/// </summary>
		/// <param name="occupiedSquares">liste complète des squares occupés</param>
		/// <param name="squaresForArea">liste des squares définissant la zone à rechercher</param>
		/// <returns></returns>
		public HashSet<Position> ComputePossiblesPositions(List<Square> occupiedSquares, List<Square> squaresForArea = null)
		{
			if (squaresForArea == null)
				squaresForArea = occupiedSquares;
			HashSet<Coordinate> coordinates = FreeAroundSquares(squaresForArea);
			HashSet<Position> positions = new HashSet<Position>();
			// prendre ceux avec un seul coté en commun avec un chromino
			// calculer orientation avec les couleurs imposées ou pas
			foreach (Coordinate firstCoordinate in coordinates)
			{
				ColorCh? firstColor = firstCoordinate.OkColorFor(occupiedSquares, out int commonSidesFirstColor);
				if (firstColor != null)
				{
					//cherche placement possible d'un square
					foreach (Orientation orientation in (Orientation[])Enum.GetValues(typeof(Orientation)))
					{
						Coordinate offset = new Coordinate(orientation);
						Coordinate secondCoordinate = firstCoordinate + offset;
						Coordinate thirdCoordinate = secondCoordinate + offset;

						if (secondCoordinate.IsFree(ref occupiedSquares) && thirdCoordinate.IsFree(ref occupiedSquares))
						{
							//calcul si chromino posable et dans quelle position
							ColorCh? secondColor = secondCoordinate.OkColorFor(occupiedSquares, out int adjacentChrominoSecondColor);
							ColorCh? thirdColor = thirdCoordinate.OkColorFor(occupiedSquares, out int adjacentChrominosThirdColor);

							if (secondColor != null && thirdColor != null && commonSidesFirstColor + adjacentChrominoSecondColor + adjacentChrominosThirdColor >= 2)
							{
								Position possibleSpace = new Position
								{
									Coordinate = firstCoordinate,
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
		/// <param name="previouslyDraw">indique si la joueur vient de piocher avant de tester</param>
		/// <param name="positions">liste des positions où chercher à placer le chromino</param>
		/// <param name="indexInHand">index du chromino convenant</param>
		/// <param name="position">position si convenant, null sinon</param>
		/// <returns>null si aucun chromino ne couvient</returns>
		public ChrominoInGame ComputeChrominoToPlay(List<ChrominoInHand> hand, bool previouslyDraw, HashSet<Position> positions, out int indexInHand, out Position position)
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
						if ((chromino.FirstColor == currentPosition.FirstColor || currentPosition.FirstColor == ColorCh.Cameleon) && (chromino.SecondColor == currentPosition.SecondColor || chromino.SecondColor == ColorCh.Cameleon || currentPosition.SecondColor == ColorCh.Cameleon) && (chromino.ThirdColor == currentPosition.ThirdColor || currentPosition.ThirdColor == ColorCh.Cameleon))
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
						else if ((chromino.FirstColor == currentPosition.ThirdColor || currentPosition.ThirdColor == ColorCh.Cameleon) && (chromino.SecondColor == currentPosition.SecondColor || chromino.SecondColor == ColorCh.Cameleon || currentPosition.SecondColor == ColorCh.Cameleon) && (chromino.ThirdColor == currentPosition.FirstColor || currentPosition.FirstColor == ColorCh.Cameleon))
						{
							goodChrominoInHand = chrominoInHand;
							Coordinate offset = new Coordinate(currentPosition.Orientation);
							firstCoordinate = currentPosition.Coordinate + 2 * offset;

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

		public HashSet<Position> ComputeChrominoToPlay(ChrominoInHand chrominoInHand, HashSet<Position> positions)
		{
			Chromino chromino = ChrominoDal.Details(chrominoInHand.ChrominoId);
			HashSet<Position> goodPositions = new HashSet<Position>();
			foreach (Position currentPosition in positions)
			{
				if ((chromino.FirstColor == currentPosition.FirstColor || currentPosition.FirstColor == ColorCh.Cameleon) && (chromino.SecondColor == currentPosition.SecondColor || chromino.SecondColor == ColorCh.Cameleon || currentPosition.SecondColor == ColorCh.Cameleon) && (chromino.ThirdColor == currentPosition.ThirdColor || currentPosition.ThirdColor == ColorCh.Cameleon))
					goodPositions.Add(currentPosition);

				if (chromino.FirstColor != chromino.ThirdColor && (chromino.FirstColor == currentPosition.ThirdColor || currentPosition.ThirdColor == ColorCh.Cameleon) && (chromino.SecondColor == currentPosition.SecondColor || chromino.SecondColor == ColorCh.Cameleon || currentPosition.SecondColor == ColorCh.Cameleon) && (chromino.ThirdColor == currentPosition.FirstColor || currentPosition.FirstColor == ColorCh.Cameleon))
				{
					Orientation orientation;
					Coordinate newCoordinate = currentPosition.Coordinate;

					switch (currentPosition.Orientation)
					{
						case Orientation.Horizontal:
							orientation = Orientation.HorizontalFlip;
							newCoordinate += 2 * Coordinate.StepX;
							break;
						case Orientation.HorizontalFlip:
							orientation = Orientation.Horizontal;
							newCoordinate -= 2 * Coordinate.StepX;
							break;
						case Orientation.Vertical:
							orientation = Orientation.VerticalFlip;
							newCoordinate -= 2 * Coordinate.StepY;
							break;
						case Orientation.VerticalFlip:
						default:
							orientation = Orientation.Vertical;
							newCoordinate += 2 * Coordinate.StepY;
							break;
					}
					Position newPosition = new Position { Orientation = orientation, Coordinate = newCoordinate };
					goodPositions.Add(newPosition);
				}
			}
			return goodPositions;
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
				if (rightCoordinate.IsFree(ref squares))
					coordinates.Add(rightCoordinate);
				if (bottomCoordinate.IsFree(ref squares))
					coordinates.Add(bottomCoordinate);
				if (leftCoordinate.IsFree(ref squares))
					coordinates.Add(leftCoordinate);
				if (topCoordinate.IsFree(ref squares))
					coordinates.Add(topCoordinate);
			}
			return coordinates;
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

		private List<ComputedChromino> ToDelete(Square square)
		{
			int x = square.X;
			int y = square.Y;

			List<ComputedChromino> ComputedChrominos = new List<ComputedChromino>();
			foreach (Orientation orientation in (Orientation[])Enum.GetValues(typeof(Orientation)))
			{
				int coef;
				if (orientation == Orientation.Horizontal || orientation == Orientation.Vertical)
					coef = 1;
				else
					coef = -1;

				switch (orientation)
				{
					case Orientation.Horizontal:
					case Orientation.HorizontalFlip:
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - coef * 3, Y = y });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - coef * 2, Y = y - 1 });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - coef * 2, Y = y });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - coef * 2, Y = y + 1 });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - coef, Y = y - 1 });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - coef, Y = y });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - coef, Y = y + 1 });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x, Y = y - 1 });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x, Y = y });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x, Y = y + 1 });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x + coef, Y = y });
						break;

					case Orientation.Vertical:
					case Orientation.VerticalFlip:
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x, Y = y + coef * 3 });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - 1, Y = y + coef * 2 });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x, Y = y + coef * 2 });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x + 1, Y = y + coef * 2 });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - 1, Y = y + coef });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x, Y = y + coef });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x + 1, Y = y + coef });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x - 1, Y = y });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x, Y = y });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x + 1, Y = y });
						ComputedChrominos.Add(new ComputedChromino { Orientation = orientation, X = x, Y = y - coef });
						break;
				}
			}
			return ComputedChrominos;
		}
	}
}

