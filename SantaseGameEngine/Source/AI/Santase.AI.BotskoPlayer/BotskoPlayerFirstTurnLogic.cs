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

        private Card PlayWhenRulesDoNotApply(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay, AnnounceInfo playerAnnounce)
        {
            if (context.State.CanAnnounce20Or40 && playerAnnounce != AnnounceInfo.DoNotHaveAnnounce)
            {
            }

            Card cardToPlay = this.FindSmallestNotTrumpCard(possibleCardsToPlay, context.TrumpCard.Suit);
            return cardToPlay;
        }

        private Card PlayWhenIsClosed(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
        {
            Card cardToPlay = null;
            var trumpSuit = context.TrumpCard.Suit;
            var trumpsCount = possibleCardsToPlay.Where(c => c.Suit == trumpSuit).Count();

            var biggestTrumpInSecondPlayer = this.FindBiggestTrumpCard(possibleCardsToPlay, trumpSuit);
            //var biggestTrumpInFirstPlayer = this.FindBiggestTrumpCard(opponentCards, trumpSuit);

            //if(biggestTrumpInSecondPlayer.Type > biggestTrumpInSecondPlayer.Type && trumpsCount > 1)
            //{
            //    cardToPlay = biggestTrumpInFirstPlayer;
            //}

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
        private bool CanWinWithTrumpCard(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
        {
            var biggestTrumpCardInHand = this.FindBiggestTrumpCard(possibleCardsToPlay, context.TrumpCard.Suit);
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

        private bool HasWinningNotTrumpAceOrTenInHand(ICollection<Card> possibleCardsToPlay, CardSuit trumpSuit)
        {
            var possibleWinners = possibleCardsToPlay
                .Where(c => (c.Type == CardType.Ace ||
                             c.Type == CardType.Ten) &&
                            c.Suit != trumpSuit)
                            .ToList();


            return false;
        }

        private Card FindBiggestTrumpCard(ICollection<Card> possibleCardsToPlay, CardSuit trumpSuit)
        {
            var biggestTrump = possibleCardsToPlay
                .Where(c => c.Suit == trumpSuit)
                .OrderByDescending(c => c.GetValue())
                .FirstOrDefault();

            return biggestTrump;
        }

        private Card FindSmallestNotTrumpCard(ICollection<Card> possibleCardsToPlay, CardSuit trumpSuit)
        {
            var smallestNotTrumpCard = possibleCardsToPlay
                .Where(c => c.Suit != trumpSuit)
                .OrderBy(c => c.GetValue())
                .FirstOrDefault();

            return smallestNotTrumpCard;
        }

        private bool IsBiggestTrumpIsInMyHand(Card biggestTrump)
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

                    if (biggestTrumpValue < cardValue)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}