namespace Santase.AI.BotskoPlayer
{
    using System.Linq;
    using Logic.Cards;
    using Logic.Players;
    using Santase.Logic.Extensions;

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
            if (context.FirstPlayedCard == null)
            {
                cardToPlay = this.FirstTurnLogic.Execute(context);
            }
            else
            {
                cardToPlay = this.SecondTurnLogic.Execute(context);
            }

            return this.PlayCard(cardToPlay);
        }
    }
}