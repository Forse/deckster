/**
 *
 * Please note:
 * This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * Do not edit this file manually.
 *
 */

@file:Suppress(
    "ArrayInDataClass",
    "EnumEntryName",
    "RemoveRedundantQualifierName",
    "UnusedImport"
)

package no.forse.decksterlib.model


import com.fasterxml.jackson.annotation.JsonProperty

/**
 * 
 *
 * @param rank 
 * @param suit 
 */


data class CommonCard (

    @get:JsonProperty("rank")
    val rank: kotlin.Int? = null,

    @get:JsonProperty("suit")
    val suit: kotlin.String? = null

) {


}

