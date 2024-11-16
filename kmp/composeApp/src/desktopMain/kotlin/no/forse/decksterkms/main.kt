package no.forse.decksterkms

import androidx.compose.ui.window.Window
import androidx.compose.ui.window.application
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import no.forse.decksterandroid.chatroom.Chat
import no.forse.decksterandroid.chatroom.ChatRoomsViewModel
import no.forse.decksterandroid.chatroom.ChatViewModel
import no.forse.decksterandroid.chatroom.GameRoom
import no.forse.decksterandroid.login.AppRepository
import no.forse.decksterandroid.login.LoginScreen
import no.forse.decksterandroid.login.LoginViewModel
import no.forse.decksterkms.crazyeight.CrazyEightScreen
import no.forse.decksterkms.crazyeight.CrazyEightViewModel
import no.forse.decksterkms.gamebrowser.GameTypeSelector
import no.forse.decksterlib.DecksterServer
import no.forse.decksterlib.crazyeights.CrazyEightsClient

fun main() = application {
    Window(
        onCloseRequest = ::exitApplication,
        title = "DecksterKMS",
    ) {

        var lastDecksterServer: DecksterServer? = null

        val navController = rememberNavController()

        NavHost(
            navController = navController,
            startDestination = "login",

            ) {
            composable("login") {
                LoginScreen(
                    LoginViewModel(ChatRepository, AppRepository()),
                    onLoginSuccess = { decksterServer ->
                        lastDecksterServer = decksterServer
                        navController.navigate(route = "gameTypeSelect")
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


            composable("crazyeightLobby") {
                val chatRoomViewModel = viewModel(
                    modelClass = ChatRoomsViewModel::class,
                    factory = ChatRoomsViewModel.Factory()
                )
                GameRoom(chatRoomViewModel, onEnter = { gameId ->
                    navController.navigate(
                        "chat/$gameId"
                    )
                }, onBackpressed = {
                    navController.popBackStack()
                })
            }

            composable("crazyeight") {
                val viewModel = CrazyEightViewModel(CrazyEightsClient(lastDecksterServer!!))
                CrazyEightScreen(viewModel)
            }
        }
    }
}