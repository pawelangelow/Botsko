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

        // This method will execute only if the other logic do not find card
        // it return random card
        public override Card Execute(PlayerTurnContext context, BasePlayer basePlayer, Card playerAnnounce)
        {
            return base.Execute(context, basePlayer, playerAnnounce);
        }

        public Card PlayWhenRulesDoNotApply(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay, Card playerAnnounce)
        {
            // 1. Check if can win the round with the biggest trump
            if (this.CanWinWithTrumpCard(context, possibleCardsToPlay))
            {
                return this.currentWinningCard;
            }

            // 2. Check if can call 20 or 40 -> and do it
            if (context.State.CanAnnounce20Or40 && playerAnnounce != null)
            {
                return playerAnnounce;
            }

            // 3. Find smallest not trump card and play it
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
            var biggestTrumpCardInHand = this.FindTrumpCardsInHand(possibleCardsToPlay, context.TrumpCard.Suit)
                                            .FirstOrDefault();
            if (biggestTrumpCardInHand == null)
            {
                return false;
            }

            var pointsWithBiggestTrumpCard
                = biggestTrumpCardInHand.GetValue() + context.SecondPlayerRoundPoints;

            if (this.IsBiggestTrumpInMyHand(biggestTrumpCardInHand) &&
                pointsWithBiggestTrumpCard >= 66)
            {
                this.currentWinningCard = biggestTrumpCardInHand;
                return true;
            }

            return false;
        }

        public int CountCardInGivenSuit(ICollection<Card> possibleCardsToPlay, CardSuit suit)
        {
            return possibleCardsToPlay.Count(c => c.Suit == suit);
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

                // TODO: Check if all cards from this suit are in my hand
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
                if (this.PlayedCards[(int)card.Suit, 5] &&
                    !this.IsCardLastOne((int)card.Suit))
                {
                    return card;
                }

                // Check if Ace is used and there no more trumps in the game
                if (this.PlayedCards[(int)card.Suit, 5] &&
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

        /// <summary>
        /// Check if the card is the last one from this suit.
        /// </summary>
        /// <param name="suit">Card suit.</param>
        /// <returns>Return true if the card is the last one
        ///          else returns false.</returns>
        public bool IsCardLastOne(int suit)
        {
            int count = 0;
            for (int type = 5; type >= 0; type--)
            {
                if (this.PlayedCards[suit, type])
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

        /// <summary>
        /// Find all trump cards in hand and order them by power.
        /// </summary>
        /// <param name="possibleCardsToPlay">Cards that player can play.</param>
        /// <param name="trumpSuit">Trump card suit.</param>
        /// <returns>List with trump cards orderes by their power.</returns>
        public List<Card> FindTrumpCardsInHand(ICollection<Card> possibleCardsToPlay, CardSuit trumpSuit)
        {
            var trumpsInHand = possibleCardsToPlay
                .Where(c => c.Suit == trumpSuit)
                .OrderByDescending(c => c.GetValue())
                .ToList();

            return trumpsInHand;
        }

        /// <summary>
        /// Search in the cards for the smallest card to play. 
        /// If the smallest card is Queen or King first check for possible 20.
        /// </summary>
        /// <param name="possibleCardsToPlay">Cards that player can play.</param>
        /// <param name="trumpSuit">Trump card suit.</param>
        /// <returns>Return the smallest founded card.</returns>
        public Card FindSmallestNotTrumpCard(ICollection<Card> possibleCardsToPlay, CardSuit trumpSuit)
        {
            var smallestNotTrumpCards = possibleCardsToPlay
                .Where(c => c.Suit != trumpSuit)
                .OrderBy(c => c.GetValue())
                .ToList();

            foreach (var card in smallestNotTrumpCards)
            {
                if (card.Type != CardType.Queen && card.Type != CardType.King)
                {
                    return card;
                }

                if (!this.CheckForPossible20(card))
                {
                    return card;
                }
            }

            return smallestNotTrumpCards.FirstOrDefault();
        }

        /// <summary>
        /// Check if this card is not a part from possible 20.
        /// </summary>
        /// <param name="card">Queen or King to be checked.</param>
        /// <returns>If the other card from 20 is already played returns false
        ///          else the card is still in the game.</returns>
        public bool CheckForPossible20(Card card)
        {
            if (card.Type == CardType.Queen)
            {
                if (this.PlayedCards[(int)card.Suit, 3] == false)
                {
                    return true;
                }

                return false;
            }
            else
            {
                if (this.PlayedCards[(int)card.Suit, 2] == false)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Check if the biggest trump in the hand is the biggest not played.
        /// </summary>
        /// <param name="biggestTrump">The biggest trump card in the hand.</param>
        /// <returns>Return true if biggestTrump is the biggest one left in the game.
        ///          Return false if there is bigger one.</returns>
        public bool IsBiggestTrumpInMyHand(Card biggestTrump)
        {
            int suit = (int)biggestTrump.Suit;
            int biggestTrumpValue = biggestTrump.GetValue();

            if (biggestTrumpValue == 11)
            {
                return true;
            }

            for (int type = 5; type >= 0; type--)
            {
                if (this.PlayedCards[suit, type] == false)
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

        /// <summary>
        /// Convert column value from PlayedCards array to card value.
        /// </summary>
        /// <param name="type">Represents the column in PlayedCards array.</param>
        /// <returns>The card value.</returns>
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
    }
}