package no.forse.decksterlib.protocol

import no.forse.decksterlib.model.protocol.DecksterMessage
import kotlin.reflect.KClass

fun DecksterMessage.getType(): String = this.javaClass.name.replace("no.forse.decksterlib.model.", "")

fun <T> dtoType(clazz: KClass<T>):  String where T: DecksterMessage =
    clazz.qualifiedName.toString().replace("no.forse.decksterlib.model.", "")