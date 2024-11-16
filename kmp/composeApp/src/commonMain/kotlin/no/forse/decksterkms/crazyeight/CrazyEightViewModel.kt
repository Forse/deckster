package no.forse.decksterkms.crazyeight

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewmodel.CreationExtras
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.*
import kotlinx.coroutines.launch
import no.forse.decksterlib.DecksterServer
import no.forse.decksterlib.crazyeights.CrazyEightsClient
import no.forse.decksterlib.model.common.Card
import no.forse.decksterlib.model.common.Suit
import no.forse.decksterlib.model.crazyeights.*
import threadpoolScope
import kotlin.reflect.KClass

class CrazyEightViewModel(
    val gameId: String,
    val asbot: Boolean,
    val spectateMode: Boolean,
    server: DecksterServer,
) : ViewModel() {
    private val crazyEightsClient = CrazyEightsClient(server)

    private val _uiState = MutableStateFlow(
        CrazyEightUiState(
            emptyList(),
            Card(1, Suit.Hearts),
            isYourTurn = false,
            topCardIsYours = false,
        )
    )
    val uiState: StateFlow<CrazyEightUiState> = _uiState.asStateFlow()

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
        crazyEightsClient.prepareLoggedInGamme(crazyEightsClient.decksterServer.accessToken!!)
        if (spectateMode) {
            crazyEightsClient.spectate(gameId)

        } else {
            crazyEightsClient.join(gameId)
        }

        threadpoolScope.launch {
            crazyEightsClient.crazyEightsNotifications?.collect {
                when (it) {
                    is PlayerPutCardNotification, is PlayerPutEightNotification -> _uiState.update {
                        it.copy(
                            crazyEightsClient.currentState!!.cards,
                            crazyEightsClient.currentState!!.topOfPile,
                            topCardIsYours = false,
                        )
                    }
                }
            }
        }

        threadpoolScope.launch {
            crazyEightsClient.yourTurnFlow.collect { playerView ->
                onYourTurn(playerView)
            }
        }
    }

    private suspend fun onYourTurn(playerView: PlayerViewOfGame) {
        println("XXX my turn")
        _uiState.update {
            it.copy(
                playerHand = playerView.cards,
                topOfPile = playerView.topOfPile,
                topCardIsYours = false,
                isYourTurn = true,
            )
        }
        if (asbot) {
            yourTurnAsBot(playerView)
        } else {
            // todo logic to process card selection from player
        }
    }

    private suspend fun yourTurnAsBot(playerView: PlayerViewOfGame) {
        delay(1000)

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
            val drawnCard = drawCard(3, playerView)
            println("XXX drawn Card $drawnCard")
            if (drawnCard != null) {
                println("XXX will put $drawnCard")
                crazyEightsClient.putCard(drawnCard)
            } else {
                println("XXX will pass turn")
                crazyEightsClient.passTurn()
            }
        }

        // new state, while waiting for
        _uiState.update { it.copy(
                playerView.cards,
                playerView.topOfPile,
                topCardIsYours = false,
                isYourTurn = false,
            )
        }
        delay(300) // Wait a bit in case the other player is a bot as well - things will happen fast
    }

    private suspend fun drawCard(numberOfCardsToDraw: Int, playerView: PlayerViewOfGame): Card? {
        if (numberOfCardsToDraw == 0) {
            return null
        }

        val drawnCard = crazyEightsClient.drawCard()
        return if (drawnCard.card.suit == playerView.currentSuit || drawnCard.card.rank == playerView.topOfPile.rank) {
            drawnCard.card
        } else {
            drawCard(numberOfCardsToDraw - 1, playerView)
        }
    }

    private fun determineSuiteToRequest(cards: List<Card>) = Suit.entries.map { suit ->
        val count = cards.filter { it.suit == suit }.size
        Pair(suit, count)
    }.maxByOrNull { it.second }?.first

    fun leave() {
        crazyEightsClient.leaveGame()
    }

    class Factory(private val gameId: String,
                  private val asBot: Boolean,
                  private val spectateMode: Boolean,
                  private val server: DecksterServer) :
        ViewModelProvider.Factory {
        override fun <T : ViewModel> create(modelClass: KClass<T>, extras: CreationExtras): T {
            return CrazyEightViewModel(gameId, asBot, spectateMode, server) as T
        }
    }
}