package no.forse.decksterlib.chatroom

import no.forse.decksterlib.model.controllers.GameVm
import retrofit2.http.GET

interface ChatRoomApi {
    @GET("/chatroom/games")
    suspend fun getGames() : List<GameVm>
}