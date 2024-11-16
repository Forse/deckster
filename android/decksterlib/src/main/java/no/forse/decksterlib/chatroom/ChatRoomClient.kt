package no.forse.decksterlib.chatroom

import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map
import no.forse.decksterlib.DecksterServer
import no.forse.decksterlib.communication.MessageSerializer
import no.forse.decksterlib.communication.throwOnError
import no.forse.decksterlib.game.GameClientBase
import no.forse.decksterlib.model.chatroom.*
import no.forse.decksterlib.model.controllers.GameVm
import no.forse.decksterlib.model.protocol.DecksterNotification
import no.forse.decksterlib.protocol.dtoType
import no.forse.decksterlib.protocol.getType
import retrofit2.Retrofit
import retrofit2.converter.jackson.JacksonConverterFactory

class ChatRoomClient(
    decksterServer: DecksterServer
) : GameClientBase(decksterServer, "chatroom") {

    private val api = Retrofit.Builder()
        .baseUrl(decksterServer.hostBaseUrl)
        .client(decksterServer.okHttpClient)
        .addConverterFactory(JacksonConverterFactory.create(MessageSerializer.jackson))
        .build()
        .create(ChatRoomApi::class.java)

    suspend fun chatAsync(message: String) {
        val msg = SendChatRequest(dtoType(SendChatRequest::class), message = message, playerId = joinedGameOrThrow.userUuid)
        val response = sendAndReceive<ChatResponse>(msg)
        response.throwOnError()
    }

    suspend fun getGameList(): List<GameVm> = api.getGames()

    val playerSaid: Flow<ChatNotification>?
        get() = joinedGame?.notificationFlow?.map { it as ChatNotification }

    override suspend fun onNotificationArrived(notif: DecksterNotification) {
        println("ChatRoom onMessageArrived: $notif")
    }

    override fun onGameLeft() {
    }

    override fun onGameJoined() {
    }
}