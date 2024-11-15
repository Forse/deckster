package no.forse.decksterandroid.chatroom

import androidx.lifecycle.*
import kotlinx.coroutines.flow.*
import kotlinx.coroutines.launch
import no.forse.decksterkms.ChatRepository
import no.forse.decksterlib.model.chatroom.ChatNotification
import no.forse.decksterlib.model.common.PlayerData

data class ChatState(val chats: List<ChatMessage>, val users: List<PlayerData>)

data class ChatMessage(val message: String, val sender: String)


class ChatViewModel(
    private val chatRepository: ChatRepository
) : ViewModel() {

    private var players: List<PlayerData> = emptyList()

    private val _chatState: MutableStateFlow<ChatState> = MutableStateFlow(
        value = ChatState(
            emptyList(), players
        )
    )
    val chatState: StateFlow<ChatState> = _chatState.asStateFlow()

    fun sendMessage(message: String) = viewModelScope.launch {
        ChatRepository.sendMessage(message)
    }

    fun getChat(chatId: String?) = viewModelScope.launch {
        val chats = ChatRepository.getChats()
        val initialGamelist = ChatRepository.getGameList()
        players = initialGamelist.find { it.name == chatId }?.players ?: emptyList()
        _chatState.update {
            ChatState(emptyList(), players)
        }

        chats.mapNotNull { it: ChatNotification ->
            val (sender, message) = (it.sender to it.message)
            val gameList =
                ChatRepository.getGameList() // Since the name of the sender is not included in the notification we fetch it using the gamelist
            players = gameList
                .find { it.name == chatId }?.players ?: emptyList()
            val senderName = players.find { it.id.toString() == sender }?.name
            if (senderName != null) ChatMessage(message, senderName) else null
        }.collect { chatMessage ->
            _chatState.value = ChatState(_chatState.value.chats + chatMessage, players)
        }
    }

    fun leave() = viewModelScope.launch {
        ChatRepository.leaveChat()
    }
}