package no.forse.decksterkms

import androidx.compose.ui.window.Window
import androidx.compose.ui.window.application
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import no.forse.decksterandroid.chatroom.*
import no.forse.decksterandroid.gamebrowser.GamesLobby
import no.forse.decksterandroid.gamebrowser.GamesLobbyViewModel
import no.forse.decksterandroid.login.AppRepository
import no.forse.decksterandroid.login.LoginScreen
import no.forse.decksterandroid.login.LoginViewModel
import no.forse.decksterkms.crazyeight.CrazyEightScreen
import no.forse.decksterkms.crazyeight.CrazyEightViewModel
import no.forse.decksterkms.gamebrowser.GameTypeSelector
import no.forse.decksterlib.crazyeights.CrazyEightsClient

fun main() = application {
    Window(
        onCloseRequest = ::exitApplication,
        title = "DecksterKMS",
    ) {

        val appState = AppState()

        val navController = rememberNavController()

        NavHost(
            navController = navController,
            startDestination = "login",

            ) {
            composable("login") {
                LoginScreen(
                    LoginViewModel(AppRepository()),
                    onLoginSuccess = { decksterServer ->
                        appState.loggedInDecksterServer = decksterServer
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
                val gameRoomVm = viewModel(
                    modelClass = GamesLobbyViewModel::class,
                    factory = GamesLobbyViewModel.Factory("chatroom", appState.loggedInDecksterServer!!)
                )
                GamesLobby(gameRoomVm, onEnterGameName = { gameName ->
                    appState.gameNameToJoin = gameName
                    navController.navigate(
                        "chat/${gameName}"
                    )
                }, onBackpressed = navController::popBackStack)
            }

            composable("chat/{gameId}") { backstack ->
                val gameId = backstack.arguments?.getString("gameId")
                val chatViewModel = viewModel(
                    modelClass = ChatViewModel::class,
                    factory = ChatViewModel.Factory(appState.gameNameToJoin!!, appState.loggedInDecksterServer!!)
                )
                Chat(viewModel = chatViewModel, onBackpressed = {
                    navController.popBackStack()
                })
            }

            composable("crazyeightLobby") {
                val gameRoomVm = viewModel(
                    modelClass = GamesLobbyViewModel::class,
                    factory = GamesLobbyViewModel.Factory("crazyeights", appState.loggedInDecksterServer!!)
                )
                GamesLobby(gameRoomVm, onEnterGameName = { gameName ->
                    appState.gameNameToJoin = gameName
                    navController.navigate(
                        "crazyeight/{gameName}"
                    )
                }, onBackpressed = navController::popBackStack)
            }

            composable("crazyeight/{gameName}") { backstack ->
                val gameId = backstack.arguments?.getString("gameId")
                val viewModel = CrazyEightViewModel(CrazyEightsClient(appState.loggedInDecksterServer!!))
                CrazyEightScreen(viewModel)
            }
        }
    }
}