namespace Santase.AI.BotskoPlayer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Contracts;
    using Logic.Cards;
    using Logic.Extensions;
    using Logic.PlayerActionValidate;
    using Logic.Players;

    public class BotskoPlayerCommonLogic : IBotskoPlayerCommonLogic
    {
        protected IPlayerActionValidator playerActionValidator;
        protected ICollection<Card> cards;
        protected static bool[,] usedCards;

        public BotskoPlayerCommonLogic(IPlayerActionValidator playerActionValidator, ICollection<Card> cards)
        {
            this.playerActionValidator = playerActionValidator;
            this.cards = cards;
            usedCards = new bool[4, 6];
        }

        public bool[,] PlayedCards
        {
            get
            {
                return usedCards;
            }
        }

        // TODO: Remove the dummy logic and make the method abstract, so that the other two cases may override it.
        public virtual Card Execute(PlayerTurnContext context, BasePlayer basePlayer, Card playerAnnounce)
        {
            var possibleCardsToPlay = this.playerActionValidator.GetPossibleCardsToPlay(context, this.cards);
            var shuffledCards = possibleCardsToPlay.Shuffle();
            var cardToPlay = shuffledCards.First();

            return cardToPlay;
        }

        public bool IsGoodToClose(PlayerTurnContext context)
        {
            if (!context.State.CanClose)
            {
                return false;
            }

            var handSummary = this.GetTrumpsInHand(this.cards, context.TrumpCard.Suit);

            if (handSummary.CountOfTrumps > 4)
            {
                return true; //// With more than 4 trumps in hand => go get the win
            }

            if (handSummary.CountOfTrumps == 4 && handSummary.CountOfAcesNoTrumps > 1)
            {
                var condition = this.cards
                    .Where(c => c.Suit == context.TrumpCard.Suit &&
                            c.Type == CardType.Queen || c.Type == CardType.King)
                    .Count();

                if (condition > 1)
                {
                    return true; ////4 trumps, maybe 40 as announce and at least one ace has good chance to win the game
                }
            }

            if (handSummary.CountOfTrumps >= 3 &&
                handSummary.CountOfAcesNoTrumps > 0 &&
                (66 - (handSummary.PointsOfAll + handSummary.PointsOfTrumps + context.FirstPlayerRoundPoints)) <= 15)
            {
                if (this.IsThereBigCardsInPlay(context.TrumpCard.Suit, 3)) //// Secure the choice even more if big values are gone, it is risky
                {
                    return true; //// At least 3 trumps, 1 ace and points close to win
                }
            }

            // if(handSummary.CountOfTrumps >= 2 &&)

            return false;
        }

        public virtual void RegisterUsedCard(Card theCard)
        {
            int firstCoordinate = (int)theCard.Suit;
            int secondCoordinate = 0;

            switch (theCard.Type)
            {
                case CardType.Nine:
                    secondCoordinate = 0;
                    break;

                case CardType.Jack:
                    secondCoordinate = 1;
                    break;

                case CardType.Queen:
                    secondCoordinate = 2;
                    break;

                case CardType.King:
                    secondCoordinate = 3;
                    break;

                case CardType.Ten:
                    secondCoordinate = 4;
                    break;

                case CardType.Ace:
                    secondCoordinate = 5;
                    break;

                default:
                    throw new ArgumentException("Unsupported card to play!");
            }

            usedCards[firstCoordinate, secondCoordinate] = true;
        }

        //Help methods

        private HandSummary GetTrumpsInHand(ICollection<Card> hand, CardSuit trumpSuit)
        {
            //TODO: If it is slow -> break HQC and return array with 3 ingegers
            int countOfTrumps = 0;
            int trumpCardPoints = 0;
            int otherCardPoints = 0;
            int countOfAcesNoTrumps = 0;

            foreach (var card in hand)
            {
                if (card.Suit == trumpSuit)
                {
                    countOfTrumps++;
                    trumpCardPoints += card.GetValue();
                }
                else
                {
                    if (card.GetValue() == 11)
                    {
                        countOfAcesNoTrumps++;
                    }
                    otherCardPoints += card.GetValue();
                }
            }

            var output = new HandSummary
            {
                CountOfTrumps = countOfTrumps,
                PointsOfAll = otherCardPoints,
                PointsOfTrumps = trumpCardPoints,
                CountOfAcesNoTrumps = countOfAcesNoTrumps
            };

            return output;
        }

        private bool IsThereBigCardsInPlay(CardSuit trumpSuit, int howMuch)
        {
            int count = 0;

            for (int i = 0; i < 4; i++) //// Anti HQC
            {
                for (int j = 4; j < 6; j++) //// Anti HQC
                {
                    if (i != (int)trumpSuit &&
                        usedCards[i, j] == false)
                    {
                        count++;
                    }
                }
            }

            if (count >= howMuch)
            {
                return true;
            }

            return false;
        }

        protected int HowMuchTrumpsAreInPlay(CardSuit trumpSuit)
        {
            int count = 0;

            for (int j = 0; j < 6; j++)
            {
                if (usedCards[(int)trumpSuit, j] == false)
                {
                    count++;
                }
            }

            return count;
        }
    }
}