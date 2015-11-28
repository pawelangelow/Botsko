namespace Santase.AI.BotskoPlayer
{
    using System.Linq;

    using Santase.Logic.Extensions;
    using Santase.Logic.Players;

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

        public override PlayerAction GetTurn(PlayerTurnContext context)
        {
            if (context.IsFirstPlayerTurn)
            {
                // Az igraq purvi
                // Tuk ne pipa Ivan, shtoto bolqt zubi.
            }
            else
            {
                // Az igraq vtori 
                // Tuk pipa Ivan, shtoto si misli che znae kvo pravi.
            }

            // Gets the cards in our hand
            var possibleCardsToPlay = this.PlayerActionValidator.GetPossibleCardsToPlay(context, this.Cards);
            var shuffledCards = possibleCardsToPlay.Shuffle();
            var cardToPlay = shuffledCards.First();

            // SecondPlayer == Opponent
            var opponentCardType = context.SecondPlayedCard.Type;
            var opponentCardSuit = context.SecondPlayedCard.Suit;

            return this.PlayCard(cardToPlay);
        }
    }
}