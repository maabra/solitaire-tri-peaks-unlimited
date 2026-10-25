# Third-party components

Original Meadow Peaks code and generated project artwork use the project LICENSE.
User-supplied references, videos and example games are not distributed.

The Windows executable bundles .NET and Microsoft Edge WebView2 Fixed Version
156.0.4314.42 x64. These Microsoft components retain their own licenses and notices;
the project LICENSE does not relicense them. The WebView2 SDK LICENSE/NOTICE files
are shipped beside the game and included in the source archive. .NET runtime
LICENSE and third-party notices are also included. The official WebView2 runtime
CAB is packaged unchanged, preserving its included third-party material.

Runtime source: Microsoft's [WebView2 download page](https://developer.microsoft.com/microsoft-edge/webview2/).
Distribution instructions: [Microsoft Learn](https://learn.microsoft.com/microsoft-edge/webview2/concepts/distribution).
The pinned download URL and SHA-256 are recorded in `scripts/prepare_webview_runtime.ps1`.
The large runtime CAB is excluded from Git/source exports and downloaded only when
building the Windows distribution. Refresh the pinned version and checksum when
updating the bundled renderer, and rerun packaged startup/UI checks.

## Bundled fonts

The game bundles unmodified variable fonts for offline use in Windows and the browser.

- **Nunito**, interface text and readable numerical labels. Copyright 2014 The Nunito Project Authors. Source: https://github.com/google/fonts/tree/main/ofl/nunito . License: `web/assets/fonts/nunito-OFL.txt`.
- **Baloo 2**, titles and primary play controls. Copyright 2019 The Baloo 2 Project Authors. Source: https://github.com/google/fonts/tree/main/ofl/baloo2 . License: `web/assets/fonts/baloo2-OFL.txt`.

Both fonts use the SIL Open Font License 1.1. Their licenses accompany the font files in the embedded Windows resources and web package. The game does not contact a font service at runtime.
