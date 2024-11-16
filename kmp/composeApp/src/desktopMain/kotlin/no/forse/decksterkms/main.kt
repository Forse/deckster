package no.forse.decksterkms

import androidx.compose.foundation.layout.Column
import androidx.compose.material.Button
import androidx.compose.material.Text
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
        var lastUserModel: UserModel? = null

        val navController = rememberNavController()

        NavHost(
            navController = navController,
            startDestination = "login",

        ) {
            composable("login") {
                LoginScreen(
                    LoginViewModel(ChatRepository, AppRepository()),
                    onLoginSuccess = { decksterServer, userModel ->
                        lastDecksterServer = decksterServer
                        lastUserModel = userModel
                        navController.navigate(route = "gamelist")
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
            composable("chatroom") {
               /* val chatViewModel = viewModel(
                    modelClass = ChatViewModel::class.java,
                    factory = ChatViewModel.Factory()
                )*/
                val viewModel = ChatViewModel(ChatRepository)
                Chat(id = null, viewModel = viewModel, onBackpressed = {
                    navController.popBackStack()
                })
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

            composable("crazyeight") {
                val viewModel = CrazyEightViewModel(CrazyEightsClient(lastDecksterServer!!), lastUserModel: UserModel)
                CrazyEightScreen(viewModel)
            }
        }
    }
}