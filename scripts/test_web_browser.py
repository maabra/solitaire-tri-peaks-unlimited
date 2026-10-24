"""Optional local UI check: pip install playwright; playwright install chromium."""
import asyncio
import json
from pathlib import Path
from playwright.async_api import async_playwright

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'artifacts' / 'web-screenshots'
OUT.mkdir(parents=True, exist_ok=True)

async def main():
    async with async_playwright() as p:
        browser = await p.chromium.launch(channel='msedge', headless=True)
        context = await browser.new_context(viewport={'width':1280, 'height':950}, reduced_motion='reduce')
        page = await context.new_page()
        errors=[]
        page.on('pageerror', lambda e: errors.append(str(e)))
        await page.goto('http://127.0.0.1:8765/', wait_until='networkidle')
        await page.screenshot(path=str(OUT/'home.png'), full_page=True)
        await page.locator('[data-action="continue"]').click()
        await page.screenshot(path=str(OUT/'cards.png'), full_page=True)
        fixture=json.loads((ROOT/'shared'/'engine-fixtures.json').read_text('utf-8'))['Deals'][0]
        for step in fixture['Deal']['Solution']:
            action='draw' if step['Index']<0 else 'card:'+str(step['Index'])
            await page.locator('[data-action="'+action+'"]').click()
        await page.wait_for_selector('dialog[open] .victory')
        assert await page.locator('.victory').inner_text() and 'Beautifully' in await page.locator('.victory').inner_text()
        saved=await page.evaluate('JSON.parse(localStorage.getItem("meadow-peaks-profile"))')
        assert saved['Unlocked']==2 and saved['Coins']>300
        await page.screenshot(path=str(OUT/'victory.png'),full_page=True)
        await page.locator('[data-action="home"]').last.click()
        await page.locator('[data-action="gems"]').first.click()
        await page.locator('[data-action="gemHint"]').click()
        hints=await page.locator('.gem.found').evaluate_all('els=>els.map(e=>e.dataset.action)')
        assert len(hints)==2
        for action in hints: await page.locator('[data-action="'+action+'"]').click()
        await page.wait_for_timeout(350)
        assert (await page.evaluate('JSON.parse(localStorage.getItem("meadow-peaks-profile")).Coins'))>saved['Coins']
        await page.screenshot(path=str(OUT/'gems.png'),full_page=True)
        await page.locator('[data-action="settings"]').click()
        await page.locator('[data-setting="ReducedMotion"]').check()
        await page.locator('[data-action="close"]').click()
        await page.reload()
        assert await page.locator('body').evaluate('e=>e.classList.contains("reduced")')
        # The deepest board must stay inside a narrow viewport and remain playable.
        await page.evaluate('''async()=>{let {defaults}=await import('./session.mjs');let {generate,Hand}=await import('./core.mjs');let p=defaults();p.Unlocked=2101;p.Active=new Hand(generate(104729+2101*997,2101)).capture(104729+2101*997,2101,false);localStorage.setItem('meadow-peaks-profile',JSON.stringify(p));}''')
        await page.set_viewport_size({'width':390,'height':844})
        await page.reload()
        await page.locator('[data-action="continue"]').click()
        assert await page.evaluate('document.documentElement.scrollWidth<=innerWidth')
        await page.screenshot(path=str(OUT/'mobile-eight-rows.png'),full_page=True)
        await page.locator('[data-action="guide"]').first.click()
        assert await page.locator('.guide-grid article').count()==13
        assert errors==[], errors
        await browser.close()
        print('PASS: real-browser win, rewards, gem swap, save restore, settings, guide, and mobile deep board; no JavaScript errors.')

asyncio.run(main())
