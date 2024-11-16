/**
 * Autogenerated by really, really eager small hamsters.
 *
 * Notifications (events) for this game:
 * GameStarted: GameStartedNotification
 * PlayerPutCard: PlayerPutCardNotification
 * PlayerDrewCard: PlayerDrewCardNotification
 * PlayerDrewPenaltyCard: PlayerDrewPenaltyCardNotification
 * GameEnded: GameEndedNotification
 * RoundStarted: RoundStartedNotification
 * RoundEnded: RoundEndedNotification
 * PlayerLostTheirTurn: PlayerLostTheirTurnNotification
 *
*/
package no.forse.decksterlib.gabong

interface GabongClient {
    suspend fun drawCard(request: DrawCardRequest): PlayerViewOfGame
    suspend fun playGabong(request: PlayGabongRequest): PlayerViewOfGame
    suspend fun playBonga(request: PlayBongaRequest): PlayerViewOfGame
    suspend fun pass(request: PassRequest): PlayerViewOfGame
    suspend fun putCard(request: PutCardRequest): PlayerViewOfGame
}
