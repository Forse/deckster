/**
 * Autogenerated by really, really eager small hamsters.
 *
 * Notifications (events) for this game:
 * PlayerSaid: ChatNotification
 *
*/
package no.forse.decksterlib.chatroom

interface ChatRoomClient {
    suspend fun chatAsync(request: SendChatRequest): ChatResponse
}
