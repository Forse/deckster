package no.forse.decksterkms.crazyeight

import no.forse.decksterlib.model.common.Card

data class CrazyEightUiState(

    val playerHand: List<Card>,
    val topOfPile: Card
)