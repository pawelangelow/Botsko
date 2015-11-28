﻿namespace Santase.AI.DummyPlayer
{
    using System.Linq;

    using Santase.Logic.Extensions;
    using Santase.Logic.Players;

    /// <summary>
    /// This dummy player follows the rules and always plays random card.
    /// Dummy never changes the trump or closes the game.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class DummyPlayer : BasePlayer
    {
        public DummyPlayer()
            : this("Dummy Player Lvl. 1")
        {
        }

        public DummyPlayer(string name)
        {
            this.Name = name;
        }

        public override string Name { get; }

        public override PlayerAction GetTurn(PlayerTurnContext context)
        {
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
