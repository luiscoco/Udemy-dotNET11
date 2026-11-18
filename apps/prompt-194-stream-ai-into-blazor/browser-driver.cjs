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
for(let base=__dirname;!modulePath;){for(const c of [path.join(base,'tools/node_modules/puppeteer-core'),path.join(base,'output/video/lesson-01/tools/node_modules/puppeteer-core')])if(fs.existsSync(c))modulePath=c;const next=path.dirname(base);if(next===base)break;base=next;}
if(!modulePath)throw Error('Run npm install --prefix tools or set PUPPETEER_MODULE');
const puppeteer=require(modulePath),url=process.argv[2],scenario=process.argv[3];
(async()=>{const browser=await puppeteer.launch({executablePath:process.env.CHROME_PATH||'C:/Program Files/Google/Chrome/Application/chrome.exe',headless:true,pipe:true,timeout:60000,protocolTimeout:600000,userDataDir:path.resolve('artifacts/chrome-profile'),args:['--no-sandbox','--disable-gpu']});
try{const page=await browser.newPage();await page.setViewport({width:1280,height:900});const network=[];page.on('request',r=>network.push({type:'request',url:r.url()}));page.on('response',r=>network.push({type:'response',url:r.url(),status:r.status()}));page.on('requestfailed',r=>network.push({type:'failed',url:r.url(),error:r.failure()?.errorText}));const errors=[];page.on('pageerror',e=>{errors.push(e.message);console.log('browser: '+e.message);});page.on('console',m=>{if(m.type()==='error')console.log('browser: '+m.text());});page.on('response',r=>{if(r.status()>=400)console.log('browser: HTTP '+r.status()+' '+r.url());});
try{await page.goto(url,{waitUntil:'domcontentloaded',timeout:60000});}catch(e){fs.mkdirSync('artifacts',{recursive:true});fs.writeFileSync('artifacts/browser-navigation-failure.json',JSON.stringify(network,null,2));console.log('browser: last network events '+JSON.stringify(network.slice(-10)));throw e;}try{await page.waitForFunction(()=>document.querySelector('#ask')?.disabled===false,{timeout:60000});}catch(e){fs.mkdirSync('artifacts',{recursive:true});await page.screenshot({path:'artifacts/browser-debug.png'});console.log('browser: '+(await page.content()).slice(-6000));throw e;}
await page.evaluate(()=>{window.observed=[];window.poll=setInterval(()=>{const n=Number(document.querySelector('#updates').textContent);if(n>0&&!window.observed.some(o=>o.updates===n))window.observed.push({updates:n,status:document.querySelector('#status').textContent,characters:document.querySelector('#answer').textContent.length,ms:performance.now()});},15);});
await page.click('#ask');await page.waitForFunction(()=>document.querySelector('#status').textContent==='Complete',{timeout:120000});
const result=await page.evaluate(()=>{clearInterval(window.poll);return {updates:Number(document.querySelector('#updates').textContent),text:document.querySelector('#answer').textContent,observed:window.observed};});
fs.mkdirSync('artifacts',{recursive:true});await page.screenshot({path:'artifacts/browser-'+(scenario==='streaming'?'success':'buffered')+'.png'});
const passed=result.updates>1&&result.text.length>0&&errors.length===0;
const report={passed,scenario,actualBlazor:true,actualModel:true,...result,errors};
fs.writeFileSync('artifacts/browser-'+scenario+'-verification.json',JSON.stringify(report,null,2));
if(scenario==='streaming')fs.writeFileSync('artifacts/browser-ui-verification.json',JSON.stringify(report,null,2));
console.log('STREAM actual Blazor updates='+result.updates+' characters='+result.text.length+' incremental='+passed);
if(!passed)throw Error('AI streaming response was buffered');
}finally{await closeOwnedBrowser(browser);}})().then(()=>process.stdout.write('',()=>process.exit(0)),e=>process.stderr.write(e.message+'\n',()=>process.exit(1)));
