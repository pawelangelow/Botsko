namespace Santase.AI.BotskoPlayer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Logic.Cards;
    using Logic.PlayerActionValidate;
    using Logic.Players;

    public class BotskoPlayerFirstTurnLogic : BotskoPlayerCommonLogic
    {
        private Card currentWinningCard;

        public BotskoPlayerFirstTurnLogic(IPlayerActionValidator playerActionValidator, ICollection<Card> cards)
            : base(playerActionValidator, cards)
        {
        }

        public override Card Execute(PlayerTurnContext context, BasePlayer basePlayer, Card playerAnnounce)
        {
            var possibleCardsToPlay = this.playerActionValidator.GetPossibleCardsToPlay(context, this.cards);
            if (this.CanWinWithTrumpCard(context, possibleCardsToPlay))
            {
                return this.currentWinningCard;
            }

            Card cardToPlay = null;

            return base.Execute(context, basePlayer, playerAnnounce);
        }

        private Card PlayWhenRulesDoNotApply(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay, Card playerAnnounce)
        {
            // Check if can call 20 or 40 -> and do it
            if (context.State.CanAnnounce20Or40 && playerAnnounce != null)
            {
                return playerAnnounce;
            }

            Card cardToPlay = this.FindSmallestNotTrumpCard(possibleCardsToPlay, context.TrumpCard.Suit);
            return cardToPlay;
        }

        // Help methods

        /// <summary>
        /// Check for 100% winning card in the hand
        /// </summary>
        /// <param name="context">The information about current turn.</param>
        /// <param name="possibleCardsToPlay">Cards that player can play.</param>
        /// <returns>If the player have winning card return true,
        ///          if not return false.</returns>
        public bool CanWinWithTrumpCard(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
        {
            var biggestTrumpCardInHand = this.FindTrumpCardsInHand(possibleCardsToPlay, context.TrumpCard.Suit).FirstOrDefault();
            var biggestTrumpCardInHandValue = biggestTrumpCardInHand.GetValue();
            var pointsWithBiggestTrumpCard
                = biggestTrumpCardInHandValue + context.SecondPlayerRoundPoints;

            if (this.IsBiggestTrumpIsInMyHand(biggestTrumpCardInHand) &&
                pointsWithBiggestTrumpCard >= 66)
            {
                this.currentWinningCard = biggestTrumpCardInHand;
                return true;
            }

            return false;
        }

        public Card HasWinningNotTrumpAce(ICollection<Card> possibleCardsToPlay, CardSuit trumpSuit)
        {
            var possibleWinners = possibleCardsToPlay
                .Where(c => c.Type == CardType.Ace && c.Suit != trumpSuit)
                .ToList();

            foreach (var card in possibleWinners)
            {
                if (this.HowMuchTrumpsAreInPlay(trumpSuit) == this.FindTrumpCardsInHand(possibleCardsToPlay, trumpSuit).Count())
                {
                    return card;
                }

                if (!this.IsCardLastOne((int)card.Suit))
                {
                    return card;
                }
            }

            return null;
        }

        public Card HasWinningNotTrumpTen(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay, CardSuit trumpSuit)
        {
            var possibleWinners = possibleCardsToPlay
                .Where(c => c.Type == CardType.Ten && c.Suit != trumpSuit)
                .ToList();

            foreach (var card in possibleWinners)
            {
                // Check if Ace is used and this 10 is not the last one from this suit
                if (usedCards[(int)card.Suit, 5] &&
                    !this.IsCardLastOne((int)card.Suit))
                {
                    return card;
                }

                // Check if Ace is used and there no more trumps in the game
                if (usedCards[(int)card.Suit, 5] &&
                    this.HowMuchTrumpsAreInPlay(trumpSuit) == this.FindTrumpCardsInHand(possibleCardsToPlay, trumpSuit).Count())
                {
                    return card;
                }

                // Add logic when is Closed and there are cards in the deck
                // this is a risky logic
                // TODO: Talk about this with Ivan and Pavel !!!
                //if (context.CardsLeftInDeck != 0)
                //{
                //    return card;
                //}
            }

            return null;
        }

        public bool IsCardLastOne(int suit)
        {
            int count = 0;
            for (int type = 5; type >= 0; type--)
            {
                if (usedCards[suit, type])
                {
                    count++;
                }
            }

            if (count == 5)
            {
                return true;
            }

            return false;
        }

        public List<Card> FindTrumpCardsInHand(ICollection<Card> possibleCardsToPlay, CardSuit trumpSuit)
        {
            var biggestTrump = possibleCardsToPlay
                .Where(c => c.Suit == trumpSuit)
                .OrderByDescending(c => c.GetValue())
                .ToList();

            return biggestTrump;
        }

        public bool IsBiggestTrumpIsInMyHand(Card biggestTrump)
        {
            int suit = (int)biggestTrump.Suit;
            int biggestTrumpValue = biggestTrump.GetValue();

            if (biggestTrumpValue == 11)
            {
                return true;
            }

            for (int type = 5; type >= 0; type--)
            {
                if (usedCards[suit, type] == false)
                {
                    int cardValue = this.GetCardValue(type);
                    if (biggestTrumpValue < cardValue)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private int GetCardValue(int type)
        {
            int cardValue = 0;
            switch (type)
            {
                case 0: cardValue = 0; break;
                case 1: cardValue = 2; break;
                case 2: cardValue = 3; break;
                case 3: cardValue = 4; break;
                case 4: cardValue = 10; break;
                case 5: cardValue = 11; break;
                default: throw new ArgumentException("Unsupported card to play!");
            }

            return cardValue;
        }

        private Card FindSmallestNotTrumpCard(ICollection<Card> possibleCardsToPlay, CardSuit trumpSuit)
        {
            var smallestNotTrumpCard = possibleCardsToPlay
                .Where(c => c.Suit != trumpSuit)
                .OrderBy(c => c.GetValue())
                .FirstOrDefault();

            return smallestNotTrumpCard;
        }
    }
}