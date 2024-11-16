package no.forse.decksterkms.crazyeight

import kotlinx.coroutines.launch
import no.forse.decksterlib.crazyeights.CrazyEightsClient
import no.forse.decksterlib.model.common.Card
import no.forse.decksterlib.model.common.Suit
import no.forse.decksterlib.model.crazyeights.PlayerViewOfGame
import threadpoolScope

class CrazyEightViewModel(private val crazyEightsClient: CrazyEightsClient){

    // 1. create game in browser add a bot
    // 2. join game in the compose app
    // 3. start game in browser


    private companion object {
        const val GAME_ID = "vanedannende stykke"
    }

    private fun List<Card>.getSuit(suit: Suit) = this.firstOrNull { it.suit == suit }
    private fun List<Card>.getRank(rank: Int) = this.firstOrNull { it.rank == rank }
    private fun List<Card>.findEight() = this.firstOrNull { it.rank == 8 }

    private fun determineCardToPut(topOfPile: Card, cards: List<Card>) {
        cards.getSuit(topOfPile.suit)
            ?: cards.getRank(topOfPile.rank)
            ?: cards.findEight()


    }

    suspend fun initialize() {
        crazyEightsClient.joinGame(crazyEightsClient.decksterServer.accessToken!!, GAME_ID)

        threadpoolScope.launch {
            crazyEightsClient.yourTurnFlow.collect { threeViewsOfAPlayer ->
                println("my turn")
                //getBotAction(threeViewsOfAPlayer.topOfPile, threeViewsOfAPlayer.cards)


            }
        }
        //println("pre start game")
        //val foo = crazyEightsClient.startGame(GAME_ID)
        //println("post start game: $foo")

    }
}