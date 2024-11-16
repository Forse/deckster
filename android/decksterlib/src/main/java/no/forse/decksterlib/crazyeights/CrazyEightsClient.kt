package no.forse.decksterlib.crazyeights

import kotlinx.coroutines.CompletableDeferred
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.launch
import no.forse.decksterlib.DecksterServer
import no.forse.decksterlib.communication.ConnectedDecksterGame
import no.forse.decksterlib.communication.DecksterGameInitiater
import no.forse.decksterlib.game.GameClientBase
import no.forse.decksterlib.model.common.Card
import no.forse.decksterlib.model.common.EmptyResponse
import no.forse.decksterlib.model.common.Suit
import no.forse.decksterlib.model.crazyeights.*
import no.forse.decksterlib.model.protocol.DecksterNotification
import no.forse.decksterlib.protocol.dtoType
import no.forse.decksterlib.protocol.getType
import threadpoolScope

/**
 * CrazyEights game.
 * Spec:
 * http://localhost:13992/crazyeights/metadata
 * http://localhost:13992/swagger/index.html
 */
class CrazyEightsClient(decksterServer: DecksterServer) :
    GameClientBase(decksterServer, "crazyeights") {

    override suspend fun onNotificationArrived(notif: DecksterNotification) {
        println("CrazyEightsClient ${joinedGame?.userUuid} onMessageArrived: $notif")
    }

    var currentState: PlayerViewOfGame? = null
        private set

    var gameStarted = CompletableDeferred<PlayerViewOfGame>()
        private set

    val yourTurnFlow: MutableSharedFlow<PlayerViewOfGame> = MutableSharedFlow(replay = 0, extraBufferCapacity = 1)

    private fun onGameStarted(notif: GameStartedNotification) {
        currentState = notif.playerViewOfGame
        gameStarted.complete(notif.playerViewOfGame)
    }

    val crazyEightsNotifications: Flow<DecksterNotification>?
        get() = joinedGame?.notificationFlow

    override fun onGameLeft() {
        currentState = null
        gameStarted = CompletableDeferred()
    }

    override fun onGameJoined() {
        threadpoolScope.launch {
            crazyEightsNotifications!!.collect { event ->
                println ("Crazy eight notif received, type: ${event.type}")
                when (event) {
                    is GameStartedNotification -> onGameStarted(event)
                    is ItsYourTurnNotification -> onYourTurn(event)
                }
            }
        }
    }

    private fun onYourTurn(event: ItsYourTurnNotification) {
        threadpoolScope.launch {
            yourTurnFlow.emit(event.playerViewOfGame)
        }
    }

    suspend fun passTurn(): EmptyResponse {
        guardNotSpectateMode()
        val playerId = joinedGameOrThrow.userUuid
        val typedMessage = PassRequest(dtoType(PassRequest::class), playerId)
        return sendAndReceive<EmptyResponse>(typedMessage)
    }

    suspend fun drawCard(): DrawnCardData {
        guardNotSpectateMode()
        val playerId = joinedGameOrThrow.userUuid
        val typedMessage = DrawCardRequest(dtoType(DrawCardRequest::class), playerId)
        return sendAndReceive<CardResponse>(typedMessage).let {
            val curState = currentState!!
            currentState = curState.copy(cards = curState.cards + listOf(it.card))
            DrawnCardData(it.card, currentState!!)
        }
    }

    suspend fun putCard(card: Card): PlayerViewOfGame {
        guardNotSpectateMode()
        val playerId = joinedGameOrThrow.userUuid
        val typedMessage = PutCardRequest(dtoType(PutCardRequest::class), playerId, card)
        return sendAndReceive<PlayerViewOfGame>(typedMessage).also {
            this.currentState = it
        }
    }

    suspend fun putEight(card: Card, suit: Suit): PlayerViewOfGame {
        guardNotSpectateMode()
        val playerId = joinedGameOrThrow.userUuid
        val typedMessage = PutEightRequest(dtoType(PutEightRequest::class), playerId, card, suit)
        return sendAndReceive<PlayerViewOfGame>(typedMessage).also {
            this.currentState = it
        }
    }

    data class DrawnCardData(
        val drawnCard: Card,
        val playerViewOfGame: PlayerViewOfGame,
    )
}