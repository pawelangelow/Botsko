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
        public const int NoOptimalCardInHand = -1;
        public const int AceCardValue = 11;
        public const int TenCardValue = 10;

        public BotskoPlayerSecondTurnLogic(IPlayerActionValidator playerActionValidator, ICollection<Card> cards, bool[,] playedCards)
            : base(playerActionValidator, cards, playedCards)
        {
        }

        /// <summary>
        /// Core logic for taking response decisions.
        /// </summary>
        /// <param name="context">PlayerTurnContext holding the turn data.</param>
        /// <returns>Response card.</returns>
        public override Card Execute(PlayerTurnContext context, BasePlayer basePlayer, Card playerAnnounce)
        {
            //// PAVEL EDIT
            //// Pavel logic

            var opponentCard = context.FirstPlayedCard;

            //// Check for card which ends the game
            var winCard = this.IsThereWinCard(context, context.TrumpCard.Suit, opponentCard, playerAnnounce);
            if (winCard != null)
            {
                return winCard;
            }

            //// Check if opponent plays big value card
            if (opponentCard.GetValue() >= 10)
            {
                var myCardInThisSituation = this.CardToBeatHis(opponentCard, context);
                if (myCardInThisSituation != null)
                {
                    return myCardInThisSituation;
                }
            }

            //// If opponent has many points or announcement of 40, get as much as you can
            var opponentPoints = BotskoPlayer.BotskoIsFirstPlayer ?
                context.SecondPlayerRoundPoints : context.FirstPlayerRoundPoints;
            var botskoPoints = BotskoPlayer.BotskoIsFirstPlayer ?
                context.FirstPlayerRoundPoints : context.SecondPlayerRoundPoints;
            if ((opponentPoints > 45 && botskoPoints < 30) ||
                context.FirstPlayerAnnounce == Announce.Forty)
            {
                var cardToPlayToSave = this.CardToSave(opponentCard, context);
                if (cardToPlayToSave != null)
                {
                    return cardToPlayToSave;
                }
            }

            // Check if we break 20 or 40


            // Check for good last card
            if (context.TrumpCard.GetValue() > 2 && context.CardsLeftInDeck == 2)
            {
                var cardToPlay = this.CardToGetTheLastOfDeck(opponentCard, context);
                if (cardToPlay != null)
                {
                    return cardToPlay;
                }
            }

            // Default
            return this.cards
                .OrderBy(c => c.GetValue())
                .First();


            //// END OF PAVEL EDIT

            ////// IVANS shits
            //Card card = null;

            //if (context.State.ShouldObserveRules)
            //{
            //    card = this.TakingHandWinsTheRound(context);

            //    if (card != null)
            //    {
            //        return card;
            //    }

            //    card = this.ThrowMinimalCard(context);
            //    if (card != null)
            //    {
            //        return card;
            //    }
            //}
            //else
            //{
            //    // Highest priority.
            //    card = this.TakingHandWithAnyOptimalCardWinsTheRound(context);
            //    if (card != null)
            //    {
            //        return card;
            //    }

            //    // First-Mid priority logic
            //    card = this.TakingHandWithOptimalCardDifferentThanTrump(context);
            //    if (card != null)
            //    {
            //        return card;
            //    }

            //    // Second-Mid priority logic
            //    card = this.TakingHandWithOptimalTrumpCard(context);
            //    if (card != null)
            //    {
            //        return card;
            //    }

            //    card = this.ThrowMinimalCard(context);
            //    if (card != null)
            //    {
            //        return card;
            //    }
            //}
            ////END OF THEM

            return base.Execute(context, basePlayer, playerAnnounce);
        }

        //// PAVEL
        private Card IsThereWinCard(PlayerTurnContext context, CardSuit trumpSuit, Card opponentCard, Card playerAnnounce)
        {
            var trumps = this.cards
                .Where(c => c.Suit == trumpSuit)
                .OrderByDescending(c => c.GetValue())
                .ToArray();

            if (trumps.Count() == 0)
            {
                return null;
            }

            int botskoPoints = BotskoPlayer.BotskoIsFirstPlayer ?
                    context.FirstPlayerRoundPoints : context.SecondPlayerRoundPoints;

            //// Case 1: we have trump and playing it will end the game
            if (trumps[0].GetValue() + botskoPoints + opponentCard.GetValue() >= 66)
            {
                return trumps[0];
            }

            //// Case 2: we have 2 trumps and playing them will end the game
            if (trumps.Count() >= 2)
            {
                var trumpsSum = trumps[0].GetValue() + trumps[1].GetValue();
                if (trumpsSum + botskoPoints + opponentCard.GetValue() >= 66)
                {
                    return trumps[1];
                }
            }

            //// Case 3: we have 40 and something else, playing something else to call 40 on next round
            if (playerAnnounce != null &&
                playerAnnounce.Suit == trumps[0].Suit &&
                trumps.Length >= 3)
            {
                return trumps
                    .Where(c => c.Type != CardType.King && c.Type != CardType.Queen)
                    .OrderBy(c => c.GetValue())
                    .First();
            }

            return null;
        }

        private Card CardToBeatHis(Card opponentCard, PlayerTurnContext context)
        {
            if (opponentCard.Suit == context.TrumpCard.Suit)
            {
                return null; // There is no point
            }

            var myCard = this.cards
                .Where(c =>
                c.GetValue() > opponentCard.GetValue() &&
                c.Suit == opponentCard.Suit)
                .FirstOrDefault();

            if (myCard != null)
            {
                return myCard; // We have ace to beat his 10
            }

            var trumpCards = this.cards
                .Where(c => c.Suit == context.TrumpCard.Suit)
                .OrderBy(c => c.GetValue())
                .ToList();
            var trumpCard = trumpCards
                .FirstOrDefault();

            if (trumpCard != null) // TODO: Optimize it
            {
                // In case we have 9 and is good to change
                if (trumpCard.Type == CardType.Nine &&
                    this.IsGoodToChange(context) &&
                    (trumpCards.Last().Type != CardType.Queen && trumpCards.Last().Type != CardType.King))
                {
                    return trumpCards.Last();
                }

                if (this.CheckForPossible20or40(trumpCard))
                {
                    return trumpCard; // Booo, get this damage!
                }
            }

            return null;
        }

        private bool IsGoodToChange(PlayerTurnContext context)
        {
            if (context.TrumpCard.GetValue() > 2)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if this card is not a part from possible 20.
        /// </summary>
        /// <param name="card">Queen or King to be checked.</param>
        /// <returns>If the other card from 20 is already played returns false
        ///          else the card is still in the game.</returns>
        public bool CheckForPossible20or40(Card card)
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

        private Card CardToSave(Card opponentCard, PlayerTurnContext context)
        {
            Card cardToPlay = this.cards
                    .Where(c => c.Suit == opponentCard.Suit && c.GetValue() > opponentCard.GetValue())
                    .OrderByDescending(c => c.GetValue())
                    .FirstOrDefault();

            if (opponentCard.Suit == context.TrumpCard.Suit)
            {
                return cardToPlay;
            }
            else
            {
                if (cardToPlay == null)
                {
                    cardToPlay = this.cards
                        .Where(c => c.Suit == context.TrumpCard.Suit)
                        .OrderByDescending(c => c.GetValue()) //Maybe check for 40
                        .FirstOrDefault();
                }
            }

            return cardToPlay; // Plays he highest card
        }

        private Card CardToGetTheLastOfDeck(Card opponentCard, PlayerTurnContext context)
        {
            Card cardToReturn = null;

            if (opponentCard.GetValue() <= 4)
            {
                cardToReturn = this.cards
                    .OrderBy(c => c.GetValue())
                    .FirstOrDefault();
            }
            else
            {
                var countOf40Cards = this.cards
                    .Where(c => c.Suit == context.TrumpCard.Suit &&
                    (c.Type == CardType.King || c.Type == CardType.Queen))
                    .Count();

                if (countOf40Cards == 1 &&
                    (context.TrumpCard.Type == CardType.King || context.TrumpCard.Type == CardType.Queen))
                {
                    cardToReturn = this.cards
                    .OrderBy(c => c.GetValue())
                    .FirstOrDefault(); // Maybe get his card in other cases
                }
            }

            return cardToReturn;
        }

        //// END OF PAVEL

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
        private Card GetHighestCard()
        {
            var card = this.cards.OrderByDescending(x => x.GetValue()).FirstOrDefault();

            return card;
        }

        /// <summary>
        /// Finds the highest card available in hand with CardSuit equal to the trump card.
        /// </summary>
        /// <param name="context">PlayerTurnContext holding the turn data.</param>
        /// <param name="firstSuit">CardSuit of the trump card.</param>
        /// <returns>The highest card found that matches the conditions.</returns>
        private Card GetHighestCard(CardSuit firstSuit)
        {
            var card = this.cards
                .Where(x => x.Suit == firstSuit)
                .OrderByDescending(x => x.GetValue())
                .FirstOrDefault();

            return card;
        }

        /// <summary>
        /// Finds the highest card available in hand with CardSuit equal either to the opponent's card or the trump card.
        /// </summary>
        /// <param name="context">PlayerTurnContext holding the turn data.</param>
        /// <param name="firstSuit">CardSuit of the opponent's card.</param>
        /// <param name="secondSuit">CardSuit of the trump card.</param>
        /// <returns>The highest card found that matches the conditions.</returns>
        private Card GetHighestCard(CardSuit firstSuit, CardSuit secondSuit)
        {
            var card = this.cards
                .Where(x => x.Suit == firstSuit || x.Suit == secondSuit)
                .OrderByDescending(x => x.GetValue())
                .FirstOrDefault();

            return card;
        }

        private Card GetLowestCard()
        {
            var card = this.cards.OrderBy(x => x.GetValue()).FirstOrDefault();

            return card;
        }

        private Card GetLowestCard(CardSuit firstSuit)
        {
            var card = this.cards
               .Where(x => x.Suit == firstSuit)
               .OrderBy(x => x.GetValue())
               .FirstOrDefault();

            return card;
        }

        private Card GetLowestCard(CardSuit firstSuit, CardSuit secondSuit)
        {
            var card = this.cards
               .Where(x => x.Suit == firstSuit || x.Suit == secondSuit)
               .OrderBy(x => x.GetValue())
               .FirstOrDefault();

            return card;
        }

        private Card GetLowestCard(CardSuit firstSuit, CardSuit secondSuit, CardSuit thirdSuit)
        {
            var card = this.cards
               .Where(x => x.Suit == firstSuit || x.Suit == secondSuit || x.Suit == thirdSuit)
               .OrderBy(x => x.GetValue())
               .FirstOrDefault();

            return card;
        }

        private Card GetLowestCard(CardSuit firstSuit, CardSuit secondSuit, CardSuit thirdSuit, CardSuit fourthSuit)
        {
            var card = this.cards
               .Where(x => x.Suit == firstSuit || x.Suit == secondSuit || x.Suit == thirdSuit || x.Suit == fourthSuit)
               .OrderBy(x => x.GetValue())
               .FirstOrDefault();

            return card;
        }

        private Card GetLowestCardDifferentThan(CardSuit firstSuit)
        {
            var card = this.cards
                .Where(x => x.Suit != firstSuit)
                .OrderBy(x => x.GetValue())
                .FirstOrDefault();

            return card;
        }

        /// <summary>
        /// Gets the optimal card which will win the round by taking the current hand.
        /// </summary>
        /// <param name="context">PlayerTurnContext holding the turn data.</param>
        /// <returns>The optimal card that wins the round or NULL if that is not possible.</returns>
        private Card TakingHandWithAnyOptimalCardWinsTheRound(PlayerTurnContext context)
        {
            var trumpCardSuit = context.TrumpCard.Suit;
            var opponentCard = context.FirstPlayedCard;
            var opponentCardSuit = opponentCard.Suit;

            Card highestCard = null;

            // Gets the highest response card possible.
            if (opponentCardSuit != trumpCardSuit)
            {
                highestCard = this.GetHighestCard(opponentCardSuit, trumpCardSuit);
            }
            else
            {
                highestCard = this.GetHighestCard(trumpCardSuit);
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
        /// Checks if it is worth taking the current opponent card and finds the optimal response card to be played.
        /// </summary>
        /// <param name="context">PlayerTurnContext holding the turn data.</param>
        /// <returns>The optimal card that takes the hand or NULL if that is not possible.</returns>
        private Card TakingHandWithOptimalCardDifferentThanTrump(PlayerTurnContext context)
        {
            var trumpCardSuit = context.TrumpCard.Suit;
            var opponentCard = context.FirstPlayedCard;
            var opponentCardSuit = opponentCard.Suit;
            var opponentCardValue = opponentCard.GetValue();
            var hasCardWithCorrespondingSuit = this.HasCardsWithSuit(opponentCardSuit);

            Card card = null;

            if (hasCardWithCorrespondingSuit && opponentCardSuit != trumpCardSuit)
            {
                card = this.GetHighestCard(opponentCardSuit);

                var myCardValue = card != null ? card.GetValue() : NoOptimalCardInHand;

                if (myCardValue > opponentCardValue && this.HandSumGreaterThan20(myCardValue, opponentCardValue))
                {
                    return card;
                }
                else if (myCardValue > opponentCardValue && !this.CardBreaks20Or40(card))
                {
                    return card;
                }
                else if (myCardValue > opponentCardValue && this.CardBreaks20Or40(card))
                {
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if it is worth taking the current opponent card and finds the optimal response card to be played.
        /// </summary>
        /// <param name="context">PlayerTurnContext holding the turn data.</param>
        /// <returns>The optimal card that takes the hand or NULL if such does not exist.</returns>
        private Card TakingHandWithOptimalTrumpCard(PlayerTurnContext context)
        {
            var trumpCardSuit = context.TrumpCard.Suit;

            if (!this.HasCardsWithSuit(trumpCardSuit))
            {
                return null;
            }

            var opponentCard = context.FirstPlayedCard;
            var opponentCardSuit = opponentCard.Suit;
            var opponentCardValue = opponentCard.GetValue();

            if (!this.HasCardGreaterThanOpponent(opponentCard))
            {
                return null;
            }

            Card card = null;

            if (opponentCardSuit != trumpCardSuit)
            {
                if (!this.IsWorthSpendingTrump(opponentCardValue))
                {
                    return null;
                }
                else
                {
                    card = this.GetLowestCard(trumpCardSuit);

                    if (this.CardBreaks20Or40(card))
                    {
                        return null;
                    }
                    else
                    {
                        return card;
                    }
                }
            }

            // If opponent card is trump
            else
            {
                Card previousCard = null;

                if (this.IsWorthSpendingTrump(opponentCardValue))
                {
                    card = this.GetHighestCard(trumpCardSuit);

                    return card;
                }
                else
                {
                    card = this.GetLowestCardDifferentThan(trumpCardSuit);

                    if (card != null && card.GetValue() < 10)
                    {
                        return card;
                    }

                    if (card == null || card.GetValue() > 5)
                    {
                        previousCard = card;
                        card = this.GetLowestCard(trumpCardSuit);

                        if (card.GetValue() > 5)
                        {
                            return previousCard;
                        }
                        else
                        {
                            return card;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the minimal optimal card and throws it as a response to the opponent's one.
        /// </summary>
        /// <param name="context">PlayerTurnContext holding the turn data.</param>
        /// <returns>The optimal weakest card for response.</returns>
        private Card ThrowMinimalCard(PlayerTurnContext context)
        {
            var trumpCardSuit = context.TrumpCard.Suit;
            var opponentCard = context.FirstPlayedCard;
            var opponentCardSuit = opponentCard.Suit;

            Card card = null;

            if (this.HasCardsWithSuit(opponentCardSuit))
            {
                card = this.GetLowestCard(opponentCardSuit);
            }
            else if (this.HasCardsWithSuitDifferentThan(trumpCardSuit))
            {
                card = this.GetLowestCardDifferentThan(trumpCardSuit);
            }
            else
            {
                card = this.GetLowestCard();
            }

            return card;
        }

        private Card TakingHandWinsTheRound(PlayerTurnContext context)
        {
            var trumpCardSuit = context.TrumpCard.Suit;
            var opponentCard = context.FirstPlayedCard;
            var opponentCardSuit = opponentCard.Suit;

            Card card = null;

            // Taking hand wins the round
            if (this.CanTakeHand(opponentCard, trumpCardSuit) && this.TakingHandWinsTheRound(context, opponentCard, out card))
            {
                if (card != null)
                {
                    return card;
                }
            }

            else if (this.HasCardGreaterThanOpponent(opponentCard))
            {
                return this.GetHighestCard(opponentCard.Suit);
            }

            else if (!this.HasCardsWithSuit(opponentCard.Suit) && opponentCard.Suit != trumpCardSuit && this.HasCardsWithSuit(trumpCardSuit))
            {
                return this.GetHighestCard(trumpCardSuit);
            }


            else if (this.CantPlayOtherThanOpponentSuit(opponentCardSuit))
            {
                card = this.GetLowestCard(opponentCardSuit);

                return card;
            }

            // Is worth taking hand
            else if (this.IsWorthTaking(opponentCard) && this.CanTakeHand(opponentCard, trumpCardSuit))
            {
                card = this.GetHighestCard(opponentCardSuit);

                if (card != null)
                {
                    return card;
                }
                else
                {
                    card = this.GetHighestCard(trumpCardSuit);

                    return card;
                }
            }

            // Is not worth taking hand or cannot take hand
            else
            {
                card = this.GetLowestCard(opponentCardSuit);

                if (card != null)
                {
                    return card;
                }
                else
                {
                    card = this.GetLowestCard();

                    return card;
                }
            }

            return null;
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
                var currentTotalPoints = BotskoPlayer.BotskoIsFirstPlayer ?
                    context.FirstPlayerRoundPoints : context.SecondPlayerRoundPoints;

                var currentHandPoints = highestCardValue + opponentCardValue;

                return (currentTotalPoints + currentHandPoints) >= PointsRequiredForWinningRound ? true : false;
            }
            else
            {
                return false;
            }
        }

        private bool HandSumGreaterThan20(int cardValue, int opponentCardValue)
        {
            return (cardValue + opponentCardValue) > 20;
        }

        private bool HasCardsWithSuit(CardSuit firstSuit)
        {
            return this.cards.Any(x => x.Suit == firstSuit);
        }

        private bool HasCardsWithSuitDifferentThan(CardSuit firstSuit)
        {
            return this.cards.Any(x => x.Suit == firstSuit);
        }

        private bool HasCardsWithSuitDifferentThan(CardSuit firstSuit, CardSuit secondSuit)
        {
            return this.cards.Any(x => x.Suit != firstSuit && x.Suit != secondSuit);
        }

        private bool HasCardsWithSuitDifferentThan(CardSuit firstSuit, CardSuit secondSuit, CardSuit thirdSuit)
        {
            return this.cards.Any(x => x.Suit != firstSuit && x.Suit != secondSuit && x.Suit != thirdSuit);
        }

        /// <summary>
        /// Checks if the card passed as argument is returned as a response card, will break 20/40.
        /// </summary>
        /// <param name="card">The card pending approval to be returned as response.</param>
        /// <returns>True, if responding with the card will break 20/40. False, otherwise.</returns>
        private bool CardBreaks20Or40(Card card)
        {
            var cardSuit = card.Suit;
            var cardType = card.Type;

            if (cardType == CardType.Queen)
            {
                var deuce = this.cards.Where(x => x.Suit == cardSuit && x.Type == CardType.King).FirstOrDefault();

                return deuce != null ? true : false;
            }
            else if (cardType == CardType.King)
            {
                var deuce = this.cards.Where(x => x.Suit == cardSuit && x.Type == CardType.Queen).FirstOrDefault();

                return deuce != null ? true : false;
            }
            else
            {
                return false;
            }
        }

        private bool IsWorthSpendingTrump(int opponentCardValue)
        {
            return opponentCardValue > 4;
        }

        private bool IsWorthTaking(Card opponentCard)
        {
            return opponentCard.GetValue() >= 3;
        }

        private bool HasCardGreaterThanOpponent(Card opponentCard)
        {
            return this.cards.Any(x => x.Suit == opponentCard.Suit && x.GetValue() > opponentCard.GetValue());
        }

        private bool CanTakeHand(Card opponentCard, CardSuit trumpCardSuit)
        {
            if (this.HasCardsWithSuit(opponentCard.Suit))
            {
                var canTake = this.cards.Any(x => x.Suit == opponentCard.Suit && x.GetValue() > opponentCard.GetValue());

                return canTake;
            }
            else if (this.HasCardsWithSuit(trumpCardSuit))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CantPlayOtherThanOpponentSuit(CardSuit opponentCardSuit)
        {
            return this.cards.Any(x => x.Suit != opponentCardSuit);
        }

        private bool TakingHandWinsTheRound(PlayerTurnContext context, Card opponentCard, out Card myPlayCard)
        {
            if (this.HasCardsWithSuit(opponentCard.Suit))
            {
                var card = this.cards
                    .Where(x => x.Suit == opponentCard.Suit && x.GetValue() > opponentCard.GetValue())
                    .OrderByDescending(x => x.GetValue())
                    .FirstOrDefault();

                myPlayCard = card;

                if (card.GetValue() > opponentCard.GetValue())
                {
                    var takingScore = card.GetValue() + opponentCard.GetValue();

                    int botskoPoints = BotskoPlayer.BotskoIsFirstPlayer ?
                    context.FirstPlayerRoundPoints : context.SecondPlayerRoundPoints;

                    if (takingScore + botskoPoints >= PointsRequiredForWinningRound)
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                var card = this.cards
                    .Where(x => x.Suit == context.TrumpCard.Suit)
                    .OrderByDescending(x => x.GetValue())
                    .FirstOrDefault();

                myPlayCard = card;

                if (myPlayCard.GetValue() > opponentCard.GetValue())
                {
                    var takingScore = myPlayCard.GetValue() + opponentCard.GetValue();

                    int botskoPoints = BotskoPlayer.BotskoIsFirstPlayer ?
                    context.FirstPlayerRoundPoints : context.SecondPlayerRoundPoints;

                    if (takingScore + botskoPoints >= PointsRequiredForWinningRound)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}