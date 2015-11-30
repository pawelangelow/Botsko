namespace Santase.AI.BotskoPlayer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Logic;
    using Logic.Cards;
    using Logic.PlayerActionValidate;
    using Logic.Players;

    public class BotskoPlayerSecondTurnLogic : BotskoPlayerCommonLogic
    {
        // Extract to constants class or use already defined constants.
        public const int PointsRequiredForWinningRound = 66;
        public const int NoValidCardInHand = -1;

        public BotskoPlayerSecondTurnLogic(IPlayerActionValidator playerActionValidator, ICollection<Card> cards)
            : base(playerActionValidator, cards)
        {
        }

        /// <summary>
        /// Core logic for taking response decisions.
        /// </summary>
        /// <param name="context">PlayerTurnContext holding the turn data.</param>
        /// <returns>Response card.</returns>
        public override Card Execute(PlayerTurnContext context, BasePlayer basePlayer, Card playerAnnounce)
        {
            return base.Execute(context, basePlayer, playerAnnounce);
        }

        /// <summary>
        /// Gets the highest card available in hand which has CardSuit different than the trump card.
        /// </summary>
        /// <param name="context">PlayerTurnContext holding the turn data.</param>
        /// <returns>The highest card found that matches the conditions.</returns>
        private Card GetHighestCardDifferentThanTrump(PlayerTurnContext context)
        {
            var trumpCardSuit = context.TrumpCard.Suit;
            var card = this.cards
                .Where(x => x.Suit != trumpCardSuit)
                .OrderByDescending(x => x.GetValue())
                .FirstOrDefault();

            return card;
        }

        /// <summary>
        /// Gets the highest card available in hand.
        /// </summary>
        /// <param name="context">PlayerTurnContext holding the turn data.</param>
        /// <returns>The highest card found that matches the conditions.</returns>
        private Card GetHighestCard(PlayerTurnContext context)
        {
            var card = this.cards.OrderByDescending(x => x.GetValue()).FirstOrDefault();

            return card;
        }

        /// <summary>
        /// Finds the highest card available in hand with CardSuit equal either to the opponent's card or the trump card.
        /// </summary>
        /// <param name="context">PlayerTurnContext holding the turn data.</param>
        /// <param name="trumpCardSuit">CardSuit of the trump card.</param>
        /// <returns>The highest card found that matches the conditions.</returns>
        private Card GetHighestCard(PlayerTurnContext context, CardSuit trumpCardSuit)
        {
            var card = this.cards
                .Where(x => x.Suit == trumpCardSuit)
                .OrderByDescending(x => x.GetValue())
                .FirstOrDefault();

            return card;
        }

        /// <summary>
        /// Finds the highest card available in hand with CardSuit equal either to the opponent's card or the trump card.
        /// </summary>
        /// <param name="context">PlayerTurnContext holding the turn data.</param>
        /// <param name="opponentCardSuit">CardSuit of the opponent's card.</param>
        /// <param name="trumpCardSuit">CardSuit of the trump card.</param>
        /// <returns>The highest card found that matches the conditions.</returns>
        private Card GetHighestCard(PlayerTurnContext context, CardSuit opponentCardSuit, CardSuit trumpCardSuit)
        {
            var card = this.cards
                .Where(x => x.Suit == opponentCardSuit || x.Suit == trumpCardSuit)
                .OrderByDescending(x => x.GetValue())
                .FirstOrDefault();

            return card;
        }

        /// <summary>
        /// Gets the optimal card which will win the round by taking the current hand.
        /// </summary>
        /// <param name="context">PlayerTurnContext holding the turn data.</param>
        /// <returns>The optimal card that wins the round or NULL if that is not possible.</returns>
        private Card GetCardWhenTakingHandWinsTheRound(PlayerTurnContext context)
        {
            var trumpCardSuit = context.TrumpCard.Suit;
            var opponentCard = context.SecondPlayedCard;
            var opponentCardSuit = context.SecondPlayedCard.Suit;

            Card highestCard = null;

            // Gets the highest response card possible.
            if (opponentCardSuit != trumpCardSuit)
            {
                highestCard = this.GetHighestCard(context, opponentCardSuit, trumpCardSuit);
            }
            else
            {
                highestCard = this.GetHighestCard(context, trumpCardSuit);
            }

            var highestCardValue = highestCard != null ? highestCard.GetValue() : NoValidCardInHand;
            var opponentCardValue = opponentCard.GetValue();

            // Returns the highest selected card which can win the round.
            // Otherwise returns null.
            if (this.TakingHandWinsRound(context, highestCardValue, opponentCardValue))
            {
                return highestCard;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Calculates if we can win the round with our current optimal card.
        /// </summary>
        /// <param name="context">PlayerTurnContext holding the turn data.</param>
        /// <param name="highestCardValue">Our optimal turn card value.</param>
        /// <param name="opponentCardValue">The oponnent's card value.</param>
        /// <returns>Boolean value representing the possibility to win the round.</returns>
        private bool TakingHandWinsRound(PlayerTurnContext context, int highestCardValue, int opponentCardValue)
        {
            if (highestCardValue > opponentCardValue)
            {
                var currentTotalPoints = context.FirstPlayerRoundPoints;
                var currentHandPoints = highestCardValue + opponentCardValue;

                return (currentTotalPoints + currentHandPoints) >= PointsRequiredForWinningRound ? true : false;
            }
            else
            {
                return false;
            }
        }
    }
}