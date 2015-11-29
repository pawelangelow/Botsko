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
        private static bool[,] usedCards;

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
        public virtual Card Execute(PlayerTurnContext context)
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

            var trumps = this.GetTrumpsInHand(this.cards, context.TrumpCard.Suit);

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

        private TrumpSummary GetTrumpsInHand(ICollection<Card> hand, CardSuit trumpSuit)
        {
            int count = 0;
            int points = 0;

            foreach (var card in hand)
            {
                if (card.Suit == trumpSuit)
                {
                    count++;
                    points += (int)card.Type;
                }
            }

            var output = new TrumpSummary
            {
                Count = count,
                PointsOfAll = points
            };

            return output;
        }
    }
}