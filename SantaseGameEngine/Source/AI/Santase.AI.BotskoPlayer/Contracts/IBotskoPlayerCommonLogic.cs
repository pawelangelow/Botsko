namespace Santase.AI.BotskoPlayer.Contracts
{
    using Logic.Cards;
    using Logic.Players;

    public interface IBotskoPlayerCommonLogic
    {
        Card Execute(PlayerTurnContext context);
    }
}