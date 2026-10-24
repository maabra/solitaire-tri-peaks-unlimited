"""Package only the original static browser game for alternate hosting."""
from pathlib import Path
from zipfile import ZipFile,ZIP_DEFLATED
root=Path(__file__).resolve().parents[1]
web=root/'web'
files=[web/'index.html',web/'style.css',web/'ui-theme.css',web/'icon.svg',*web.glob('*.mjs'),*web.joinpath('assets').rglob('*')]
output=root/'artifacts'/'MeadowPeaks-web.zip'
output.parent.mkdir(exist_ok=True)
with ZipFile(output,'w',ZIP_DEFLATED) as archive:
    for file in files:
        if file.is_file():archive.write(file,file.relative_to(web).as_posix())
    archive.write(root/'LICENSE','LICENSE.txt')
print('Browser package: '+str(output))
