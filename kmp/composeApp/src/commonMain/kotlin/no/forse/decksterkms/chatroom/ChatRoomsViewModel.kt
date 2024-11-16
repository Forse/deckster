package no.forse.decksterandroid.chatroom

import androidx.lifecycle.*
import androidx.lifecycle.viewmodel.CreationExtras
import kotlinx.coroutines.flow.*
import kotlinx.coroutines.launch
import no.forse.decksterkms.ChatRepository
import no.forse.decksterlib.model.controllers.GameVm
import kotlin.concurrent.timer
import kotlin.reflect.KClass

sealed interface ChatRoomUiState {
    data class ChatRoom(val games: List<GameVm>) :
        GamesLobbyUiState
}

class ChatRoomsViewModel(private val decksterRepository: ChatRepository) : ViewModel() {

    private val _uiState: MutableStateFlow<GamesLobbyUiState> =
        MutableStateFlow(value = GamesLobbyUiState.GameList(emptyList()))
    val uiState: StateFlow<GamesLobbyUiState> = _uiState.asStateFlow()


    fun join(id: String, onDone: () -> Unit) = viewModelScope.launch {
        decksterRepository.joinChat(id)
        onDone()
    }

    fun getGameList() = timer("GetGameList", period = 1000) {
        viewModelScope.launch {
            val games = decksterRepository.getGameList()
            _uiState.update { GamesLobbyUiState.GameList(games) }
        }
    }

    class Factory : ViewModelProvider.Factory {
        override fun <T : ViewModel> create(modelClass: KClass<T>, extras: CreationExtras): T {
            return GamesLobbyViewModel(ChatRepository) as T
        }
    }
}