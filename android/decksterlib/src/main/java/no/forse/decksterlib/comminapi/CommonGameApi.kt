package no.forse.decksterlib.comminapi

import no.forse.decksterlib.model.controllers.GameVm
import retrofit2.http.GET
import retrofit2.http.Path

interface CommonGameApi {
    @GET("/{gameId}/games")
    suspend fun getGames(@Path("gameId") gameId: String) : List<GameVm>
}