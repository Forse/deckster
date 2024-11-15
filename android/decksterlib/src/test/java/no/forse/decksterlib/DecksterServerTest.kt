package no.forse.decksterlib

import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.runBlocking
import no.forse.decksterlib.authentication.LoginModel
import no.forse.decksterlib.chatroom.ChatRoomClient
import org.junit.Test
import java.net.HttpURLConnection
import java.net.URL


class DecksterServerTest {

    private fun prop(propName: String, defaultVal: String): String {
        val prop = System.getProperty(propName)
        if (prop.isNullOrBlank()) {
            println("WARN: Property '$propName' not set. Using default.")
            return defaultVal
        }
        return prop
    }

    @Test
    fun testChatRoom() = runBlocking {
        val lib = DecksterServer("192.168.1.183:13992")

        val chatGame = ChatRoomClient(lib)
        val gameId = prop("gameId", "tåpelig vær")
        val user = LoginModel(
            username = prop("userId", "kjetil"),
            password = prop("password", "123456"),
        )
        println("Attempting to join game as user '${user.username}', gameId '$gameId'")
        chatGame.login(user)

        println ("Joining game START...")
        chatGame.joinGame(gameId)
        println ("Joining game DONE...")
        chatGame.chatAsync(message = "hi there " + (Math.random() * 1000).toInt())

        CoroutineScope(Dispatchers.Default).launch {
            chatGame.playerSaid!!.collect() {
                println (" --> ${it.sender}: ${it.message}")
            }
        }
        Thread.sleep(2000)
        chatGame.leaveGame()
        Unit
    }

    @Test
    fun testGetGameList() = runBlocking {
        val lib = DecksterServer("localhost:13992")
        val user = LoginModel(
            username = prop("userId", "defaultUser111"),
            password = prop("password", "1234"),
        )
        val chatGame = ChatRoomClient(lib)
        val list = chatGame.getGameList()
        println(list.joinToString("\n"))
    }
}