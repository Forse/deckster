package no.forse.decksterkms.crazyeight

import kotlinx.coroutines.launch
import no.forse.decksterlib.crazyeights.CrazyEightsClient
import no.forse.decksterlib.model.common.Card
import no.forse.decksterlib.model.common.Suit
import threadpoolScope

class CrazyEightViewModel(private val crazyEightsClient: CrazyEightsClient){

    // 1. create game in browser add a bot
    // 2. join game in the compose app
    // 3. start game in browser


    private companion object {
        const val GAME_ID = "ensom eske"
    }

    private fun List<Card>.getSuit(suit: Suit) = this.firstOrNull { it.suit == suit }
    private fun List<Card>.getRank(rank: Int) = this.firstOrNull { it.rank == rank }
    private fun List<Card>.findEight() = this.firstOrNull { it.rank == 8 }

    private fun determineCardToPut(topOfPile: Card, cards: List<Card>): Card? {
        return cards.getSuit(topOfPile.suit)
            ?: cards.getRank(topOfPile.rank)
            ?: cards.findEight()
    }

    suspend fun initialize() {
        crazyEightsClient.joinGame(crazyEightsClient.decksterServer.accessToken!!, GAME_ID)

        threadpoolScope.launch {
            crazyEightsClient.crazyEightsNotifications?.collect {
                println(it.type)
            }
        }

        threadpoolScope.launch {
            crazyEightsClient.yourTurnFlow.collect { playerView ->
                println("my turn")

                val cardToPut = determineCardToPut(playerView.topOfPile, playerView.cards)
                if (cardToPut != null) {
                    if (cardToPut.rank == 8) {
                        determineSuiteToRequest(playerView.cards)?.let { suit ->
                            crazyEightsClient.putEight(cardToPut, suit)
                        } ?: crazyEightsClient.passTurn()
                    } else {
                        crazyEightsClient.putCard(cardToPut)
                    }
                } else {
                    val drawnCard = crazyEightsClient.drawCard()
                    if (drawnCard.card.suit == playerView.currentSuit || drawnCard.card.rank == playerView.topOfPile.rank) {
                        crazyEightsClient.putCard(drawnCard.card)
                    } else {
                        crazyEightsClient.passTurn()
                    }
                }
            }
        }
    }

    private fun determineSuiteToRequest(cards: List<Card>) = Suit.entries.map { suit ->
        val count = cards.filter { it.suit == suit }.size
        Pair(suit, count)
    }.maxByOrNull { it.second }?.first
}