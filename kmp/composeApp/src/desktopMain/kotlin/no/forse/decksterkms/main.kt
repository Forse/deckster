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
                GamesLobby(gameRoomVm,
                    onEnterGameName = { gameName ->
                        appState.gameNameToJoin = gameName
                        appState.doSpectate = false
                        navController.navigate(
                            "chat/${gameName}"
                        )
                    },
                    onBotGameName = { gameName ->
                        appState.gameNameToJoin = gameName
                        appState.doSpectate = true
                        navController.navigate(
                            "chat/${gameName}"
                        )
                    },
                    onSpectateGameName = { gameName ->
                        appState.gameNameToJoin = gameName
                        appState.doSpectate = true
                        navController.navigate(
                            "chat/${gameName}"
                        )
                    },
                onBackpressed = navController::popBackStack)
            }

            composable("chat/{gameId}") { backstack ->
                val gameId = backstack.arguments?.getString("gameId")!!
                val chatViewModel = viewModel(
                    modelClass = ChatViewModel::class,
                    factory = ChatViewModel.Factory(gameId, appState.loggedInDecksterServer!!)
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
                GamesLobby(gameRoomVm,
                    onEnterGameName = { gameName ->
                        println("main onPlyeer")
                        appState.doSpectate = false
                        navController.navigate(
                            "crazyeight/$gameName/false"
                        )
                    },
                    onBotGameName = { gameName ->
                        println("main onbot")
                        appState.doSpectate = false
                        navController.navigate(
                            "crazyeight/$gameName/true"
                        )
                    },
                    onSpectateGameName = { gameName ->
                        appState.doSpectate = true
                        navController.navigate(
                            "crazyeight/${gameName}/false"
                        )
                    },
                    onBackpressed = navController::popBackStack)
            }

            composable("crazyeight/{gameName}/{asBot}") { backstack ->
                val gameId = backstack.arguments?.getString("gameName")!!
                val asBot = backstack.arguments?.getString("asBot") == "true"
                val viewModel = viewModel(
                    modelClass = CrazyEightViewModel::class,
                    factory = CrazyEightViewModel.Factory(
                        gameId,
                        asBot,
                        appState.doSpectate,
                        appState.loggedInDecksterServer!!
                    )
                )
                CrazyEightScreen(viewModel, onBackpressed = navController::popBackStack)
            }
        }
    }
}