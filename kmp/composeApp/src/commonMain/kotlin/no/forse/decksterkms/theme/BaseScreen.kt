
import androidx.compose.foundation.layout.*
import androidx.compose.material.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp

@Composable
fun BaseScreen(
    modifier: Modifier = Modifier,
    topBarTitle: String,
    onBackPressed: (() -> Unit)? = null,
    content: @Composable (PaddingValues) -> Unit,
) {
    Scaffold(
        topBar = {
            TopAppBar(
                windowInsets = WindowInsets(
                    top = 0.dp,
                    left = 0.dp,
                    right = 0.dp,
                    bottom = 0.dp
                ),
                title = {
                    Text(
                        topBarTitle,
                        maxLines = 1,
                        overflow = TextOverflow.Ellipsis,
                        //style = Typography.titleLarge
                    )
                },
                navigationIcon = {
                    if (onBackPressed != null) IconButton(onClick = { onBackPressed?.invoke() }) {
                        Icon(
                            Icons.AutoMirrored.Default.ArrowBack,
                            contentDescription = "Back",
                            modifier = Modifier.testTag("backButton")
                        )
                    }
                },
            )
        },
        modifier = modifier,
        content = { innerPadding ->
            Column(
                modifier = modifier
                    .consumeWindowInsets(innerPadding)
                    .padding(innerPadding),
            ) {
                content(innerPadding)
            }
        },
    )
}