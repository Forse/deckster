package no.forse.decksterkms

import androidx.compose.foundation.layout.Column
import androidx.compose.material.*
import androidx.compose.runtime.collectAsState
import androidx.compose.ui.window.Window
import androidx.compose.ui.window.application
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavType
import androidx.navigation.Navigator
import androidx.navigation.compose.*
import androidx.navigation.navArgument
import no.forse.decksterandroid.chatroom.*
import no.forse.decksterandroid.login.*
import no.forse.decksterkms.crazyeight.CrazyEightScreen
import no.forse.decksterkms.crazyeight.CrazyEightViewModel
import no.forse.decksterlib.DecksterServer
import no.forse.decksterlib.authentication.UserModel
import no.forse.decksterlib.crazyeights.CrazyEightsClient
import kotlin.reflect.typeOf

data class GameListExtras(val userModel: UserModel) : Navigator.Extras

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
                        navController.navigate(route = "gamelist")
                    }
                )
            }
            composable("gamelist") {
                Column {
                    Button(onClick = {
                        navController.navigate("crazyeightLobby")
                    }) {
                        Text("Crazyeight")
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


            composable("crazyeightLobby") {
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

            composable("crazyeight") {
                val viewModel = CrazyEightViewModel(CrazyEightsClient(lastDecksterServer!!))
                CrazyEightScreen(viewModel)
            }
        }
    }
}