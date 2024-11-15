package no.forse.decksterandroid.chatroom

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.flow.*
import kotlinx.coroutines.launch
import no.forse.decksterkms.ChatRepository
import no.forse.decksterlib.model.controllers.GameVm
import kotlin.concurrent.timer

sealed interface ChatRoomUiState {
    data class ChatRoom(val games: List<GameVm>) :
        ChatRoomUiState
}

class ChatRoomsViewModel(private val decksterRepository: ChatRepository) : ViewModel() {

    private val _uiState: MutableStateFlow<ChatRoomUiState> =
        MutableStateFlow(value = ChatRoomUiState.ChatRoom(emptyList()))
    val uiState: StateFlow<ChatRoomUiState> = _uiState.asStateFlow()


    fun join(id: String, onDone: () -> Unit) = viewModelScope.launch {
        decksterRepository.joinChat(id)
        onDone()
    }

    fun getGameList() = timer("GetGameList", period = 1000) {
        viewModelScope.launch {
            val games = decksterRepository.getGameList()
            _uiState.update { ChatRoomUiState.ChatRoom(games) }
        }
    }
}