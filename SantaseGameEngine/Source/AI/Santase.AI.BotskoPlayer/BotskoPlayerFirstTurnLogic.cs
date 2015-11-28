namespace Santase.AI.BotskoPlayer
{
    using System.Collections.Generic;
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

            if (context.State.ShouldObserveRules)
            {

            }
            else
            {
                var cardsToPlay = this.playerActionValidator.GetPossibleCardsToPlay(context, this.cards);
            }

            return base.Execute(context);
        }
    }
}