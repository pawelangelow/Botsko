namespace Santase.AI.BotskoPlayer
{
    using System.Linq;

    using Santase.Logic.Extensions;
    using Santase.Logic.Players;
    using Logic.Cards;

    /// <summary>
    /// This dummy player follows the rules and always plays random card.
    /// Dummy never changes the trump or closes the game.
    /// </summary>
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
        }

        public override string Name { get; }

        public BotskoPlayerFirstTurnLogic FirstTurnLogic { get; set; }

        public BotskoPlayerSecondTurnLogic SecondTurnLogic { get; set; }

        public override PlayerAction GetTurn(PlayerTurnContext context)
        {
            Card cardToPlay = null;

            if (context.IsFirstPlayerTurn)
            {
                // Return Card To Play
                cardToPlay = this.FirstTurnLogic.Execute(context, this.PlayerActionValidator, this.Cards);
            }
            else
            {
                // Return Card To Play
                cardToPlay = this.SecondTurnLogic.Execute(context, this.PlayerActionValidator, this.Cards);
            }

            return this.PlayCard(cardToPlay);
        }
    }
}