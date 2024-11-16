package no.forse.decksterandroid.chatroom

import androidx.lifecycle.*
import androidx.lifecycle.viewmodel.CreationExtras
import kotlinx.coroutines.flow.*
import kotlinx.coroutines.launch
import no.forse.decksterlib.DecksterServer
import no.forse.decksterlib.chatroom.ChatRoomClient
import no.forse.decksterlib.model.chatroom.ChatNotification
import no.forse.decksterlib.model.common.PlayerData
import kotlin.reflect.KClass

data class ChatState(val chats: List<ChatMessage>, val users: List<PlayerData>)

data class ChatMessage(val message: String, val sender: String)

class ChatViewModel(
    private val gameName: String,
    private val server: DecksterServer,
) : ViewModel() {

    private var players: List<PlayerData> = emptyList()
    private val chatGame = ChatRoomClient(server)

    private val _chatState: MutableStateFlow<ChatState> = MutableStateFlow(
        value = ChatState(
            emptyList(), players
        )
    )
    val chatState: StateFlow<ChatState> = _chatState.asStateFlow()

    fun sendMessage(message: String) = viewModelScope.launch {
        chatGame.chatAsync(message)
    }

    fun load() = viewModelScope.launch {
        if (server.accessToken == null) throw IllegalStateException("Not logged in to server")
        chatGame.prepareLoggedInGamme(server.accessToken!!)
        chatGame.join(gameName)
        val chats = chatGame.playerSaid!!
        val initialGamelist = chatGame.getGameList()
        players = initialGamelist.find { it.name == gameName }?.players ?: emptyList()
        _chatState.update {
            ChatState(emptyList(), players)
        }

        chats.mapNotNull { it: ChatNotification ->
            println("q: $it")
            val (sender, message) = (it.sender to it.message)
            val gameList = chatGame.getGameList() // Since the name of the sender is not included in the notification we fetch it using the gamelist
            players = gameList
                .find { it.name == gameName }?.players ?: emptyList()
            val senderName = players.find { it.id.toString() == sender }?.name
            if (senderName != null) ChatMessage(message, senderName) else null
        }.collect { chatMessage ->
            println("CghatMsg: $chatMessage")
            _chatState.value = ChatState(_chatState.value.chats + chatMessage, players)
        }
    }

    fun leave() = viewModelScope.launch {
        chatGame.leaveGame()
    }


    class Factory(private val gameId: String, private val server: DecksterServer) : ViewModelProvider.Factory {
        override fun <T : ViewModel> create(modelClass: KClass<T>, extras: CreationExtras): T {
            return ChatViewModel(gameId, server) as T
        }
    }
}