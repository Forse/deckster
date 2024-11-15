package no.forse.decksterkms

import androidx.compose.foundation.layout.Column
import androidx.compose.material.Button
import androidx.compose.material.Text
import androidx.compose.ui.Modifier
import androidx.compose.ui.window.Window
import androidx.compose.ui.window.application
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.compose.*
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
                        navController.navigate("chatroom")
                    }) {
                        Text("chatroom")
                    }
                }
            }

            composable("gameRoom") {
                /*val chatRoomViewModel = viewModel(

                    modelClass = ChatRoomsViewModel::class.java,
                    factory = ChatRoomsViewModel.Factory()
                )
                GameRoom(viewModel = chatRoomViewModel, onEnter = { id ->
                    navController.navigate(
                        "gameRoom/$id"
                    )
                })*/
            }
        }
    }
}