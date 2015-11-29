namespace Santase.AI.BotskoPlayer
{
    using System.Collections.Generic;
    using System.Linq;

    using Logic.Cards;
    using Logic.PlayerActionValidate;
    using Logic.Players;

    public class BotskoPlayerFirstTurnLogic : BotskoPlayerCommonLogic
    {
        public BotskoPlayerFirstTurnLogic(IPlayerActionValidator playerActionValidator, ICollection<Card> cards)
            : base(playerActionValidator, cards)
        {
        }

        public override Card Execute(PlayerTurnContext context)
        {
            Card cardToPlay = null;
            // TODO: Check if can ChangeTrump()

            // TODO: Check if CanClose()

            return base.Execute(context);
        }

        private Card PlayWhenRulesDoNotApply(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
        {
            Card cardToPlay = null;

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

        private bool CheckIfCanWin(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
        {
            // Check cards that are passed, my cards (trumps), calculate result
            var biggestTrumpCard = this.FindBiggestTrumpCard(possibleCardsToPlay, context.TrumpCard.Suit);
            // var biggestLeftTrump = this.FindBiggestTrumpCard(..., context.TrumpCard.Suit);
            if (context.SecondPlayerRoundPoints + biggestTrumpCard.GetValue() >= 66)
            {
                return true;
            }

            return false;
        }

        private Card FindBiggestTrumpCard(ICollection<Card> possibleCardsToPlay, CardSuit trump)
        {
            var result = possibleCardsToPlay
                .Where(c => c.Suit == trump)
                .OrderByDescending(c => c.Type)
                .FirstOrDefault();

            return result;
        }
    }
}