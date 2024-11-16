package no.forse.decksterkms.gamebrowser

import BaseScreen
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.padding
import androidx.compose.material.Button
import androidx.compose.material.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp


@Composable
fun GameTypeSelector(onBackpressed: () -> Unit, onGameSelected: (String) -> Unit) {
    BaseScreen(topBarTitle = "Gaming Rooms", onBackPressed = {
        onBackpressed.invoke()
    }) {
        Column(modifier = Modifier.padding(32.dp)) {
            Button(onClick = { onGameSelected("crazyeightLobby") }) {
                Text("Crazy Eight")
            }
            Button(onClick = { onGameSelected("uno") }) {
                Text("Uno")
            }
            Button(onClick = { onGameSelected("chatLobby") }) {
                Text("Chat")
            }
        }
    }
}