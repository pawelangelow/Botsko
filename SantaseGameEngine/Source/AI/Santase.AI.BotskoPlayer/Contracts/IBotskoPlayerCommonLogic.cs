namespace Santase.AI.BotskoPlayer.Contracts
{
    using System.Collections.Generic;
    using Logic.Cards;
    using Logic.PlayerActionValidate;
    using Logic.Players;

    public interface IBotskoPlayerCommonLogic
    {
        Card Execute(PlayerTurnContext context, IPlayerActionValidator playerActionValidator, ICollection<Card> cards);
    }
}