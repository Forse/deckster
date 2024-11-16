package no.forse.decksterandroid.gamebrowser

import androidx.compose.foundation.layout.*
import androidx.compose.material.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import no.forse.decksterlib.model.controllers.GameVm
import org.jetbrains.compose.ui.tooling.preview.Preview

@Composable
fun GameInfoCard(game: GameVm, onJoinGameClicked: () -> Unit, onSpectateClicked: () -> Unit) {
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
                Button(onClick = onJoinGameClicked, modifier = Modifier.padding(8.dp)) {
                    Text("Enter")
                }
                Button(onClick = onSpectateClicked, modifier = Modifier.padding(8.dp)) {
                    Text ("Spectate")
                }
            }
        }
    }
}

@Preview
@Composable
fun GameInfoCardPreview() {

}