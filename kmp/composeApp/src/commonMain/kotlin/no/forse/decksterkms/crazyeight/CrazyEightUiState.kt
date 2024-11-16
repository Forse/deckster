package no.forse.decksterkms.crazyeight

import no.forse.decksterlib.model.common.Card
import no.forse.decksterlib.model.common.Suit

data class CrazyEightUiState(
    val playerHand: List<Card>,
    val topOfPile: Card,
    val isYourTurn: Boolean,
    val topCardIsYours: Boolean,
    val currentSuit: Suit,
    val error: String?,
    /** When 8 is played, ask for suit */
    val doSuitQuestion: Boolean,
    val gameState: GameState,
    val loseName: String? = null,
) {
    val canPlayCard: Boolean
        get() = isYourTurn && !doSuitQuestion
}

enum class GameState {
    WAITING,
    STARTED,
    ENDED,
}