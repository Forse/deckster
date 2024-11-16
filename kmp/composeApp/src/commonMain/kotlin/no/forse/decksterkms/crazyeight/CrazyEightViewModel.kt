package no.forse.decksterkms.crazyeight

import no.forse.decksterlib.crazyeights.CrazyEightsClient

class CrazyEightViewModel constructor(private val crazyEightsClient: CrazyEightsClient){



    suspend fun initialize() {
        //crazyEightsClient.login(LoginModel(userModel.username, ))


        crazyEightsClient.startGame()




    }
}