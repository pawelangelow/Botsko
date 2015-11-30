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
    using Models;

    public class BotskoPlayerSecondTurnLogic : BotskoPlayerCommonLogic
    {
        // Extract to constants class or use already defined constants.
        public const int PointsRequiredForWinningRound = 66;
        public const int NoOptimalCardInHand = -1;

        public BotskoPlayerSecondTurnLogic(IPlayerActionValidator playerActionValidator, ICollection<Card> cards)
            : base(playerActionValidator, cards)
        {
        }

        /// <summary>
        /// Core logic for taking response decisions.
        /// </summary>
        /// <param name="context">PlayerTurnContext holding the turn data.</param>
        /// <returns>Response card.</returns>
        public override Card Execute(PlayerTurnContext context, BasePlayer basePlayer)
        {
            var card = this.GetCardForTakingHandWinsTheRound(context);

            if (card != null)
            {
                return card;
            }

            // Next priority logic

            return base.Execute(context, basePlayer);
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
        /// Finds the highest card available in hand with CardSuit equal to the trump card.
        /// </summary>
        /// <param name="context">PlayerTurnContext holding the turn data.</param>
        /// <param name="desiredCardSuit">CardSuit of the trump card.</param>
        /// <returns>The highest card found that matches the conditions.</returns>
        private Card GetHighestCard(PlayerTurnContext context, CardSuit desiredCardSuit)
        {
            var card = this.cards
                .Where(x => x.Suit == desiredCardSuit)
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
        private Card GetCardForTakingHandWinsTheRound(PlayerTurnContext context)
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

            var highestCardValue = highestCard != null ? highestCard.GetValue() : NoOptimalCardInHand;
            var opponentCardValue = opponentCard.GetValue();

            // Returns the highest selected card which can win the round.
            // Otherwise returns null object indicating that this move is not optimal.
            if (this.TakingHandWinsTheRound(context, highestCardValue, opponentCardValue))
            {
                return highestCard;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Cases:
        /// 1.Proverqvai dali ako imash ot suit-a na oponenta, kartata koqto imash e po-golqma ot negovata, 
        /// 1.1 Ako imash po-golqma, no ti razvalq 20, ne q davai, 
        /// 1.2 Ako ne ti razvalq dvadeset, vzimai,
        /// 1.3 Ako nqmash karta ot suit-a na oponenta, proveri dali imash kozove => case 2.
        /// 2. Ako imash kozove, Proverqvai dali protivnikovata karta struva poveche ot 4 tochki, ako e po-malko ne si struva da habish koz
        /// i vurni nai-malkoto ot druga boq, koeto ne razvalq 20.
        /// 1.4 TODO: Ako nqma takova i imash samo tuzove i desqtki, vzemi s koz.
        /// 2.1 Ako kartata na protivnika struva poveche. Proveri dali nqma da si razvalish 40- s koza.
        /// Gets the optimal card which will win the turn by taking the current hand.
        /// With priority to choosing card with suit different than the trump's one.
        /// If such doesn't exist - it gets the lowest of the trumps.
        /// If the player has no trumps
        /// </summary>
        /// <param name="context">PlayerTurnContext holding the turn data.</param>
        /// <returns>The optimal card that takes the hand or NULL if that is not possible.</returns>
        private Card GetCardForTakingHandWithHighestOptimalCard(PlayerTurnContext context)
        {
            var trumpCardSuit = context.TrumpCard.Suit;
            var opponentCard = context.SecondPlayedCard;
            var opponentCardSuit = context.SecondPlayedCard.Suit;
            var hasCardWithCorrespondingSuit = this.cards.Any(x => x.Suit == opponentCardSuit);

            Card highestCard = null;

            // Gets the highest response card possible.
            // First tries to find the highest card with opponent's suit that can take the current hand.
            // If such is not found - tries to find the highest card with trump suit that can take the current hand.
            if (hasCardWithCorrespondingSuit && opponentCardSuit != trumpCardSuit)
            {
                highestCard = this.GetHighestCard(context, opponentCardSuit);
            }
            else
            {
                highestCard = this.GetHighestCard(context, trumpCardSuit);
            }

            // Gets the highest card's value.
            // If no card is returned from any of the above two methods,
            // Sets the value to -1, corresponding to NoOptimalCardInHand found.
            var highestCardValue = highestCard != null ? highestCard.GetValue() : NoOptimalCardInHand;
            var opponentCardValue = opponentCard.GetValue();

            if (highestCardValue > opponentCardValue)
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
        private bool TakingHandWinsTheRound(PlayerTurnContext context, int highestCardValue, int opponentCardValue)
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