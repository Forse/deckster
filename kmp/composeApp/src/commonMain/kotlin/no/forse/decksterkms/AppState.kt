package no.forse.decksterkms

import no.forse.decksterlib.DecksterServer
import no.forse.decksterlib.communication.ConnectedDecksterGame

class AppState {
    var loggedInDecksterServer: DecksterServer? = null
    var connectedDecksterGame: ConnectedDecksterGame? = null
    var gameNameToJoin: String? = null
    var doSpectate = false
}