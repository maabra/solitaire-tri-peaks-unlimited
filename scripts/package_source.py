"""Export only original project files. Reference examples are never traversed."""
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED

root = Path(__file__).resolve().parents[1]
output = root / "artifacts" / "MeadowPeaks-source.zip"
output.parent.mkdir(exist_ok=True)
allowed = ["src", "tests", "scripts", "docs", ".github", "web", "shared"]
files = [root / name for name in ("README.md", "LICENSE", ".gitignore", "global.json", "TODO.txt")]
for directory in allowed:
    files.extend(p for p in (root / directory).rglob("*") if p.is_file() and not {"bin", "obj", "__pycache__"}.intersection(p.relative_to(root).parts))
with ZipFile(output, "w", ZIP_DEFLATED) as archive:
    for file in sorted(files):
        archive.write(file, file.relative_to(root).as_posix())
with ZipFile(output) as archive:
    assert all(not {"examples", "bin", "obj", "artifacts", ".git"}.intersection(Path(n).parts) for n in archive.namelist())
print(f"Source archive: {output} ({len(files)} original project files; no examples or build outputs)")
