package no.forse.decksterkms

import androidx.compose.ui.window.Window
import androidx.compose.ui.window.application
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.compose.*
import no.forse.decksterandroid.chatroom.*
import no.forse.decksterandroid.login.*
import no.forse.decksterkms.gamebrowser.GameTypeSelector

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
                        navController.navigate("gameTypeSelect")
                    }
                )
            }

            composable("gameTypeSelect") {
                GameTypeSelector(onBackpressed = navController::popBackStack) { gameName ->
                    navController.navigate(gameName)
                }
            }

            composable("chatLobby") {
                val chatRoomViewModel = viewModel(
                    modelClass = ChatRoomsViewModel::class,
                    factory = ChatRoomsViewModel.Factory()
                )
                GameRoom(chatRoomViewModel, onEnter = { gameId ->
                    navController.navigate(
                        "chat/$gameId"
                    )
                }, onBackpressed = navController::popBackStack)
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