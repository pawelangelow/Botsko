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

            //if (context.State.ShouldObserveRules)
            //{

            //}
            //else
            //{
            //    var possibleCardsToPlay = this.playerActionValidator.GetPossibleCardsToPlay(context, this.cards);
            //    //cardToPlay = this.PlayWhenRulesDoNotApply(context, possibleCardsToPlay);
            //}

            var possibleCardsToPlay = this.playerActionValidator.GetPossibleCardsToPlay(context, this.cards);
            this.FindBiggestTrumpCard(possibleCardsToPlay, context.TrumpCard.Suit);

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
            
            return cardToPlay;
        }

        // Help methods

        private bool CheckIfCanWin(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
        {
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