package no.forse.decksterlib


import kotlinx.coroutines.suspendCancellableCoroutine
import no.forse.decksterlib.authentication.LoginModel
import no.forse.decksterlib.authentication.UserModel
import no.forse.decksterlib.comminapi.CommonGameApi
import no.forse.decksterlib.communication.DecksterApi
import no.forse.decksterlib.communication.DecksterGameInitiater
import no.forse.decksterlib.communication.DecksterWebSocketListener
import no.forse.decksterlib.communication.MessageSerializer
import no.forse.decksterlib.communication.WebSocketConnection
import no.forse.decksterlib.model.core.GameInfo
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.jackson.JacksonConverterFactory
import java.io.IOException

class DecksterServer(
    hostAddress: String,
) {
    var accessToken: String? = null
    val okHttpClient = OkHttpClient.Builder().addInterceptor(
        HttpLoggingInterceptor().setLevel(
            HttpLoggingInterceptor.Level.BASIC
        )
    ).build()
    val host = hostAddress.let { if (!hostAddress.contains(":")) "$it:13992" else it }
    val hostBaseUrl = "http://$host"

    fun getCommonApi() : CommonGameApi {
        return Retrofit.Builder()
            .baseUrl(hostBaseUrl)
            .client(okHttpClient)
            .addConverterFactory(JacksonConverterFactory.create(MessageSerializer.jackson))
            .build()
            .create(CommonGameApi::class.java)
    }

    private val api = Retrofit.Builder()
        .baseUrl(hostBaseUrl)
        .client(okHttpClient)
        .callFactory {
            okHttpClient.newCall(
                if (accessToken == null) it else it.newBuilder()
                    .addHeader("Authorization", "Bearer $accessToken").build()
            )
        }
        .addConverterFactory(JacksonConverterFactory.create(MessageSerializer.jackson))
        .build()
        .create(DecksterApi::class.java)


    fun getGameInstance(gameName: String, token: String): DecksterGameInitiater {
        return DecksterGameInitiater(this, gameName, token)
    }

    suspend fun create(gameName: String): GameInfo = api.createGame(gameName)

    suspend fun startGame(gameId: String, gameName: String) {
        api.startGame(gameName = gameName, gameId = gameId)
    }

    suspend fun login(credentials: LoginModel): UserModel = api.login(credentials).also {
        accessToken = it.accessToken
    }

    fun getRequest(path: String, token: String): Request {
        return Request.Builder()
            .url("ws://$host/$path")
            .addHeader("Content-Type", "application/json")
            .addHeader("Authorization", "Bearer $token")
            .build()
    }

    suspend fun connectWebSocket(request: Request): WebSocketConnection {
        return suspendCancellableCoroutine<WebSocketConnection> { cont ->
            val listener = DecksterWebSocketListener(cont)
            okHttpClient.newWebSocket(request, listener)
        }
    }
}

class LoginFailedException(cause: IOException) : Throwable(cause)