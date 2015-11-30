﻿namespace Santase.AI.BotskoPlayer
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

                cardToPlay = this.FirstTurnLogic.Execute(context, this, announce);
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

        private Card PlayWhenIsClosed(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay, Card playerAnnounce)
        {
            Card cardToPlay = null;
            var trumpSuit = context.TrumpCard.Suit;
            var trumpsCount = possibleCardsToPlay.Where(c => c.Suit == trumpSuit).Count();

            var biggestTrumpInHand = this.FirstTurnLogic.FindBiggestTrumpCard(possibleCardsToPlay, trumpSuit);
            if (this.FirstTurnLogic.IsBiggestTrumpIsInMyHand(biggestTrumpInHand))
            {
                if (biggestTrumpInHand.GetValue() >= 10)
                {
                    return biggestTrumpInHand;
                }

                if (playerAnnounce.Suit == context.TrumpCard.Suit)
                {
                    return playerAnnounce;
                }

                // Check other cases
            }

            return cardToPlay;
        }

        private Card CallAnnounce(PlayerTurnContext context)
        {
            // First check for 40
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

            // Check for 20
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