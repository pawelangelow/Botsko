namespace Santase.AI.BotskoPlayer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Logic.Cards;
    using Logic.PlayerActionValidate;
    using Logic.Players;

    public class BotskoPlayerSecondTurnLogic : BotskoPlayerCommonLogic
    {
        public override Card Execute(PlayerTurnContext context, IPlayerActionValidator playerActionValidator, ICollection<Card> cards)
        {
            return base.Execute(context, playerActionValidator, cards);
        }
    }
}