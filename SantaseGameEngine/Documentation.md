**Data Structures and Algorithms Santase AI Teamwork**
------------------------------------------

Team "Botsko" - (Iwelina.Popova, INKolev, PawelAngelow)
-------------------------------------------------------


----------


Logic description
-----------------

(Decisions are ordered according to a priority level, starting from Highest to Lowest)
------------------------------------------------------------------------


**Logic for first turn when rules shouldn't be observed.**

1. Check for having 40 or 20 points. If yes => Announce.
2. Check if the highest trump that is still in the game is in my hand.
If yes => Check if (our points + the points from the highest trump) will win the round (score >= 66). **bool CanWinWithTrumpCard**(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay).
If yes => return the highest trump.
3. Check for having a sequence of winning trumps (i.e. Ace & Ten..) **bool HasSequenceOfWinningTrumps**(PlayerTurnContext context, ICollection<Card> possibleCardsToPlay).
4. If none of the previous steps are executed => find the smallest card different than trump and return it => **Card FindSmallestNonTrumpCard**(ICollection<Card> possibleCardsToPlay, CardSuit trumpSuit).
5. This is probably unnecessary check, but still - Check if smallest found card is Ace => If true => Play the lowest trump.


**Logic for first turn when rules should be observed** 

1. If there's only one card left in my hand => play it directly.
2. Check for having 40 or 20, which may be enough for scoring >= 66 points and win the round. If true => play it.
3. Get all the trumps in the hand.
----If no trumps => **pass the decision to point 4.**
----If only 1 trump in hand => check if it can win the round, and if yes => play it.
----If more than 2 trumps in hand => check if the highest of the trumps is the highest left in game. 
--------If yes => Check if it breaks 40 => 
------------If yes => play the Queen .
------------If no => return the highest trump.
--------If no => Check if it is part of 40 couple.
------------If yes => play it.
------------If no => pass the decision to point 4.
4. If none of the previous cases is executed, Check for having cards from a color different than the trump's one.
If yes =>
Call PlayNotTrumpCard(possibleCardsToPlay, playerAnnounce, trumpSuit). This method check every card if it is a winning one. Sorts the cards by strength and checks if the opponent has cards from the same color and if my cards are higher.
----If such card is not found => announce 20 (if possible). If not possible => play the lowest card.
5. If only trumps are left => play the weakest trump.

**Logic for second turn when rules shouldn't be observed.**

**1. Check if we can take the opponent's card with our highest from the same type.**
----If yes => 
--------Check if taking the hand will win the current round (score >= 66).
------------If yes = > return the highest card from the same type.
------------If no =>
----------------Check if we break 20 or 40 with that response card.
--------------------If yes => **pass the decision** to method 2.
--------------------If no => return the highest card from the same type.
----If this is not possible => **pass the decision** to the next method from the priority list.

**2. Check if the opponent's card is worth taking (Ten or Ace).**
----If yes =>
--------Check if you have a trump
------------If yes => 
----------------Check if your highest trump breaks 40.
--------------------If yes => 
------------------------Check if you have a trump that doesn't break 40.
----------------------------If yes=> return the selected optimal trump.
----------------------------If no => 
--------------------------------Check if taking the current hand by the opponent surpasses 66 points.
------------------------------------If yes => block him and return the highest trump.
------------------------------------If no => **pass the decision** to the next method from the priority list.
--------------------If no => take the opponent card with your highest trump.
------------If no => **pass the decision** to the next method from the priority list.
----If no =>
--------Check if the opponent's score will surpass 66 if he takes the current hand
------------If yes=> 
----------------Check if you have a trump
--------------------If yes => take the hand with your lowest trump.
--------------------If no => **pass the decision** to the next method from the priority list.
------------If no => 
----------------Check if you need to take the current hand in order to call 20 or 40 in the next turn.
--------------------If yes =>
------------------------Check if you have a trump
----------------------------If yes => take the hand with your lowest trump.
----------------------------If no => **pass the decision** to the next method from the priority list.
--------------------If no => **pass the decision** to the next method from the priority list.

**3. Find the lowest card from the color type that has the lowest count.**
----If this card breaks 20 => 
--------Find the lowest card from the second lowest count type and return it.
--------If such does not exist => return the lowest card from the previous step.


Logic for second turn when rules should be observed.



