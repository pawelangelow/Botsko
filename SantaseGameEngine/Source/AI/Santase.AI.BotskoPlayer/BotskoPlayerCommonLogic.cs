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
        public virtual Card Execute(PlayerTurnContext context, IPlayerActionValidator playerActionValidator, ICollection<Card> cards)
        {
            var possibleCardsToPlay = playerActionValidator.GetPossibleCardsToPlay(context, cards);
            var shuffledCards = possibleCardsToPlay.Shuffle();
            var cardToPlay = shuffledCards.First();

            return cardToPlay;
        }
    }
}