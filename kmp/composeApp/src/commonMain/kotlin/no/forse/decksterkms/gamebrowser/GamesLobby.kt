package no.forse.decksterandroid.chatroom

import BaseScreen
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.Text
import androidx.compose.runtime.*
import androidx.compose.ui.unit.dp
import no.forse.decksterandroid.gamebrowser.GameInfoCard

@Composable
fun GamesLobby(
    viewModel: GamesLobbyViewModel,
    onEnter: (String) -> Unit,
    onBackpressed: () -> Unit,
) {
    BaseScreen(topBarTitle = "Gaming Rooms", onBackPressed = {
        onBackpressed.invoke()
    }) {
        LaunchedEffect(key1 = true) {
            //Log.d("ChatRoom", "LaunchedEffect")
            viewModel.pollGameList()
        }

        val chatRoomUiState = viewModel.uiState.collectAsState().value

         when (chatRoomUiState) {
            is GamesLobbyUiState.GameList -> {
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

