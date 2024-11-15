package no.forse.decksterkms

import androidx.compose.foundation.layout.Column
import androidx.compose.material.*
import androidx.compose.runtime.collectAsState
import androidx.compose.ui.window.Window
import androidx.compose.ui.window.application
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.compose.*
import no.forse.decksterandroid.chatroom.*
import no.forse.decksterandroid.login.*

fun main() = application {
    Window(
        onCloseRequest = ::exitApplication,
        title = "DecksterKMS",
    ) {

        val navController = rememberNavController()

        NavHost(
            navController = navController,
            startDestination = "login",

            ) {
            composable("login") {
                LoginScreen(
                    LoginViewModel(ChatRepository, AppRepository()),
                    onLoginSuccess = {
                        navController.navigate("gamelist")
                    }
                )
            }
            composable("gamelist") {
                Column {
                    Button(onClick = {
                        navController.navigate("crazyeight")
                    }) {
                        Text("crazyeight")
                    }
                    Button(onClick = {
                        navController.navigate("uno")
                    }) {
                        Text("uno")
                    }
                    Button(onClick = {
                        navController.navigate("chatGameList")
                    }) {
                        Text("chatRoom")
                    }
                }
            }

            composable("chatGameList") {
                val chatRoomViewModel = viewModel(
                    modelClass = ChatRoomsViewModel::class,
                    factory = ChatRoomsViewModel.Factory()
                )
                GameRoom(chatRoomViewModel, onEnter = { gameId ->
                    navController.navigate(
                        "chat/$gameId"
                    )
                })
            }

            composable("chat/{gameId}") { backstack ->
                val gameId = backstack.arguments?.getString("gameId")
                val chatViewModel = viewModel(
                    modelClass = ChatViewModel::class,
                    factory = ChatViewModel.Factory()
                )
                Chat(id = gameId, viewModel = chatViewModel, onBackpressed = {
                    navController.popBackStack()
                })
            }
        }
    }
}