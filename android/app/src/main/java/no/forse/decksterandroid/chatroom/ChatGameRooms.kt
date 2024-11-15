package no.forse.decksterandroid.chatroom

import BaseScreen
import android.util.Log
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.*
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import no.forse.decksterandroid.ChatRepository
import no.forse.decksterandroid.gamebrowser.GameInfoCard
import no.forse.decksterandroid.shared.theme.Typography

@Composable
fun GameRoom(
    viewModel: ChatRoomsViewModel,
    onEnter: (String) -> Unit,
) {
    BaseScreen(topBarTitle = "Gaming Rooms") {
        LaunchedEffect(key1 = true) {
            Log.d("ChatRoom", "LaunchedEffect")
            viewModel.getGameList()
        }

        val chatRoomUiState = viewModel.uiState.collectAsState().value

        when (chatRoomUiState) {
            is ChatRoomUiState.ChatRoom -> {
                LazyColumn(
                    contentPadding = PaddingValues(16.dp), verticalArrangement =
                    Arrangement.spacedBy(16.dp)
                ) {
                    item { Text("Chat Rooms", style = Typography.titleMedium) }
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
        GameRoom(vm, {})
    }
}
