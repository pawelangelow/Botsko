namespace Santase.AI.BotskoPlayer
{
    using System.Linq;

    using Logic;
    using Logic.Cards;
    using Logic.Players;
    using Santase.Logic.Extensions;
    using System.Collections.Generic;

    // ReSharper disable once UnusedMember.Global
    public class BotskoPlayer : BasePlayer
    {
        private Card lastOpponentPlayedCard;
        private Card myPlayedCard;

        public BotskoPlayer()
            : this("Botsko Player")
        {
        }

        public BotskoPlayer(string name)
        {
            this.Name = name;
            this.FirstTurnLogic = new BotskoPlayerFirstTurnLogic(this.PlayerActionValidator, this.Cards);
            this.SecondTurnLogic = new BotskoPlayerSecondTurnLogic(this.PlayerActionValidator, this.Cards);
        }

        public override string Name { get; }

        public BotskoPlayerFirstTurnLogic FirstTurnLogic { get; set; }

        public BotskoPlayerSecondTurnLogic SecondTurnLogic { get; set; }

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

            this.myPlayedCard = cardToPlay;
            return this.PlayCard(cardToPlay);
        }

        public override void EndTurn(PlayerTurnContext context)
        {
            this.FirstTurnLogic.RegisterUsedCard(context.FirstPlayedCard);
            this.FirstTurnLogic.RegisterUsedCard(context.SecondPlayedCard);

            if (context.FirstPlayedCard == this.myPlayedCard)
            {
                this.lastOpponentPlayedCard = context.SecondPlayedCard;
            }
            else
            {
                this.lastOpponentPlayedCard = context.FirstPlayedCard;
            }

            base.EndTurn(context);
        }

        private Card PlayWhenObserveRules(PlayerTurnContext context, Card playerAnnounce)
        {
            var possibleCardsToPlay = this.PlayerActionValidator.GetPossibleCardsToPlay(context, this.Cards);

            var trumpSuit = context.TrumpCard.Suit;
            // var trumpsCount = possibleCardsToPlay.Where(c => c.Suit == trumpSuit).Count();
            var biggestTrumpInHand = this.FirstTurnLogic.FindTrumpCardsInHand(possibleCardsToPlay, trumpSuit).FirstOrDefault();

            if (biggestTrumpInHand != null &&
                this.FirstTurnLogic.IsBiggestTrumpInMyHand(biggestTrumpInHand))
            {
                // If have only this one ??
                if (biggestTrumpInHand.GetValue() >= 10)
                {
                    // TODO: Check trumps count in the hand

                    // TODO: Check round points

                    return biggestTrumpInHand;
                }

                if (playerAnnounce != null &&
                    playerAnnounce.Suit == trumpSuit)
                {
                    return playerAnnounce;
                }
            }

            var winningAce = this.FirstTurnLogic.HasWinningNotTrumpAce(possibleCardsToPlay, context.TrumpCard.Suit);
            if (winningAce != null)
            {
                return winningAce;
            }

            var winningTen = this.FirstTurnLogic.HasWinningNotTrumpTen(context, possibleCardsToPlay, context.TrumpCard.Suit);
            if (winningTen != null)
            {
                return winningTen;
            }

            // TODO: Fix bug - what to play when the left cards are only trumps

            // TODO: Find other smaller but winning card

            // If do not find any winning card, play the smallest one
            return this.FirstTurnLogic.FindSmallestNotTrumpCard(possibleCardsToPlay, context.TrumpCard.Suit);
        }

        private Card CallAnnounce(PlayerTurnContext context)
        {
            // 1. Check for 40
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

            // 2. Check for 20
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
    }
}