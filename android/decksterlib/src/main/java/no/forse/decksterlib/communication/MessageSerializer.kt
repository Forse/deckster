package no.forse.decksterlib.communication

import com.fasterxml.jackson.databind.DeserializationFeature
import com.fasterxml.jackson.databind.ObjectMapper
import com.fasterxml.jackson.module.kotlin.readValue
import no.forse.decksterlib.handshake.DecksterNotification

class MessageSerializer {

    private val jackson = ObjectMapper()
        .configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false)

    fun <T> tryDeserialize(message: String, type: Class<T>): T? {
        return try {
            jackson.readValue<T>(message, type)
        } catch (ex: Exception) {
            println("Error deserializing: $ex. Data:\n$message")
            null
        }
    }

    fun serialize(obj: Any): String = jackson.writeValueAsString(obj)

    fun deserializeNotification(message: String): DecksterNotification? {
        return try {
            jackson.readValue<DecksterNotification>(message)
        } catch (ex: Exception) {
            println("Error deserializing: $ex. Data:\n$message")
            null
        }
    }
}