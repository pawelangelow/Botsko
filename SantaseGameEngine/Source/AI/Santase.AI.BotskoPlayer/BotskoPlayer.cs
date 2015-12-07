namespace Santase.AI.BotskoPlayer
{
    using System.Linq;

    using Logic;
    using Logic.Cards;
    using Logic.Players;
    using Santase.Logic.Extensions;
    using System.Collections.Generic;
    using System;

    // ReSharper disable once UnusedMember.Global
    public class BotskoPlayer : BasePlayer
    {
        private Card playerAnnounce;

        public BotskoPlayer()
            : this("Botsko Player")
        {
        }

        public BotskoPlayer(string name)
        {
            this.Name = name;
            this.PlayedCards = new bool[4, 6];
            this.FirstTurnLogic = new BotskoPlayerFirstTurnLogic(this.PlayerActionValidator, this.Cards, this.PlayedCards);
            this.SecondTurnLogic = new BotskoPlayerSecondTurnLogic(this.PlayerActionValidator, this.Cards, this.PlayedCards);
        }

        public static bool BotskoIsFirstPlayer { get; private set; }

        public override string Name { get; }

        public BotskoPlayerFirstTurnLogic FirstTurnLogic { get; set; }

        public BotskoPlayerSecondTurnLogic SecondTurnLogic { get; set; }

        public bool[,] PlayedCards { get; set; }

        public Card BotskoFirstPlayed { get; private set; }

        public override PlayerAction GetTurn(PlayerTurnContext context)
        {
            Card cardToPlay = null;
            this.playerAnnounce = this.CallAnnounce(context);

            if (context.FirstPlayedCard == null)
            {
                // TODO: Think about times when is better not to change the trump card despite it is possible
                if (this.PlayerActionValidator.IsValid(PlayerAction.ChangeTrump(), context, this.Cards))
                {
                    return this.ChangeTrump(context.TrumpCard);
                }

                if (this.FirstTurnLogic.IsGoodToClose(context))
                {
                    this.CloseGame();
                }

                if (context.State.ShouldObserveRules)
                {
                    cardToPlay = this.PlayWhenFirstAndObserveRules(context, this.playerAnnounce);
                }

                // Remove if-statement and left only the logic in it.
                if (!context.State.ShouldObserveRules)
                {
                    cardToPlay = this.FirstTurnLogic.PlayWhenRulesDoNotApply(
                        context,
                        this.PlayerActionValidator.GetPossibleCardsToPlay(context, this.Cards),
                        playerAnnounce);
                }

                // In worst case the logic above do not find card to play
                if (cardToPlay == null)
                {
                    cardToPlay = this.FirstTurnLogic.Execute(context, this, playerAnnounce);
                }
            }
            else
            {
                if (context.State.ShouldObserveRules)
                {
                    cardToPlay = this.PlayWhenSecondAndObserveRules(context);
                }
                else
                {
                    cardToPlay = this.PlayWhenSecondAndRulesNotAply(context);
                    //cardToPlay = this.SecondTurnLogic.Execute(context, this, playerAnnounce);
                }
            }

            if (this.BotskoFirstPlayed == null)
            {
                this.BotskoFirstPlayed = cardToPlay;
            }

            return this.PlayCard(cardToPlay);
        }

        public override void EndTurn(PlayerTurnContext context)
        {
            if (this.BotskoFirstPlayed == context.FirstPlayedCard)
            {
                BotskoIsFirstPlayer = true;
            }

            if (this.BotskoFirstPlayed == context.SecondPlayedCard)
            {
                BotskoIsFirstPlayer = false;
            }

            this.FirstTurnLogic.RegisterUsedCard(context.FirstPlayedCard);
            this.FirstTurnLogic.RegisterUsedCard(context.SecondPlayedCard);

            base.EndTurn(context);
        }

        public override void EndRound()
        {
            this.ClearPlayedCards();
            this.BotskoFirstPlayed = null;
            base.EndRound();
        }

        #region Play first

        private Card PlayWhenFirstAndObserveRules(PlayerTurnContext context, Card playerAnnounce)
        {
            var possibleCardsToPlay = this.PlayerActionValidator.GetPossibleCardsToPlay(context, this.Cards);
            var trumpSuit = context.TrumpCard.Suit;

            // 1. Check if this is the last card.
            if (possibleCardsToPlay.Count == 1)
            {
                return possibleCardsToPlay.FirstOrDefault();
            }

            // 2. Check if there is 40/20 and points are 26/46 ot more.
            if (playerAnnounce != null)
            {
                int botskoPoints = BotskoPlayer.BotskoIsFirstPlayer ?
                    context.FirstPlayerRoundPoints : context.SecondPlayerRoundPoints;

                if ((playerAnnounce.Suit == trumpSuit && botskoPoints >= 26) ||
                    (playerAnnounce.Suit != trumpSuit && botskoPoints >= 46))
                {
                    return playerAnnounce;
                }
            }

            var trumpCards = this.FirstTurnLogic.FindTrumpCardsInHand(possibleCardsToPlay, trumpSuit);
            var trumpCardsCount = trumpCards.Count();
            if (trumpCardsCount == 0)
            {
                return this.PlayNotTrumpCard(possibleCardsToPlay, playerAnnounce, trumpSuit);
            }

            var biggestTrumpInHand = trumpCards.FirstOrDefault();

            if (trumpCardsCount == 1)
            {
                int botskoPoints = BotskoPlayer.BotskoIsFirstPlayer ?
                    context.FirstPlayerRoundPoints : context.SecondPlayerRoundPoints;

                if (this.FirstTurnLogic.IsBiggestCardInMyHand(biggestTrumpInHand) &&
                    (botskoPoints + biggestTrumpInHand.GetValue()) >= 66)
                {
                    return biggestTrumpInHand;
                }
            }

            // Check if the biggest trump in hand is winning card
            if (trumpCardsCount > 1)
            {
                if (this.FirstTurnLogic.IsBiggestCardInMyHand(biggestTrumpInHand))
                {
                    // Check if the biggest trump in hand is a King and have 40
                    if (playerAnnounce != null &&
                        playerAnnounce.Suit == trumpSuit &&
                        biggestTrumpInHand.Type == CardType.King)
                    {
                        return playerAnnounce;
                    }

                    return biggestTrumpInHand;
                }
                else if (playerAnnounce != null &&
                        playerAnnounce.Suit == trumpSuit)
                {
                    return playerAnnounce;
                }
            }

            if (trumpCardsCount != possibleCardsToPlay.Count)
            {
                return this.PlayNotTrumpCard(possibleCardsToPlay, playerAnnounce, trumpSuit);
            }

            if (trumpCardsCount == possibleCardsToPlay.Count)
            {
                return trumpCards.Last();
            }

            // Never goes here I hope
            return this.FirstTurnLogic.Execute(context, this, playerAnnounce);
        }

        private Card PlayNotTrumpCard(ICollection<Card> possibleCardsToPlay, Card playerAnnounce, CardSuit trumpSuit)
        {
            // 1. Check for winning not trump card
            var winningNotTrumpCard = this.FirstTurnLogic.HasWinningNotTrumpCard(possibleCardsToPlay, trumpSuit);
            if (winningNotTrumpCard != null)
            {
                return winningNotTrumpCard;
            }

            // 2. Call 20
            if (playerAnnounce != null &&
                playerAnnounce.Suit != trumpSuit)
            {
                return playerAnnounce;
            }

            // 3. Return the smallest card on the hand
            return this.FirstTurnLogic.FindSmallestNotTrumpCard(possibleCardsToPlay, trumpSuit);
        }

        private Card CallAnnounce(PlayerTurnContext context)
        {
            // 1. Check for 40.
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

            // 2. Check for 20.
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

        #endregion

        #region Play Second When Rules Apply
        private Card PlayWhenSecondAndObserveRules(PlayerTurnContext context)
        {
            var opponentCard = context.FirstPlayedCard;
            var possibleCardsToPlay = this.PlayerActionValidator.GetPossibleCardsToPlay(context, this.Cards);

            // 1. Check for only one possible card to be played.
            if (possibleCardsToPlay.Count == 1)
            {
                return possibleCardsToPlay.First();
            }

            // 2. When have card/s from same suit
            if (possibleCardsToPlay.FirstOrDefault().Suit == opponentCard.Suit)
            {
                return this.PlayCardFromSameSuit(possibleCardsToPlay, opponentCard);
            }

            // 3. When haven't got card/s from same suit but have trump card/s
            if (possibleCardsToPlay.FirstOrDefault().Suit == context.TrumpCard.Suit)
            {
                return this.PlayTrumpCardAgainstCardFromSuit(context, possibleCardsToPlay, opponentCard);
            }

            // 4. When haven't got card/s from same suit and haven't got trump card/s
            // play smallest card in hand.
            if (possibleCardsToPlay.FirstOrDefault().Suit != opponentCard.Suit &&
                possibleCardsToPlay.FirstOrDefault().Suit != context.TrumpCard.Suit)
            {
                return this.PlayDifferentSuitAndNotTrumpCard(possibleCardsToPlay, context.TrumpCard.Suit);
            }

            // TODO: Remove ??
            return possibleCardsToPlay.FirstOrDefault();
        }

        /// <summary>
        /// Choose card when rules apply and have card/s from the same suit
        /// like the card that opponent have played.
        /// </summary>
        /// <param name="possibleCardsToPlay">Cards that player can play.</param>
        /// <param name="opponentCard">Card that opponent have played.</param>
        /// <returns>Returns the optimal card to be played.</returns>
        private Card PlayCardFromSameSuit(ICollection<Card> possibleCardsToPlay, Card opponentCard)
        {
            var opponentCardValue = opponentCard.GetValue();

            var biggerCardsThanOpponent = possibleCardsToPlay
                .Where(c => c.GetValue() > opponentCardValue)
                .OrderByDescending(c => c.GetValue())
                .ToList();
            var biggerCardsCount = biggerCardsThanOpponent.Count;

            // Check if haven't got bigger card/s than opponent and return the smallest one.
            if (biggerCardsCount == 0)
            {
                return this.FindSmallestCardFromGivenSuit(possibleCardsToPlay, opponentCard.Suit);
            }

            // If have only one bigger card returns it.
            // This check can be remove but I let it here for sure.
            if (biggerCardsCount == 1)
            {
                return biggerCardsThanOpponent.First();
            }

            if (biggerCardsCount > 1)
            {
                // If have more than 1 bigger cards but the opponent have only one card
                // from this suit, play the biggest one.
                if (this.CardsFromGivenSuit(opponentCard.Suit) - this.MyCardsFromGivenSuit(opponentCard.Suit) == 1)
                {
                    return biggerCardsThanOpponent.First();
                }

                // If have more than 1 bigger cards and the opponent should have more than
                // one card from this suit play the smallest bigger.
                if (biggerCardsCount == 2)
                {
                    return biggerCardsThanOpponent.Last();
                }

                return biggerCardsThanOpponent.First();
            }

            // Should never goes here.
            return possibleCardsToPlay.First();
        }

        /// <summary>
        /// Find best trump card when do not have cards from the opponent card suit.
        /// </summary>
        /// <param name="context">The information about current turn.</param>
        /// <param name="possibleCardsToPlay">Cards that player can play.</param>
        /// <param name="opponentCard">Card that opponent have played.</param>
        /// <returns>Find the optimal trump card to be played.</returns>
        private Card PlayTrumpCardAgainstCardFromSuit(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay, Card opponentCard)
        {
            var trumpCards = possibleCardsToPlay
                .OrderByDescending(c => c.GetValue())
                .ToList();

            int botskoPoints = BotskoPlayer.BotskoIsFirstPlayer ?
                context.FirstPlayerRoundPoints : context.SecondPlayerRoundPoints;
            var biggestTrumpInHand = trumpCards.FirstOrDefault();
            var smallestTrumpInHand = trumpCards.Last();

            // Check if can win with opponent card points and biggest trump card points,
            if (botskoPoints + biggestTrumpInHand.GetValue() + opponentCard.GetValue() >= 66)
            {
                return trumpCards.FirstOrDefault();
            }

            if (trumpCards.Count > 1)
            {
                if (this.playerAnnounce != null &&
                    this.playerAnnounce.Suit == biggestTrumpInHand.Suit)
                {
                    // If have bigger trump card than King and smaller than Queen
                    // play smaller trump card.
                    if (biggestTrumpInHand.Type != CardType.King &&
                        smallestTrumpInHand.Type != CardType.Queen)
                    {
                        return smallestTrumpInHand;
                    }

                    // If have bigger trump card than King but the smallest is Queen
                    // play bigger trump card.
                    if (biggestTrumpInHand.Type != CardType.King &&
                        smallestTrumpInHand.Type == CardType.Queen)
                    {
                        return biggestTrumpInHand;
                    }

                    // If have bigger trump card than Kign but have 40 too
                    // play bigger trump card.
                    if (biggestTrumpInHand.Type == CardType.King)
                    {
                        return smallestTrumpInHand;
                    }
                }

                // Check if have more than 1 trump cards and the biggest in the game is in my hand.
                // Then play the smallest trump card.
                if (this.FirstTurnLogic.IsBiggestCardInMyHand(biggestTrumpInHand))
                {
                    return smallestTrumpInHand;
                }
            }

            return trumpCards.Last();
        }

        /// <summary>
        /// Search in the cards for the smallest card to play.
        /// If the smallest card is Queen or King first check for possible 20.
        /// </summary>
        /// <param name="possibleCardsToPlay">Cards that player can play.</param>
        /// <param name="trumpSuit">Trump card suit.</param>
        /// <returns>Return the smallest founded card.</returns>
        private Card PlayDifferentSuitAndNotTrumpCard(ICollection<Card> possibleCardsToPlay, CardSuit trumpSuit)
        {
            return this.FirstTurnLogic.FindSmallestNotTrumpCard(possibleCardsToPlay, trumpSuit);
        }

        private Card FindSmallestCardFromGivenSuit(ICollection<Card> possibleCardsToPlay, CardSuit suit)
        {
            return possibleCardsToPlay
                .Where(c => c.Suit == suit)
                .OrderBy(c => c.GetValue())
                .FirstOrDefault();
        }

        private int MyCardsFromGivenSuit(CardSuit suit)
        {
            return this.Cards
                .Where(c => c.Suit == suit)
                .Count();
        }

        private int CardsFromGivenSuit(CardSuit suit)
        {
            var suitCoor = (int)suit;
            var count = 0;

            for (int type = 0; type <= 5; type++)
            {
                if (this.PlayedCards[suitCoor, type] == false)
                {
                    count++;
                }
            }

            return count;
        }

        #endregion

        private Card PlayWhenSecondAndRulesNotAply(PlayerTurnContext context)
        {
            var opponentCard = context.FirstPlayedCard;
            var possibleCardsToPlay = this.PlayerActionValidator.GetPossibleCardsToPlay(context, this.Cards);

            // 1. Check for card which ends the game
            var winningCard = this.HasWinningTrumpCard(context, possibleCardsToPlay, opponentCard);
            if (winningCard != null)
            {
                return winningCard;
            }

            // 2. Check for bigger card in same suit different than trump suit and 20
            if (opponentCard.Suit != context.TrumpCard.Suit)
            {
                var biggerInSameSuit = this.HasBiggerCardInSameSuit(possibleCardsToPlay, opponentCard);
                if (biggerInSameSuit != null)
                {
                    if (this.playerAnnounce == null)
                    {
                        return biggerInSameSuit;
                    }

                    if (this.playerAnnounce != null &&
                        this.playerAnnounce.Suit != biggerInSameSuit.Suit)
                    {
                        return biggerInSameSuit;
                    }

                    if (this.playerAnnounce != null &&
                        this.playerAnnounce.Suit == biggerInSameSuit.Suit &&
                        biggerInSameSuit.Type != CardType.King)
                    {
                        return biggerInSameSuit;
                    }
                }
            }

            // 3. Check if opponent plays big value card
            if (opponentCard.GetValue() >= 10)
            {
                var myCardInThisSituation = this.CardToBeatHis(opponentCard, context, possibleCardsToPlay);
                if (myCardInThisSituation != null)
                {
                    return myCardInThisSituation;
                }
            }

            // 4. If opponent has many points or announcement of 40, get as much as you can
            var opponentPoints = BotskoPlayer.BotskoIsFirstPlayer ?
                context.SecondPlayerRoundPoints : context.FirstPlayerRoundPoints;
            var botskoPoints = BotskoPlayer.BotskoIsFirstPlayer ?
                context.FirstPlayerRoundPoints : context.SecondPlayerRoundPoints;
            if ((opponentPoints > 45 && botskoPoints < 30) ||
                context.FirstPlayerAnnounce == Announce.Forty)
            {
                var cardToPlayToSave = this.CardToSave(opponentCard, context, possibleCardsToPlay);
                if (cardToPlayToSave != null)
                {
                    return cardToPlayToSave;
                }
            }

            // Check for good last card
            if (context.TrumpCard.GetValue() > 2 && context.CardsLeftInDeck == 2)
            {
                var cardToPlay = this.CardToGetTheLastOfDeck(opponentCard, context, possibleCardsToPlay);
                if (cardToPlay != null)
                {
                    return cardToPlay;
                }
            }

            // Default
            return possibleCardsToPlay
                .Where(c => c.Suit != context.TrumpCard.Suit)
                .OrderBy(c => c.GetValue())
                .First();
        }

        private Card HasBiggerCardInSameSuit(ICollection<Card> possibleCardsToPlay, Card opponentCard)
        {
            var biggerCards = possibleCardsToPlay
                .Where(c => c.Suit == opponentCard.Suit &&
                            c.GetValue() > opponentCard.GetValue())
                .OrderByDescending(c => c.GetValue())
                .ToList();

            if (biggerCards != null)
            {
                return biggerCards.FirstOrDefault();
            }

            return null;
        }

        private Card HasWinningTrumpCard(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay, Card opponentCard)
        {
            var trumpSuit = context.TrumpCard.Suit;
            var trumps = possibleCardsToPlay
                .Where(c => c.Suit == trumpSuit)
                .OrderByDescending(c => c.GetValue())
                .ToList();

            if (trumps.Count() == 0)
            {
                return null;
            }

            var biggestTrumpInHand = trumps.FirstOrDefault();
            int botskoPoints = BotskoPlayer.BotskoIsFirstPlayer ?
                    context.FirstPlayerRoundPoints : context.SecondPlayerRoundPoints;

            //// Case 1: we have trump and playing it will end the game
            if (biggestTrumpInHand.GetValue() + botskoPoints + opponentCard.GetValue() >= 66)
            {
                return biggestTrumpInHand;
            }

            //// Case 2: we have 2 trumps and playing them will end the game
            if (trumps.Count() >= 2)
            {
                var trumpsSum = trumps[0].GetValue() + trumps[1].GetValue();
                if (trumpsSum + botskoPoints + opponentCard.GetValue() >= 66)
                {
                    return biggestTrumpInHand;
                }
            }

            //// Case 3: we have 40 and something else, playing something else to call 40 on next round
            if (this.playerAnnounce != null &&
                this.playerAnnounce.Suit == trumpSuit &&
                trumps.Count >= 3)
            {
                return trumps
                    .Where(c => c.Type != CardType.King && c.Type != CardType.Queen)
                    .OrderBy(c => c.GetValue())
                    .First();
            }

            return null;
        }

        private Card CardToBeatHis(Card opponentCard, PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
        {
            if (opponentCard.Suit == context.TrumpCard.Suit)
            {
                return null; // There is no point
            }

            var myCard = possibleCardsToPlay
                .Where(c =>
                c.GetValue() > opponentCard.GetValue() &&
                c.Suit == opponentCard.Suit)
                .FirstOrDefault();

            if (myCard != null)
            {
                return myCard; // We have ace to beat his 10
            }

            var trumpCards = possibleCardsToPlay
                .Where(c => c.Suit == context.TrumpCard.Suit)
                .OrderBy(c => c.GetValue())
                .ToList();
            var trumpCard = trumpCards
                .FirstOrDefault();

            if (trumpCard != null) // TODO: Optimize it
            {
                // In case we have 9 and is good to change
                if (trumpCard.Type == CardType.Nine &&
                    this.IsGoodToChange(context) &&
                    (trumpCards.Last().Type != CardType.Queen && trumpCards.Last().Type != CardType.King))
                {
                    return trumpCards.Last();
                }

                if (this.FirstTurnLogic.CheckForPossible20or40(trumpCard))
                {
                    return trumpCard; // Booo, get this damage!
                }
            }

            return null;
        }

        private bool IsGoodToChange(PlayerTurnContext context)
        {
            if (context.TrumpCard.GetValue() > 2)
            {
                return true;
            }

            return false;
        }

        private Card CardToSave(Card opponentCard, PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
        {
            Card cardToPlay = possibleCardsToPlay
                    .Where(c => c.Suit == opponentCard.Suit && c.GetValue() > opponentCard.GetValue())
                    .OrderByDescending(c => c.GetValue())
                    .FirstOrDefault();

            if (opponentCard.Suit == context.TrumpCard.Suit)
            {
                return cardToPlay;
            }
            else
            {
                if (cardToPlay == null)
                {
                    cardToPlay = possibleCardsToPlay
                        .Where(c => c.Suit == context.TrumpCard.Suit)
                        .OrderByDescending(c => c.GetValue()) //Maybe check for 40
                        .FirstOrDefault();
                }
            }

            return cardToPlay; // Plays he highest card
        }

        private Card CardToGetTheLastOfDeck(Card opponentCard, PlayerTurnContext context, ICollection<Card> possibleCardsToPlay)
        {
            Card cardToReturn = null;

            if (opponentCard.GetValue() <= 4)
            {
                cardToReturn = possibleCardsToPlay
                    .OrderBy(c => c.GetValue())
                    .FirstOrDefault();
            }
            else
            {
                var countOf40Cards = possibleCardsToPlay
                    .Where(c => c.Suit == context.TrumpCard.Suit &&
                    (c.Type == CardType.King || c.Type == CardType.Queen))
                    .Count();

                if (countOf40Cards == 1 &&
                    (context.TrumpCard.Type == CardType.King || context.TrumpCard.Type == CardType.Queen))
                {
                    cardToReturn = possibleCardsToPlay
                    .OrderBy(c => c.GetValue())
                    .FirstOrDefault(); // Maybe get his card in other cases
                }
            }

            return cardToReturn;
        }

        private void ClearPlayedCards()
        {
            for (int i = 0; i < this.PlayedCards.GetLength(0); i++)
            {
                for (int j = 0; j < this.PlayedCards.GetLength(1); j++)
                {
                    this.PlayedCards[i, j] = false;
                }
            }
        }
    }
}
