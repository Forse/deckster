package no.forse.decksterandroid.login

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.flow.*
import kotlinx.coroutines.launch
import no.forse.decksterkms.ChatRepository
import no.forse.decksterlib.DecksterServer
import no.forse.decksterlib.authentication.LoginModel
import no.forse.decksterlib.authentication.UserModel

sealed interface LoginUiState {
    data class Initial(val details: LoginDetails) : LoginUiState
    object Loading : LoginUiState
    data class Error(val details: LoginDetails) : LoginUiState
    data class Success(val decksterServer: DecksterServer) : LoginUiState
}

class LoginViewModel(
    private val chatRepository: ChatRepository,
    private val appRepository: AppRepository
) : ViewModel() {

    private val loginDetails = appRepository.getLoginDetails()

    private val _uiState: MutableStateFlow<LoginUiState> =
        MutableStateFlow(value = LoginUiState.Initial(loginDetails))

    val uiState: StateFlow<LoginUiState> = _uiState.asStateFlow()

    fun login(serverIp: String, username: String, password: String) = viewModelScope.launch {

        _uiState.update { currentState ->
            LoginUiState.Loading
        }


        try {
            val server = DecksterServer("$serverIp")
            val userModel = server.login(LoginModel(username, password))

            _uiState.update { currentState ->
                LoginUiState.Success(server)
            }

        } catch (e: Exception) {
            _uiState.update { currentState ->
                LoginUiState.Error(loginDetails)
            }
            return@launch
        }
    }
}

data class LoginDetails(
    val serverIp: String,
    val username: String,
    val password: String
)

class AppRepository() {

    fun saveLoginDetails(serverIp: String, username: String, password: String) {

    }

    fun getLoginDetails(): LoginDetails {
        //return LoginDetails(
        //
        //)
        return LoginDetails("192.168.0.178", "", "")
    }
}

