package no.forse.decksterandroid.gamebrowser

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import no.forse.decksterlib.model.controllers.GameVm

@Composable
fun GameInfoCard(game: GameVm, onJoinGameClicked: () -> Unit) {
    Card {
        Column(modifier = Modifier.padding(16.dp)) {
            Text(
                "Chat room id: ${game.name}"
            )
            Spacer(modifier = Modifier.height(8.dp))
            Text("state: ${game.state}")
            Spacer(modifier = Modifier.height(8.dp))
            Text("players: ${
                game.players.joinToString(", ") { it.name }
            }")
            Spacer(modifier = Modifier.height(8.dp))

            Row {
                Button(onClick = onJoinGameClicked) {
                    Text("Enter")
                }
            }
        }
    }
}

@Preview
@Composable
fun GameInfoCardPreview() {

}