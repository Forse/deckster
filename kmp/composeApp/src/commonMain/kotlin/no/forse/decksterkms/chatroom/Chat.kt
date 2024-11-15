package no.forse.decksterandroid.chatroom

import BaseScreen
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.*
import androidx.compose.material.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

@Composable
fun Chat(id: String?, viewModel: ChatViewModel, onBackpressed: () -> Unit) {
    LaunchedEffect(key1 = true) {
        viewModel.getChat(id)
    }

    val chatState = viewModel.chatState.collectAsState().value
    var message by remember { mutableStateOf("") }
    val state = remember { LazyListState() }

    BaseScreen(topBarTitle = "Chat", onBackPressed = {
        viewModel.leave()
        onBackpressed.invoke()
    }) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(16.dp)
        ) {
            Text(text = "Chat with ${chatState.users.map { it.name }.joinToString(separator = ",")}")
            LazyColumn(reverseLayout = true, modifier = Modifier.weight(1f), state = state) {
                items(chatState.chats.reversed()) { chat ->
                    ChatMessageItem(chat.message, chat.sender)
                }
            }
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween
            ) {

                TextField(value = message, onValueChange = {
                    message = it
                })
                Button(onClick = {
                    viewModel.sendMessage(message)
                    message = ""
                }) {
                    Text("Send")
                }
            }
        }
    }
}

@Composable
fun ChatMessageItem(message: String, from: String) {
    Text("$from: $message")
}
