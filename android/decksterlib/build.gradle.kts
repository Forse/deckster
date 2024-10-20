plugins {
    id("java-library")
    alias(libs.plugins.jetbrains.kotlin.jvm)
    alias(libs.plugins.openapi.generator)
}

java {
    sourceCompatibility = JavaVersion.VERSION_17
    targetCompatibility = JavaVersion.VERSION_17
    sourceSets["main"].java {
        srcDir("src-gen/src/main/kotlin")
    }
}

kotlin {
    jvmToolchain(17)
}

dependencies {
    implementation(libs.okhttp)
    implementation(libs.jackson.annotations)
    implementation(libs.jackson.kotlin)
    implementation(libs.retrofit)
    implementation(libs.retrofit.converter.scalars)
    implementation(libs.retrofit.converter.jackson)
    implementation(libs.kotlinx.coroutines)
    testImplementation(libs.junit)
    implementation(libs.okhttp)
    implementation(libs.okhttp.logging)
}

tasks.register("generateDtos", org.openapitools.generator.gradle.plugin.tasks.GenerateTask::class.java) {
    // IMPORANT: DELETE build.gradle GENERATED BY THIS TASK, OR STUFF WON'T COMPILE
    // open-api-generate docs: https://github.com/OpenAPITools/openapi-generator/blob/master/modules/openapi-generator-gradle-plugin/README.adoc
    group = "openapi"
    println ("$projectDir")
    description = "Generate DTO classes for DecksterLib"
    val packageRoot = "no.forse.decksterlib"
    generatorName.set("kotlin")
    // kotlin generator docs: https://openapi-generator.tech/docs/generators/kotlin/
    // Templates: https://github.com/OpenAPITools/openapi-generator/blob/master/modules/openapi-generator/src/main/resources/kotlin-client/data_class.mustache
    verbose.set(false)
    cleanupOutput.set(true)
    templateDir.set("$projectDir/openapi-templates")
    outputDir.set("$projectDir/src-gen")
    skipValidateSpec.set(false)
    inputSpec.set("$projectDir/../../decksterapi.yml")
    ignoreFileOverride.set("$projectDir/.openapi-generator-ignore")
    packageName.set("${packageRoot}.rest")
    apiPackage.set("${packageRoot}.rest")
    modelPackage.set("${packageRoot}.model")
    library.set("jvm-retrofit2")
    configOptions = mapOf(
        "serializationLibrary" to "jackson",
        "useCoroutines" to "true"
    )
    generateModelDocumentation.set(false)
    generateApiDocumentation.set(false)
    generateApiTests.set(false)
    generateModelTests.set(false)
    openapiNormalizer.set(mapOf("REF_AS_PARENT_IN_ALLOF" to "true"))
    // modelFilesConstrainedTo.set(emptyList())
    //supportingFilesConstrainedTo.set(listOf("*.kt", "**/*.kt", "build.gradle"))
    // Normalizing should have fixed this https://github.com/OpenAPITools/openapi-generator/issues/6080
    // can't quite get it to work
    // https://github.com/OpenAPITools/openapi-generator/blob/master/docs/customization.md#openapi-normalizer

//templateDir.set("$projectDir/openapi-templates")
    //modelNameSuffix.set("DTO")
}
