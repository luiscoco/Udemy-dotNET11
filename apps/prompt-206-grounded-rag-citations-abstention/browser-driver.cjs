async function closeOwnedBrowser(browser) {
 const owned = browser.process();
 let timer;
 const closed = await Promise.race([
  browser.close().then(() => true, () => false),
  new Promise(resolve => { timer = setTimeout(() => resolve(false), 15000); })
 ]);
 clearTimeout(timer);
 if (!closed) {
  // This PID belongs to the headless browser launched by this script.
  if (owned && owned.exitCode === null) {
   try { require('child_process').execFileSync('taskkill', ['/PID', String(owned.pid), '/T', '/F'], { windowsHide:true, stdio:'ignore', timeout:5000 }); }
   catch { try { owned.kill(); } catch {} }
  }
  browser.disconnect();
 }
}
const fs=require('fs'),path=require('path');let modulePath=process.env.PUPPETEER_MODULE;
for(let base=__dirname;!modulePath;){for(const p of [path.join(base,'tools/node_modules/puppeteer-core'),path.join(base,'output/video/lesson-01/tools/node_modules/puppeteer-core')])if(fs.existsSync(p))modulePath=p;let next=path.dirname(base);if(next===base)break;base=next;}
if(!modulePath)throw Error('Install tools dependencies or set PUPPETEER_MODULE');
(async()=>{const browser=await require(modulePath).launch({executablePath:process.env.CHROME_PATH||'C:/Program Files/Google/Chrome/Application/chrome.exe',headless:true,pipe:true,userDataDir:path.resolve('artifacts/chrome-profile'),args:['--no-sandbox','--disable-gpu']});
try{const page=await browser.newPage();await page.setViewport({width:1280,height:900});const errors=[];page.on('pageerror',e=>errors.push(e.message));await page.goto(process.argv[2],{waitUntil:'domcontentloaded',timeout:120000});await page.click('#ask');await page.waitForFunction(()=>document.querySelector('#status').textContent==='Answered with evidence',{timeout:120000});const citation=await page.$eval('#citations a',e=>e.textContent);if(!/keys|security/.test(citation))throw Error('Unexpected citation');const resolved=await page.evaluate(async()=>{const link=document.querySelector('#citations a');const response=await fetch(link.getAttribute('href'));const source=await response.json();return response.ok&&source.body.includes(document.querySelector('#answer').textContent.replace(/\.$/,''));});if(!resolved)throw Error('Citation source does not resolve');const forbidden=await page.evaluate(async()=>[await(await fetch('/documents/tenant-b')).status,await(await fetch('/documents/bob-only')).status]);if(forbidden.some(status=>status!==404))throw Error('Restricted source URL exposed');fs.mkdirSync('artifacts',{recursive:true});await page.screenshot({path:'artifacts/browser-success.png'});await page.$eval('#query',e=>e.value='purple moon cheese');await page.click('#ask');await page.waitForFunction(()=>document.querySelector('#status').textContent==='Abstained');if(await page.$('#citations a'))throw Error('Unsupported answer has a citation');await page.screenshot({path:'artifacts/browser-abstained.png'});if(errors.length)throw Error(errors.join(';'));fs.writeFileSync('artifacts/browser-ui-verification.json',JSON.stringify({passed:true,scenario:'actual RAG endpoint with a verified source citation and unsupported-question abstention',citation,abstained:true,citationResolved:true,restrictedSourcesRejected:true},null,2));console.log('RAGUI actual browser verified citation and abstention');}finally{await closeOwnedBrowser(browser);}})().then(()=>process.stdout.write('',()=>process.exit(0)),e=>process.stderr.write(e.message+'\n',()=>process.exit(1)));
