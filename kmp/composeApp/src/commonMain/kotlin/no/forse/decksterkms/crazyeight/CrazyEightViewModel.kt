package no.forse.decksterkms.crazyeight

import no.forse.decksterlib.authentication.UserModel
import no.forse.decksterlib.crazyeights.CrazyEightsClient

class CrazyEightViewModel constructor(private val crazyEightsClient: CrazyEightsClient, private val userModel: UserModel){



    suspend fun initialize() {
        //crazyEightsClient.login(LoginModel(userModel.username, ))


        crazyEightsClient.startGame()




    }
}