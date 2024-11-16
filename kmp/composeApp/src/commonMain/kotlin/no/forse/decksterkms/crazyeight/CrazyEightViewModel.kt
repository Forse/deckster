package no.forse.decksterkms.crazyeight

import androidx.lifecycle.*
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
            error = null,
            currentSuit = Suit.Hearts,
            gameState = GameState.WAITING,
            doSuitQuestion = false,
        )
    )
    val uiState: StateFlow<CrazyEightUiState> = _uiState.asStateFlow()

    // 1. create game in browser add a bot
    // 2. join game in the compose app
    // 3. start game in browser

    private fun List<Card>.getSuit(suit: Suit) = this.firstOrNull { it.suit == suit }
    private fun List<Card>.getRank(rank: Int) = this.firstOrNull { it.rank == rank }
    private fun List<Card>.findEight() = this.firstOrNull { it.rank == 8 }

    private var eightSelectedForPlay: Card? = null
    private var lockCardPlay = false

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
            crazyEightsClient.crazyEightsNotifications?.collect { event ->
                when (event) {
                    is PlayerPutCardNotification, is PlayerPutEightNotification -> _uiState.update {
                        it.copy(
                            crazyEightsClient.currentState!!.cards,
                            crazyEightsClient.currentState!!.topOfPile,
                            currentSuit = crazyEightsClient.currentState!!.currentSuit,
                            topCardIsYours = false,
                        )
                    }
                    is GameEndedNotification -> _uiState.update {
                        it.copy(
                            gameState = GameState.ENDED,
                            loseName = event.loserName
                        )
                    }
                    is GameStartedNotification -> _uiState.update {
                        it.copy(
                            gameState = GameState.STARTED,
                            playerHand = event.playerViewOfGame.cards,
                            currentSuit = event.playerViewOfGame.currentSuit
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
        _uiState.update {
            it.copy(
                playerHand = playerView.cards,
                topOfPile = playerView.topOfPile,
                topCardIsYours = false,
                currentSuit = playerView.currentSuit,
                error = null,
                isYourTurn = true,
            )
        }
        if (asbot) {
            yourTurnAsBot(playerView)
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

        val drawResponse = crazyEightsClient.drawCard()
        _uiState.update {
            it.copy(playerHand = drawResponse.playerViewOfGame.cards)
        }
        delay(200) // Take 200 ms to decide after having drawn card and allow the UI to update
        val drawnCard = drawResponse.drawnCard
        return if (drawnCard.suit == playerView.currentSuit || drawnCard.rank == playerView.topOfPile.rank) {
            drawnCard
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

    fun onCardClicked(card: Card) {
        _uiState.update {
            it.copy(error = null)
        }

        if (asbot) return
        if (!_uiState.value.isYourTurn) {
            println("Attempted to play card while not your turn")
            return
        }
        if (!_uiState.value.canPlayCard) return

        if (!lockCardPlay) {
            lockCardPlay = true
            tryUserAction {
                playCard(card)
                lockCardPlay = false
            }
        }
    }

    suspend fun playCard(card: Card) {
        if (card.rank == 8) {
            println("Asking for player to selecte suite for eight...")
            eightSelectedForPlay = card
            _uiState.update {
                it.copy(doSuitQuestion = true)
            }
        } else  {
            println("Player putting card: $card")
            crazyEightsClient.putCard(card)
           // _uiState.update {  it.copy(currentSuit = card.suit) }
        }
    }

    fun onSuitSelected(suit: Suit) {
        tryUserAction {
            println("Playing Eight with suit: $suit, card: $eightSelectedForPlay")
            _uiState.update {
                it.copy(
                    error = null,
                    doSuitQuestion = false,
                    currentSuit = suit,
                )
            }
            crazyEightsClient.putEight(eightSelectedForPlay!!, suit)
            eightSelectedForPlay = null
        }
    }

    fun onDrawCard() {
        tryUserAction {
            val newState = crazyEightsClient.drawCard()
            _uiState.update { it.copy(playerHand = newState.playerViewOfGame.cards) }
        }
    }

    fun onPass() {
        tryUserAction {
            crazyEightsClient.passTurn()
            _uiState.update { it.copy(isYourTurn = false) }
        }
    }

    private fun tryUserAction( action: suspend () -> Unit) {
        viewModelScope.launch {
            try {
                lockCardPlay = true
                action.invoke()
            } catch (ex: Exception) {
                _uiState.update {
                    it.copy(error = ex.message)
                }
            } finally {
                lockCardPlay = false
            }
        }
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