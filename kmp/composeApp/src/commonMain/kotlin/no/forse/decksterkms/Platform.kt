package no.forse.decksterkms

interface Platform {
    val name: String
}

expect fun getPlatform(): Platform