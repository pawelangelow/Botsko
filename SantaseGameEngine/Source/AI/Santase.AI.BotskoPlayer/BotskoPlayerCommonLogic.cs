namespace Santase.AI.BotskoPlayer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Contracts;
    using Logic.Cards;
    using Logic.Extensions;
    using Logic.PlayerActionValidate;
    using Logic.Players;

    public class BotskoPlayerCommonLogic : IBotskoPlayerCommonLogic
    {
        // TODO: Remove the dummy logic and make the method abstract, so that the other two cases may override it.
        public virtual Card Execute(PlayerTurnContext context)
        {
            var possibleCardsToPlay = this.playerActionValidator.GetPossibleCardsToPlay(context, this.cards);
            var shuffledCards = possibleCardsToPlay.Shuffle();
            var cardToPlay = shuffledCards.First();

            return cardToPlay;
        }

        public BotskoPlayerCommonLogic(IPlayerActionValidator playerActionValidator, ICollection<Card> cards)
        {
            this.playerActionValidator = playerActionValidator;
            this.cards = cards;
        }

        private IPlayerActionValidator playerActionValidator;
        private ICollection<Card> cards;
    }
}