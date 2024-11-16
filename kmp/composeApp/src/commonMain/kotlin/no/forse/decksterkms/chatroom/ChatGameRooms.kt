package no.forse.decksterandroid.chatroom

import BaseScreen
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.MaterialTheme
import androidx.compose.material.Text
import androidx.compose.runtime.*
import androidx.compose.ui.unit.dp
import no.forse.decksterandroid.gamebrowser.GameInfoCard
import no.forse.decksterkms.ChatRepository
import org.jetbrains.compose.ui.tooling.preview.Preview

@Composable
fun GameRoom(
    viewModel: ChatRoomsViewModel,
    onEnter: (String) -> Unit,
    onBackpressed: () -> Unit,
) {
    BaseScreen(topBarTitle = "Gaming Rooms", onBackPressed = {
        onBackpressed.invoke()
    }) {
        LaunchedEffect(key1 = true) {
            //Log.d("ChatRoom", "LaunchedEffect")
            viewModel.getGameList()
        }

        val chatRoomUiState = viewModel.uiState.collectAsState().value

         when (chatRoomUiState) {
            is ChatRoomUiState.ChatRoom -> {
                LazyColumn(
                    contentPadding = PaddingValues(16.dp), verticalArrangement =
                    Arrangement.spacedBy(16.dp)
                ) {
                    item { Text("Chat Rooms") }
                    items(chatRoomUiState.games) { game ->
                        GameInfoCard(game, onJoinGameClicked = {
                            viewModel.join(game.name) {
                                onEnter(game.name)
                            }
                        })
                    }
                }
            }
        }
    }
}

@Preview
@Composable
fun GameRoomsPreview() {
    MaterialTheme {
        val vm = ChatRoomsViewModel(ChatRepository)
        GameRoom(vm, {}, {})
    }
}
