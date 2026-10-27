// Re-export the generated C# tutorial catalog so guide views share one lesson definition.
// Change InterfaceCatalog.cs and regenerate instead of maintaining browser-only tutorial text.
import {featureTutorials} from './generated-content.mjs';
export {featureTutorials} from './generated-content.mjs';
