package no.forse.decksterkms.crazyeight

import androidx.compose.material.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect

@Composable
fun CrazyEightScreen(viewModel: CrazyEightViewModel) {
    Text("You are")

    LaunchedEffect(Unit) {
        viewModel.initialize()
    }

}