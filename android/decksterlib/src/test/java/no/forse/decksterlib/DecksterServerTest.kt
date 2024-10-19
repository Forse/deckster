package no.forse.decksterlib

import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.runBlocking
import no.forse.decksterlib.authentication.LoginModel
import no.forse.decksterlib.model.ChatRoomXXXChatNotification
import no.forse.decksterlib.model.ChatRoomXXXSendChatMessage
import no.forse.decksterlib.protocol.getType
import org.junit.Test


class DecksterServerTest {

    //val token = "706ea1f74d6d4fdea33403b89293b580de32a74ed4174cc29d04f93b85448670"
    val gameId = "4d3516d6-8c79-49e4-9db2-3c21d40e3a54"

    @Test
    fun testChatRoom() = runBlocking {
        // todo: extension function to create "type", replace

        // Connects to the chat room specified by gameId with token and sends a "hi there" message
        val lib = DecksterServer("localhost:13992")
        val userModel = lib.login(LoginModel("frode", "1234"))
        val gameClient = lib.getGameInstance("chatroom", userModel.accessToken)
        val game = gameClient.join(gameId)

        val msg1 = ChatRoomXXXSendChatMessage(message = "hi there " + (Math.random() * 1000).toInt())
        val msg2 = msg1.copy(type = msg1.getType())

        CoroutineScope(Dispatchers.Default).launch {
            game.notificationFlow.collect {
                if (it is ChatRoomXXXChatNotification) {
                    println (" --> ${it.sender}: ${it.message}")
                }
            }
        }
        Unit
    }

    class AInfdsa() {

    }
}