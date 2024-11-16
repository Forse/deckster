package no.forse.decksterkms.crazyeight

import BaseScreen
import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.*
import androidx.compose.material.Button
import androidx.compose.material.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.viewmodel.compose.viewModel
import decksterkms.composeapp.generated.resources.Res
import decksterkms.composeapp.generated.resources.clubs_10_150
import decksterkms.composeapp.generated.resources.clubs_2_150
import decksterkms.composeapp.generated.resources.clubs_3_150
import decksterkms.composeapp.generated.resources.clubs_4_150
import decksterkms.composeapp.generated.resources.clubs_5_150
import decksterkms.composeapp.generated.resources.clubs_6_150
import decksterkms.composeapp.generated.resources.clubs_7_150
import decksterkms.composeapp.generated.resources.clubs_8_150
import decksterkms.composeapp.generated.resources.clubs_9_150
import decksterkms.composeapp.generated.resources.clubs_a_150
import decksterkms.composeapp.generated.resources.clubs_j_150
import decksterkms.composeapp.generated.resources.clubs_k_150
import decksterkms.composeapp.generated.resources.clubs_q_150
import decksterkms.composeapp.generated.resources.diamonds_10_150
import decksterkms.composeapp.generated.resources.diamonds_2_150
import decksterkms.composeapp.generated.resources.diamonds_3_150
import decksterkms.composeapp.generated.resources.diamonds_4_150
import decksterkms.composeapp.generated.resources.diamonds_5_150
import decksterkms.composeapp.generated.resources.diamonds_6_150
import decksterkms.composeapp.generated.resources.diamonds_7_150
import decksterkms.composeapp.generated.resources.diamonds_8_150
import decksterkms.composeapp.generated.resources.diamonds_9_150
import decksterkms.composeapp.generated.resources.diamonds_a_150
import decksterkms.composeapp.generated.resources.diamonds_j_150
import decksterkms.composeapp.generated.resources.diamonds_k_150
import decksterkms.composeapp.generated.resources.diamonds_q_150
import decksterkms.composeapp.generated.resources.hearts_10_150
import decksterkms.composeapp.generated.resources.hearts_2_150
import decksterkms.composeapp.generated.resources.hearts_3_150
import decksterkms.composeapp.generated.resources.hearts_4_150
import decksterkms.composeapp.generated.resources.hearts_5_150
import decksterkms.composeapp.generated.resources.hearts_6_150
import decksterkms.composeapp.generated.resources.hearts_7_150
import decksterkms.composeapp.generated.resources.hearts_8_150
import decksterkms.composeapp.generated.resources.hearts_9_150
import decksterkms.composeapp.generated.resources.hearts_a_150
import decksterkms.composeapp.generated.resources.hearts_j_150
import decksterkms.composeapp.generated.resources.hearts_k_150
import decksterkms.composeapp.generated.resources.hearts_q_150
import decksterkms.composeapp.generated.resources.spades_10_150
import decksterkms.composeapp.generated.resources.spades_2_150
import decksterkms.composeapp.generated.resources.spades_3_150
import decksterkms.composeapp.generated.resources.spades_4_150
import decksterkms.composeapp.generated.resources.spades_5_150
import decksterkms.composeapp.generated.resources.spades_6_150
import decksterkms.composeapp.generated.resources.spades_7_150
import decksterkms.composeapp.generated.resources.spades_8_150
import decksterkms.composeapp.generated.resources.spades_9_150
import decksterkms.composeapp.generated.resources.spades_a_150
import decksterkms.composeapp.generated.resources.spades_j_150
import decksterkms.composeapp.generated.resources.spades_k_150
import decksterkms.composeapp.generated.resources.spades_q_150
import no.forse.decksterlib.model.common.Card
import no.forse.decksterlib.model.common.Suit
import org.jetbrains.compose.resources.painterResource

@Composable
fun CrazyEightScreen(viewModel: CrazyEightViewModel, onBackpressed: () -> Unit) {
    BaseScreen(topBarTitle = "Crazy Eights", onBackPressed = {
        viewModel.leave()
        onBackpressed.invoke()
    },modifier = Modifier.padding(16.dp)) {
        LaunchedEffect(Unit) {
            viewModel.initialize()
        }
        Text("Game: ${viewModel.gameId} ")
        if (viewModel.spectateMode) {
            Text("SPECTATOR MODE")
        } else {
            Text("Player type: "  + if (viewModel.asbot) "Bot" else "Human"  )
        }

        val state = viewModel.uiState.collectAsState().value
        CrazyEightContent(
            state,
            onCardClicked = viewModel::onCardClicked,
            onSuitSelected = viewModel::onSuitSelected,
            onDrawCard = viewModel::onDrawCard,
            onPass = viewModel::onPass,
        )
    }
}

@Composable
fun CrazyEightContent(
    crazyEightUiState: CrazyEightUiState,
    onCardClicked: (card: Card) -> Unit,
    onSuitSelected: (Suit) -> Unit,
    onDrawCard: () -> Unit,
    onPass: () -> Unit,
) {
    Column(modifier = Modifier.fillMaxWidth()) {
        Row(modifier = Modifier.fillMaxWidth()) {
            Column(modifier = Modifier.weight(.8F)) {
                Text("Status: ", fontSize = 22.sp)
                if (crazyEightUiState.isYourTurn) {
                    if (crazyEightUiState.error == null) {
                        Text("It's your turn!", fontSize = 22.sp)
                    } else {
                        Text(crazyEightUiState.error, fontSize = 22.sp, color = Color.Red)
                    }
                } else {
                    Text("Waiting for other player to make a move...", fontSize = 22.sp)
                }
            }
            Column(modifier = Modifier.weight(.2F)) {
                Button(onClick = onDrawCard, modifier = Modifier.width(120.dp)) {
                    Text("Draw card")
                }
                Spacer(modifier = Modifier.padding(vertical = 4.dp))
                Button(onClick = onPass, modifier = Modifier.width(120.dp)) {
                    Text("Pass")
                }
            }
        }
        Spacer(Modifier.padding(16.dp))

        // top of pile
        GameCardIcon(crazyEightUiState.topOfPile, null)

        Spacer(Modifier.padding(8.dp))

        if (crazyEightUiState.doSuitQuestion) {
            SuitQuestion(onSuitSelected)
        } else {
            Hand(crazyEightUiState, onCardClicked)
        }
    }
}

@Composable
fun SuitQuestion(onSuitSelected: (Suit) -> Unit) {
    Text("Choose suit to change to:", modifier = Modifier.padding(16.dp), color = Color.Blue, fontSize = 24.sp)
    Row {
        GameCardIcon(Card(1, Suit.Spades), { onSuitSelected(Suit.Spades) } )
        Spacer(Modifier.padding(horizontal = 4.dp))
        GameCardIcon(Card(1, Suit.Hearts), { onSuitSelected(Suit.Hearts) } )
        Spacer(Modifier.padding(horizontal = 4.dp))
        GameCardIcon(Card(1, Suit.Clubs), { onSuitSelected(Suit.Clubs) } )
        Spacer(Modifier.padding(horizontal = 4.dp))
        GameCardIcon(Card(1, Suit.Diamonds), { onSuitSelected(Suit.Diamonds) } )

    }
}

@Composable
fun Hand(crazyEightUiState: CrazyEightUiState, onCardClicked: (Card) -> Unit) {
    Text("Your cards:", fontSize = 20.sp)
    Spacer(Modifier.padding(8.dp))
    Row {
        // hand
        showHand(crazyEightUiState.playerHand, onCardClicked)
    }
}

@Composable
fun GameCardIcon(card: Card, onCardClicked: ((card: Card) -> Unit)?) {
    val res = when(card) {
        Card(1, Suit.Clubs) -> Res.drawable.clubs_a_150
        Card(2, Suit.Clubs) -> Res.drawable.clubs_2_150
        Card(3, Suit.Clubs) -> Res.drawable.clubs_3_150
        Card(4, Suit.Clubs) -> Res.drawable.clubs_4_150
        Card(5, Suit.Clubs) -> Res.drawable.clubs_5_150
        Card(6, Suit.Clubs) -> Res.drawable.clubs_6_150
        Card(7, Suit.Clubs) -> Res.drawable.clubs_7_150
        Card(8, Suit.Clubs) -> Res.drawable.clubs_8_150
        Card(9, Suit.Clubs) -> Res.drawable.clubs_9_150
        Card(10, Suit.Clubs) -> Res.drawable.clubs_10_150
        Card(11, Suit.Clubs) -> Res.drawable.clubs_j_150
        Card(12, Suit.Clubs) -> Res.drawable.clubs_q_150
        Card(13, Suit.Clubs) -> Res.drawable.clubs_k_150
        Card(1, Suit.Diamonds) -> Res.drawable.diamonds_a_150
        Card(2, Suit.Diamonds) -> Res.drawable.diamonds_2_150
        Card(3, Suit.Diamonds) -> Res.drawable.diamonds_3_150
        Card(4, Suit.Diamonds) -> Res.drawable.diamonds_4_150
        Card(5, Suit.Diamonds) -> Res.drawable.diamonds_5_150
        Card(6, Suit.Diamonds) -> Res.drawable.diamonds_6_150
        Card(7, Suit.Diamonds) -> Res.drawable.diamonds_7_150
        Card(8, Suit.Diamonds) -> Res.drawable.diamonds_8_150
        Card(9, Suit.Diamonds) -> Res.drawable.diamonds_9_150
        Card(10, Suit.Diamonds) -> Res.drawable.diamonds_10_150
        Card(11, Suit.Diamonds) -> Res.drawable.diamonds_j_150
        Card(12, Suit.Diamonds) -> Res.drawable.diamonds_q_150
        Card(13, Suit.Diamonds) -> Res.drawable.diamonds_k_150
        Card(1, Suit.Hearts) -> Res.drawable.hearts_a_150
        Card(2, Suit.Hearts) -> Res.drawable.hearts_2_150
        Card(3, Suit.Hearts) -> Res.drawable.hearts_3_150
        Card(4, Suit.Hearts) -> Res.drawable.hearts_4_150
        Card(5, Suit.Hearts) -> Res.drawable.hearts_5_150
        Card(6, Suit.Hearts) -> Res.drawable.hearts_6_150
        Card(7, Suit.Hearts) -> Res.drawable.hearts_7_150
        Card(8, Suit.Hearts) -> Res.drawable.hearts_8_150
        Card(9, Suit.Hearts) -> Res.drawable.hearts_9_150
        Card(10, Suit.Hearts) -> Res.drawable.hearts_10_150
        Card(11, Suit.Hearts) -> Res.drawable.hearts_j_150
        Card(12, Suit.Hearts) -> Res.drawable.hearts_q_150
        Card(13, Suit.Hearts) -> Res.drawable.hearts_k_150
        Card(1, Suit.Spades) -> Res.drawable.spades_a_150
        Card(2, Suit.Spades) -> Res.drawable.spades_2_150
        Card(3, Suit.Spades) -> Res.drawable.spades_3_150
        Card(4, Suit.Spades) -> Res.drawable.spades_4_150
        Card(5, Suit.Spades) -> Res.drawable.spades_5_150
        Card(6, Suit.Spades) -> Res.drawable.spades_6_150
        Card(7, Suit.Spades) -> Res.drawable.spades_7_150
        Card(8, Suit.Spades) -> Res.drawable.spades_8_150
        Card(9, Suit.Spades) -> Res.drawable.spades_9_150
        Card(10, Suit.Spades) -> Res.drawable.spades_10_150
        Card(11, Suit.Spades) -> Res.drawable.spades_j_150
        Card(12, Suit.Spades) -> Res.drawable.spades_q_150
        Card(13, Suit.Spades) -> Res.drawable.spades_k_150
        else -> null
    }

    if (onCardClicked != null) {
        Button(onClick = { onCardClicked(card) }) {
            Image(
                painter = painterResource(resource = res!!),
                contentDescription = "card"
            )
        }
    } else {
        Image(
            painter = painterResource(resource = res!!),
            contentDescription = "card"
        )
    }
}

private fun suitToIconName(suit: Suit) = when (suit) {
    Suit.Clubs -> "clubs"
    Suit.Diamonds -> "diamonds"
    Suit.Hearts -> "hearts"
    Suit.Spades -> "spades"
}

private fun rankToIconName(rank: Int) = when (rank) {
    1 -> "a"
    2 -> "2"
    3 -> "3"
    4 -> "4"
    5 -> "5"
    6 -> "6"
    7 -> "7"
    8 -> "8"
    9 -> "9"
    10 -> "10"
    11 -> "j"
    12 -> "q"
    13 -> "k"
    else -> throw IllegalArgumentException("Invalid rank: $rank")
}

@Composable
fun showHand(playerHand: List<Card>, onCardClicked: ((card: Card) -> Unit)?) {
    for (card in playerHand) {
        GameCardIcon(card, onCardClicked)
    }
}
