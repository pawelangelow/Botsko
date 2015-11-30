namespace Santase.AI.BotskoPlayer
{
    using System.Linq;

    using Logic;
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
                // Think about times when is better not to change the trump card despite it is possible
                if (this.PlayerActionValidator.IsValid(PlayerAction.ChangeTrump(), context, this.Cards))
                {
                    return this.ChangeTrump(context.TrumpCard);
                }

                if (this.FirstTurnLogic.IsGoodToClose(context))
                {
                    this.CloseGame();
                }

                // For now this will be here, but it's better to be in FirstTurnLogic
                // beacause if it's here, this check can be done only before Execute()
                // or after it, but not between the logic behind
                var announceCard = this.CallAnnounce(context);
                if (announceCard != null)
                {
                    return this.PlayCard(announceCard);
                }

                cardToPlay = this.FirstTurnLogic.Execute(context, this);
            }
            else
            {
                cardToPlay = this.SecondTurnLogic.Execute(context, this);
            }

            return this.PlayCard(cardToPlay);
        }

        public override void EndTurn(PlayerTurnContext context)
        {
            this.FirstTurnLogic.RegisterUsedCard(context.FirstPlayedCard);
            this.FirstTurnLogic.RegisterUsedCard(context.SecondPlayedCard);
            base.EndTurn(context);
        }

        private Card CallAnnounce(PlayerTurnContext context)
        {
            // First check for 40
            var possibleCards = this.PlayerActionValidator.GetPossibleCardsToPlay(context, this.Cards);
            var trumpSuit = context.TrumpCard.Suit;
            var announceCard = possibleCards
                .Where(c => c.Type == CardType.Queen &&
                        this.AnnounceValidator.GetPossibleAnnounce(this.Cards, c, context.TrumpCard) == Announce.Forty)
                .FirstOrDefault();

            if (announceCard != null)
            {
                return announceCard;
            }

            // Check for 20
            announceCard = possibleCards
                .Where(c => c.Type == CardType.Queen &&
                        this.AnnounceValidator.GetPossibleAnnounce(this.Cards, c, context.TrumpCard) == Announce.Twenty)
                .FirstOrDefault();

            if (announceCard != null)
            {
                return announceCard;
            }

            return null;
        }
    }
}