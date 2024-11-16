package no.forse.decksterandroid.gamebrowser

import androidx.lifecycle.*
import androidx.lifecycle.viewmodel.CreationExtras
import kotlinx.coroutines.flow.*
import kotlinx.coroutines.launch
import no.forse.decksterlib.DecksterServer
import no.forse.decksterlib.communication.ConnectedDecksterGame
import no.forse.decksterlib.communication.DecksterGameInitiater
import no.forse.decksterlib.model.controllers.GameVm
import java.util.Timer
import kotlin.concurrent.timer
import kotlin.reflect.KClass

sealed interface GamesLobbyUiState {
    data class GameList(val games: List<GameVm>) :
        GamesLobbyUiState
}

class GamesLobbyViewModel(
    private val gameName: String,
    private val decksterServer: DecksterServer? = null
) : ViewModel() {

    private val _uiState: MutableStateFlow<GamesLobbyUiState> =
        MutableStateFlow(value = GamesLobbyUiState.GameList(emptyList()))
    val uiState: StateFlow<GamesLobbyUiState> = _uiState.asStateFlow()


    fun join(gameId: String, onDone: () -> Unit) = viewModelScope.launch {
        try {
            //val server = decksterServer ?: throw IllegalStateException("Server is null")
           // val token = server.accessToken ?: throw IllegalStateException("Token is null. Have you logged in?")
           // val gameIniter = DecksterGameInitiater(server, gameName, token)

         //   val connectedGame = gameIniter.join(gameId)
            gamePollTimer?.cancel()
            onDone()
        } catch (ex: Exception) {
            println("Error joining game $gameId for $gameName : $ex")
        }
    }

    var gamePollTimer: Timer? = null

    fun pollGameList() {
        gamePollTimer = timer("GetGameList", period = 1000) {
            viewModelScope.launch {
                val api = decksterServer!!.getCommonApi()
                val games = api.getGames(gameName)
                _uiState.update { GamesLobbyUiState.GameList(games) }
            }
        }
    }

    class Factory(val gameName: String, val loggedInServer: DecksterServer) : ViewModelProvider.Factory {
        override fun <T : ViewModel> create(modelClass: KClass<T>, extras: CreationExtras): T {
            if (loggedInServer.accessToken == null) throw IllegalStateException("Must be logged in to server before presenting lobby")
            return GamesLobbyViewModel(gameName, loggedInServer) as T
        }
    }
}