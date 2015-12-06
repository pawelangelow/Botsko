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

        public BotskoPlayerFirstTurnLogic(IPlayerActionValidator playerActionValidator, ICollection<Card> cards, bool[,] playedCards)
            : base(playerActionValidator, cards, playedCards)
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
            // 1. Check if can call 20 or 40 -> and do it
            if (context.State.CanAnnounce20Or40 && playerAnnounce != null)
            {
                return playerAnnounce;
            }

            // 2. Check if can win the round with the biggest trump
            if (this.CanWinWithTrumpCard(context, possibleCardsToPlay))
            {
                return this.currentWinningCard;
            }

            // 3. Check if hava sequence of winning trumps
            if (this.HasSequenceOfWinningTrumps(context, possibleCardsToPlay))
            {
                return this.currentWinningCard;
            }

            // 3. Find smallest not trump card and play it
            Card cardToPlay = this.FindSmallestNotTrumpCard(possibleCardsToPlay, context.TrumpCard.Suit);

            // TODO: Talk with Pavel and Ivan to add Ten ??
            if (cardToPlay.Type == CardType.Ace)
            {
                var possibleTrump = this.FindBetterTrumpCard(possibleCardsToPlay, context.TrumpCard.Suit);
                if (possibleTrump != null)
                {
                    return possibleTrump;
                }
            }

            return cardToPlay;
        }

        /// <summary>
        /// If the smallest not trump card in the hand is Ace
        /// find the best trump card that can be played.
        /// </summary>
        /// <param name="possibleCardsToPlay">Cards that player can play.</param>
        /// <param name="trumpSuit">Trump card suit</param>
        /// <returns>If the biggest trump card is winneng returns it,
        ///          else return the smallest one.</returns>
        public Card FindBetterTrumpCard(ICollection<Card> possibleCardsToPlay, CardSuit trumpSuit)
        {
            var biggestTrumpCards = this.FindTrumpCardsInHand(possibleCardsToPlay, trumpSuit);
            var biggestTrump = biggestTrumpCards.FirstOrDefault();

            // TODO: Check of if necessary to make check if biggestTrump is != null
            if (biggestTrump != null)
            {
                if (this.IsBiggestCardInMyHand(biggestTrump))
                {
                    return biggestTrump;
                }
                else
                {
                    return biggestTrumpCards.Last();
                }
            }

            return null;
        }

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

            if (this.IsBiggestCardInMyHand(biggestTrumpCardInHand) &&
                pointsWithBiggestTrumpCard >= 66)
            {
                this.currentWinningCard = biggestTrumpCardInHand;
                return true;
            }

            return false;
        }

        public bool HasSequenceOfWinningTrumps(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
        {
            var trumpCards = this.FindTrumpCardsInHand(possibleCardsToPlay, context.TrumpCard.Suit);

            if (trumpCards.Count == 0 || trumpCards.Count == 1)
            {
                return false;
            }

            var points = 0;
            foreach (var card in trumpCards)
            {
                if (card.Type == CardType.Ace || card.Type == CardType.Ten)
                {
                    points += card.GetValue();
                }
            }

            if (context.SecondPlayerRoundPoints + points >= 66)
            {
                this.currentWinningCard = trumpCards.FirstOrDefault();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Find card which is not from trump suit and it's will win this turn.
        /// </summary>
        /// <param name="possibleCardsToPlay">Cards that player can play.</param>
        /// <param name="trumpSuit">Trump card suit.</param>
        /// <returns>If there is some card from not trump suit and it will win this turn returns it
        ///          else return null.</returns>
        public Card HasWinningNotTrumpCard(ICollection<Card> possibleCardsToPlay, CardSuit trumpSuit)
        {
            var possibleWinners = possibleCardsToPlay
                .Where(c => c.Suit != trumpSuit)
                .OrderByDescending(c => c.GetValue())
                .ToList();

            var allTrumpCardsInPlay = this.HowMuchTrumpsAreInPlay(trumpSuit);
            var handTrumpCards = this.FindTrumpCardsInHand(possibleCardsToPlay, trumpSuit).Count();
            foreach (var card in possibleWinners)
            {
                var isBiggestCardInPlay = this.IsBiggestCardInMyHand(card);

                // TODO: Check if is King and have Queen -> 20

                // Check if bigger cards are used and this card is not the last one from this suit
                if (isBiggestCardInPlay &&
                    !this.IsCardLastOne((int)card.Suit))
                {
                    return card;
                }

                // Check if bigger cards are used and there no more trumps in the game
                if (isBiggestCardInPlay &&
                    allTrumpCardsInPlay == handTrumpCards)
                {
                    return card;
                }
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
        /// Check if the biggest card from given suit in the hand is the biggest not played.
        /// </summary>
        /// <param name="biggestCard">The biggest card from given suit in the hand.</param>
        /// <returns>Return true if biggestCard is the biggest one left in the game.
        ///          Return false if there is bigger one.</returns>
        public bool IsBiggestCardInMyHand(Card biggestCard)
        {
            int suit = (int)biggestCard.Suit;
            int biggestCardValue = biggestCard.GetValue();

            if (biggestCardValue == 11)
            {
                return true;
            }

            for (int type = 5; type >= 0; type--)
            {
                if (this.PlayedCards[suit, type] == false)
                {
                    int cardValue = this.GetCardValue(type);
                    if (biggestCardValue < cardValue)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Count how many cards from given suit are left in the game.
        /// </summary>
        /// <param name="possibleCardsToPlay">Cards that player can play.</param>
        /// <param name="suit">Suit to be checked.</param>
        /// <returns>Integer with cards count.</returns>
        public int CountCardInGivenSuit(ICollection<Card> possibleCardsToPlay, CardSuit suit)
        {
            return possibleCardsToPlay.Count(c => c.Suit == suit);
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