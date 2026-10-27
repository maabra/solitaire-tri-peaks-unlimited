import asyncio
from pathlib import Path
from playwright.async_api import async_playwright
# Inspect the reference host and its embedded game frames, then capture the launched reference screen.
async def main():
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True);page=await b.new_page(viewport={'width':1280,'height':900});await page.goto('https://www.play123.com/game/solitaire-story-tripeaks',wait_until='domcontentloaded',timeout=45000);await page.wait_for_timeout(2500)
  print('Buttons',await page.get_by_role('button').all_text_contents());print('Frames',[(f.url) for f in page.frames]);print('play',await page.get_by_text('Play now',exact=True).count())
  if await page.get_by_text('Play now',exact=True).count():await page.get_by_text('Play now',exact=True).first.click(timeout=10000)
  await page.wait_for_timeout(8000);print('After frames',[(f.url) for f in page.frames]);Path('artifacts/reference-study').mkdir(exist_ok=True);await page.screenshot(path='artifacts/reference-study/play123-start.png');await b.close()
asyncio.run(main())
