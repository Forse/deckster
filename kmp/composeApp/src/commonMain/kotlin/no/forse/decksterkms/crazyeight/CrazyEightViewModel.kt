package no.forse.decksterkms.crazyeight

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewmodel.CreationExtras
import kotlinx.coroutines.launch
import no.forse.decksterlib.DecksterServer
import no.forse.decksterlib.crazyeights.CrazyEightsClient
import no.forse.decksterlib.model.common.Card
import no.forse.decksterlib.model.common.Suit
import threadpoolScope
import kotlin.reflect.KClass

class CrazyEightViewModel(
    private val gameId: String,
    server: DecksterServer,
) : ViewModel() {
    private val crazyEightsClient = CrazyEightsClient(server)

    // 1. create game in browser add a bot
    // 2. join game in the compose app
    // 3. start game in browser

    private fun List<Card>.getSuit(suit: Suit) = this.firstOrNull { it.suit == suit }
    private fun List<Card>.getRank(rank: Int) = this.firstOrNull { it.rank == rank }
    private fun List<Card>.findEight() = this.firstOrNull { it.rank == 8 }

    private fun determineCardToPut(topOfPile: Card, currentSuit: Suit, cards: List<Card>): Card? {
        return cards.getSuit(currentSuit)
            ?: cards.getRank(topOfPile.rank)
            ?: cards.findEight()
    }

    suspend fun initialize() {
        crazyEightsClient.joinGame(crazyEightsClient.decksterServer.accessToken!!, gameId)

        threadpoolScope.launch {
            crazyEightsClient.crazyEightsNotifications?.collect {
                println(it.type)
            }
        }

        threadpoolScope.launch {
            crazyEightsClient.yourTurnFlow.collect { playerView ->
                println("XXX my turn")

                val cardToPut = determineCardToPut(playerView.topOfPile, playerView.currentSuit, playerView.cards)
                if (cardToPut != null) {
                    if (cardToPut.rank == 8) {
                        determineSuiteToRequest(playerView.cards)?.let { suit ->
                            println("XXX put eight: $cardToPut")
                            crazyEightsClient.putEight(cardToPut, suit)
                        } ?: crazyEightsClient.passTurn()
                    } else {
                        println("XXX put card: $cardToPut")
                        crazyEightsClient.putCard(cardToPut)
                    }
                } else {
                    println("XXX will draw card")
                    val drawnCard = crazyEightsClient.drawCard()
                    println("XXX drawn Card $drawnCard")
                    if (drawnCard.card.suit == playerView.currentSuit || drawnCard.card.rank == playerView.topOfPile.rank) {
                        println("XXX will put $drawnCard")
                        crazyEightsClient.putCard(drawnCard.card)
                    } else {
                        println("XXX will pass turn")
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

    class Factory(private val gameId: String, private val server: DecksterServer) : ViewModelProvider.Factory {
        override fun <T : ViewModel> create(modelClass: KClass<T>, extras: CreationExtras): T {
            return CrazyEightViewModel(gameId, server) as T
        }
    }
}