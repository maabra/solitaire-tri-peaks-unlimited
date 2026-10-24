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
        await page.screenshot(path=str(OUT/'adventure-map.png'),full_page=True)
        assert await page.locator('.story-node').count()==20
        await page.locator('[data-action="wheel"]').click()
        await page.screenshot(path=str(OUT/'lucky-wheel.png'),full_page=True)
        await page.locator('[data-action="spinWheel"]').click()
        await page.wait_for_function("document.querySelector('#wheel-result').textContent.startsWith('You found')")
        assert (await page.evaluate('JSON.parse(localStorage.getItem("meadow-peaks-profile")).WheelSpinsUsed'))==1
        assert await page.locator('[data-action="spinWheel"]').is_disabled()
        await page.locator('[data-action="close"]').click()
        await page.reload()
        await page.locator('[data-action="wheel"]').click()
        assert await page.locator('[data-action="spinWheel"]').is_disabled()
        await page.locator('[data-action="close"]').click()
        await page.locator('[data-action="preview:1"]').click()
        assert 'Clear all cards!' in await page.locator('.level-preview').inner_text()
        await page.screenshot(path=str(OUT/'level-preview.png'),full_page=True)
        await page.locator('[data-action="close"]').click()
        await page.locator('[data-action="home"]').first.click()
        await page.screenshot(path=str(OUT/'home.png'), full_page=True)
        await page.locator('[data-action="continue"]').click()
        assert await page.locator('.tool').count()==3
        await page.screenshot(path=str(OUT/'cards.png'), full_page=True)
        await page.wait_for_timeout(100)
        await page.evaluate('window.stockNode=document.querySelector(\'[data-action="draw"]\');window.cardNodes=new Map([...document.querySelectorAll(".board .card")].map(e=>[e.dataset.action,e]));')
        fixture=json.loads((ROOT/'shared'/'engine-fixtures.json').read_text('utf-8'))['StoryDeals'][0]
        for step in fixture['Deal']['Solution']:
            action='draw' if step['Index']<0 else 'card:'+str(step['Index'])
            if step['Index']<0: await page.wait_for_function('!document.querySelector(\'[data-action="draw"]\').disabled')
            await page.locator('[data-action="'+action+'"]').click()
            assert await page.evaluate('window.stockNode===document.querySelector(\'[data-action="draw"]\')&&[...document.querySelectorAll(".board .card")].every(e=>window.cardNodes.get(e.dataset.action)===e)')
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
        assert await page.locator('#garden-refresh').is_disabled()
        assert '00:' in await page.locator('#garden-refresh').inner_text() or '01:00:00' in await page.locator('#garden-refresh').inner_text()
        await page.screenshot(path=str(OUT/'gems.png'),full_page=True)
        await page.locator('[data-action="settings"]').click()
        await page.locator('[data-setting="ReducedMotion"]').check()
        await page.locator('[data-action="close"]').click()
        await page.reload()
        assert await page.locator('body').evaluate('e=>e.classList.contains("reduced")')
        # The deepest board must stay inside a narrow viewport and remain playable.
        await context.close()
        context=await browser.new_context(viewport={'width':390,'height':844},reduced_motion='reduce')
        page=await context.new_page()
        await page.goto('http://127.0.0.1:8765/')
        deep=await page.evaluate('''async()=>{let {defaults}=await import('./session.mjs');let {generate,Hand}=await import('./core.mjs');let p=defaults();p.Unlocked=2104;p.Active=new Hand(generate(104729+2104*997,2104)).capture(104729+2104*997,2104,false);return p;}''')
        await context.close()
        context=await browser.new_context(viewport={'width':390,'height':844},reduced_motion='reduce')
        await context.add_init_script('localStorage.setItem("meadow-peaks-profile",'+json.dumps(json.dumps(deep))+');')
        page=await context.new_page()
        page.on('pageerror',lambda e: errors.append(str(e)))
        await page.goto('http://127.0.0.1:8765/')
        await page.locator('[data-action="home"]').first.click()
        await page.locator('[data-action="continue"]').click()
        assert 'Level 2,104' in await page.locator('h1').inner_text()
        assert await page.evaluate('document.documentElement.scrollWidth<=innerWidth')
        await page.screenshot(path=str(OUT/'mobile-eight-rows.png'),full_page=True)
        await page.locator('[data-action="guide"]').first.click()
        assert await page.locator('.guide-grid article').count()==13
        assert errors==[], errors
        await context.close()
        context=await browser.new_context(viewport={'width':1280,'height':950},reduced_motion='no-preference')
        page=await context.new_page()
        page.on('pageerror',lambda e:errors.append(str(e)))
        await page.goto('http://127.0.0.1:8765/')
        await page.locator('[data-action="gems"]').first.click()
        await page.locator('[data-action="gemHint"]').click()
        hints=await page.locator('.gem.found').evaluate_all('els=>els.map(e=>e.dataset.action)')
        for action in hints:await page.locator('[data-action="'+action+'"]').click()
        await page.wait_for_selector('.gem.matched')
        assert await page.locator('.gem-links path').count()>0
        assert await page.locator('.gem.matched').count()>=3
        assert '+' in await page.locator('.gem-reward').inner_text()
        await page.screenshot(path=str(OUT/'gem-match-animation.png'),full_page=True)
        await page.wait_for_selector('.gems .gem:enabled',timeout=15000)
        assert (await page.evaluate('JSON.parse(localStorage.getItem("meadow-peaks-profile")).ActiveGarden.Swaps.length'))==1
        await page.reload()
        await page.locator('[data-action="gems"]').first.click()
        assert '19 moves left' in await page.locator('.subhead .meta').inner_text()
        assert await page.locator('#garden-refresh').is_disabled()
        assert errors==[],errors
        await browser.close()
        print('PASS: real-browser win, rewards, gem swap, save restore, settings, guide, and mobile deep board; no JavaScript errors.')

asyncio.run(main())
