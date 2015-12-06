namespace Santase.AI.BotskoPlayer
{
    using System.Linq;

    using Logic;
    using Logic.Cards;
    using Logic.Players;
    using Santase.Logic.Extensions;
    using System.Collections.Generic;
    using System;

    // ReSharper disable once UnusedMember.Global
    public class BotskoPlayer : BasePlayer
    {
        public BotskoPlayer()
            : this("Botsko Player")
        {
        }

        public BotskoPlayer(string name)
        {
            this.Name = name;
            this.PlayedCards = new bool[4, 6];
            this.FirstTurnLogic = new BotskoPlayerFirstTurnLogic(this.PlayerActionValidator, this.Cards, this.PlayedCards);
            this.SecondTurnLogic = new BotskoPlayerSecondTurnLogic(this.PlayerActionValidator, this.Cards, this.PlayedCards);
        }

        public override string Name { get; }

        public BotskoPlayerFirstTurnLogic FirstTurnLogic { get; set; }

        public BotskoPlayerSecondTurnLogic SecondTurnLogic { get; set; }

        public bool[,] PlayedCards { get; set; }

        public override PlayerAction GetTurn(PlayerTurnContext context)
        {
            Card cardToPlay = null;
            var announce = this.CallAnnounce(context);

            if (context.FirstPlayedCard == null)
            {
                // Think about times when is better not to change the trump card despite it is possible
                if (this.PlayerActionValidator.IsValid(PlayerAction.ChangeTrump(), context, this.Cards))
                {
                    return this.ChangeTrump(context.TrumpCard);
                }

                if (this.FirstTurnLogic.IsGoodToClose(context))
                {
                    this.CloseGame();
                }

                if (context.State.ShouldObserveRules)
                {
                    cardToPlay = this.PlayWhenObserveRules(context, announce);
                }

                // Remove if-statement and left only the logic in it.
                if (!context.State.ShouldObserveRules)
                {
                    cardToPlay = this.FirstTurnLogic.PlayWhenRulesDoNotApply(
                        context,
                        this.PlayerActionValidator.GetPossibleCardsToPlay(context, this.Cards),
                        announce);
                }

                // In worst case the logic above do not find card to play
                if (cardToPlay == null)
                {
                    cardToPlay = this.FirstTurnLogic.Execute(context, this, announce);
                }
            }
            else
            {
                cardToPlay = this.SecondTurnLogic.Execute(context, this, announce);
            }

            return this.PlayCard(cardToPlay);
        }

        public override void EndTurn(PlayerTurnContext context)
        {
            this.FirstTurnLogic.RegisterUsedCard(context.FirstPlayedCard);
            this.FirstTurnLogic.RegisterUsedCard(context.SecondPlayedCard);

            base.EndTurn(context);
        }

        public override void EndRound()
        {
            this.ClearPlayedCards();
            base.EndRound();
        }

        private Card PlayWhenObserveRules(PlayerTurnContext context, Card playerAnnounce)
        {
            var possibleCardsToPlay = this.PlayerActionValidator.GetPossibleCardsToPlay(context, this.Cards);
            var trumpSuit = context.TrumpCard.Suit;

            // 1. Check if this is the last card.
            if (possibleCardsToPlay.Count == 1)
            {
                return possibleCardsToPlay.FirstOrDefault();
            }

            // 2. Check if there is 40/20 and points are 26/46 ot more.
            if (playerAnnounce != null)
            {
                if ((playerAnnounce.Suit == trumpSuit && context.SecondPlayerRoundPoints >= 26) ||
                    (playerAnnounce.Suit != trumpSuit && context.SecondPlayerRoundPoints >= 46))
                {
                    return playerAnnounce;
                }
            }

            var trumpCards = this.FirstTurnLogic.FindTrumpCardsInHand(possibleCardsToPlay, trumpSuit);
            var trumpCardsCount = trumpCards.Count();
            if (trumpCardsCount == 0)
            {
                return this.PlayNotTrumpCard(possibleCardsToPlay, playerAnnounce, trumpSuit);
            }

            var biggestTrumpInHand = trumpCards.FirstOrDefault();

            // Check if the biggest trump in hand is winning card
            if (trumpCardsCount > 1)
            {
                if (this.FirstTurnLogic.IsBiggestCardInMyHand(biggestTrumpInHand))
                {
                    // Check if the biggest trump in hand is a King and have 40
                    if (playerAnnounce != null &&
                        playerAnnounce.Suit == trumpSuit &&
                        biggestTrumpInHand.Type == CardType.King)
                    {
                        return playerAnnounce;
                    }

                    return biggestTrumpInHand;
                }
                else if (playerAnnounce != null &&
                        playerAnnounce.Suit == trumpSuit)
                {
                    return playerAnnounce;
                }
            }

            if (trumpCardsCount != possibleCardsToPlay.Count)
            {
                return this.PlayNotTrumpCard(possibleCardsToPlay, playerAnnounce, trumpSuit);
            }

            if (trumpCardsCount == possibleCardsToPlay.Count)
            {
                return trumpCards.Last();
            }

            // Never goes here I hope
            return this.FirstTurnLogic.Execute(context, this, playerAnnounce);
        }

        private Card PlayNotTrumpCard(ICollection<Card> possibleCardsToPlay, Card playerAnnounce, CardSuit trumpSuit)
        {
            // 1. Check for winning not trump card
            var winningNotTrumpCard = this.FirstTurnLogic.HasWinningNotTrumpCard(possibleCardsToPlay, trumpSuit);
            if (winningNotTrumpCard != null)
            {
                return winningNotTrumpCard;
            }

            // 2. Call 20
            if (playerAnnounce != null &&
                playerAnnounce.Suit != trumpSuit)
            {
                return playerAnnounce;
            }

            // 3. Return the smallest card on the hand
            return this.FirstTurnLogic.FindSmallestNotTrumpCard(possibleCardsToPlay, trumpSuit);
        }

        private Card CallAnnounce(PlayerTurnContext context)
        {
            // 1. Check for 40.
            var possibleCards = this.PlayerActionValidator.GetPossibleCardsToPlay(context, this.Cards);
            var trumpSuit = context.TrumpCard.Suit;
            var announceCard = possibleCards
                .Where(c => c.Type == CardType.Queen &&
                        this.AnnounceValidator.GetPossibleAnnounce(this.Cards, c, context.TrumpCard) == Announce.Forty)
                .FirstOrDefault();

            if (announceCard != null)
            {
                return announceCard;
            }

            // 2. Check for 20.
            announceCard = possibleCards
                .Where(c => c.Type == CardType.Queen &&
                        this.AnnounceValidator.GetPossibleAnnounce(this.Cards, c, context.TrumpCard) == Announce.Twenty)
                .FirstOrDefault();

            if (announceCard != null)
            {
                return announceCard;
            }

            return null;
        }

        private void ClearPlayedCards()
        {
            for (int i = 0; i < this.PlayedCards.GetLength(0); i++)
            {
                for (int j = 0; j < this.PlayedCards.GetLength(1); j++)
                {
                    this.PlayedCards[i, j] = false;
                }
            }
        }
    }
}