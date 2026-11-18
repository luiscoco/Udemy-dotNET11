const fs = require('fs');
const path = require('path');
const assert = require('node:assert/strict');
let puppeteer;
for (let base = __dirname; !puppeteer;) {
    for (const candidate of [path.join(base,'tools/node_modules/puppeteer-core'),path.join(base,'output/video/lesson-01/tools/node_modules/puppeteer-core')]) {
        if (fs.existsSync(candidate)) { puppeteer = require(candidate); break; }
    }
    const parent = path.dirname(base);
    if (parent === base) break;
    base = parent;
}
if (!puppeteer) throw new Error('Install the browser dependency with npm install --prefix tools');
const [address, number, mode] = process.argv.slice(2);
const wait = milliseconds => new Promise(resolve => setTimeout(resolve,milliseconds));
let browser;
let activePage;
const transport = { httpEncodedBytes:0, receivedWebSocketPayloadBytes:0, receivedWebSocketFrames:0 };
const checks = [];
const diagnostics=[];
function check(condition, message) {
    assert.ok(condition,message);
    checks.push(message);
    console.log('PASS '+message);
}
async function text(page, selector) {
    return page.$eval(selector,element=>element.textContent.trim());
}
async function fill(page, selector, value) {
    await page.focus(selector);
    await page.keyboard.down('Control');
    await page.keyboard.press('KeyA');
    await page.keyboard.up('Control');
    await page.type(selector,value);
    await page.keyboard.press('Tab');
}
async function content(page, selector, expected) {
    try {
        await page.waitForFunction((s,t)=>document.querySelector(s)?.textContent.includes(t),{timeout:20000},selector,expected);
    } catch {
        throw new Error('Expected '+selector+' to contain '+expected+'; observed '+await text(page,selector));
    }
}
async function snapshot(page,name) {
    await page.screenshot({path:path.join('artifacts',name+'.png')});
}

async function run(page) {
    await page.goto(address,{waitUntil:'networkidle0',timeout:120000});
    await page.waitForSelector('h1');
    if(number!=='131' && number!=='140')
        await page.waitForFunction(()=>document.querySelector('main')?.dataset.interactive==='ready');
    if (number === '132') {
        await page.waitForSelector('#quantity');
        await fill(page,'#quantity','3');
        await content(page,'#total','75');
        check(await text(page,'#total')==='EUR 75','binding updates the parent-owned quantity and total');
        check((await text(page,'#theme')).includes('Ocean'),'the child receives its cascading theme');
        await page.click('#add');
        await content(page,'#saved','3 seats');
        await snapshot(page,'browser-success');
        check((await text(page,'#saved')).includes('3 seats'),'EventCallback sends the selected value to the parent');
        if(mode==='problem') check((await text(page,'#total')).includes('25'),'incorrect expectation: the parent total ignores the bound quantity');
    } else if (number === '133') {
        await page.waitForSelector('#ready');
        const counts=await (await fetch(address+'/counts')).json();
        check(counts.loads===1,'persistent prerendered data prevents a second service load');
        check(counts.renders>=1,'OnAfterRender runs after interactive rendering');
        await snapshot(page,'browser-success');
        await page.click('#leave');
        await content(page,'h1','Lifecycle summary');
        const after=await(await fetch(address+'/counts')).json();
        check(after.disposed>=1,'navigation disposes the component subscription');
        if(mode==='problem') check(counts.loads===2,'incorrect expectation: prerendering must reload the persisted data');
    } else if(number==='135') {
        await fill(page,'#draft','My .NET course');
        await page.click('#details');
        await content(page,'h1','Course 42');
        check((await text(page,'#draft-value')).includes('My .NET course'),'scoped state survives enhanced in-app navigation');
        check((await text(page,'#level')).includes('advanced'),'query parameters supply the selected filter');
        await snapshot(page,'browser-success');
        await page.reload({waitUntil:'networkidle0'});
        check((await text(page,'#draft-value')).includes('Untitled'),'a full refresh starts a new circuit-scoped draft');
        if(mode==='problem') check((await text(page,'#draft-value')).includes('My .NET course'),'incorrect expectation: circuit-scoped state survives a full refresh');
    } else if(number==='137') {
        await page.waitForSelector('#interop-ready');
        await page.click('#focus-name');
        await page.waitForFunction(()=>document.activeElement?.id==='name');
        check(await page.$eval('#name',e=>e===document.activeElement),'the isolated JavaScript module focuses the requested element');
        await page.setViewport({width:1180,height:860});
        await content(page,'#width','1180');
        await snapshot(page,'browser-success');
        await page.click('#leave');
        await content(page,'h1','Interop summary');
        check(await page.evaluate(()=>window.courseInterop.active)===0,'module cleanup removes the browser resize listener');
        const counters=await(await fetch(address+'/interop-counts')).json();
        check(counters.disposed>=1,'component disposal releases its .NET callback reference');
        if(mode==='problem') check(await page.evaluate(()=>window.courseInterop.active)===1,'incorrect expectation: a disposed component keeps its resize listener');
    } else if(number==='138') {
        await page.waitForSelector('.row');
        await wait(300);
        const before=await page.$$eval('.row',rows=>rows.length);
        check(before<50,'Virtualize bounds the rendered rows for a ten-thousand-item data set');
        const initial=await(await fetch(address+'/metrics')).json();
        await page.$eval('#viewport',element=>element.scrollTop=48000);
        await page.waitForFunction(()=>[...document.querySelectorAll('.row')].some(e=>e.textContent.includes('Course 1000')));
        const after=await(await fetch(address+'/metrics')).json();
        check(after.requests>initial.requests,'scrolling requests another provider window');
        check(after.items<200,'the provider returns bounded slices instead of the full data set');
        const metrics=await page.metrics();
        fs.writeFileSync('artifacts/render-metrics.json',JSON.stringify({initial,after,renderedRows:await page.$$eval('.row',r=>r.length),browser:metrics,transport,scope:'Single local run. HTTP encoded bytes and received WebSocket payload bytes are separate CDP observations.'},null,2));
        console.log('browser: rendered rows '+before+', provider items '+after.items+', received WebSocket payload bytes '+transport.receivedWebSocketPayloadBytes);
        await snapshot(page,'browser-success');
        if(mode==='problem') check(before===10000,'incorrect expectation: virtualization renders every data item');
    } else {
        await scenarios[number](page);
    }
}
const scenarios = {};
scenarios['131'] = async page => {
    await page.goto(address+'/auto',{waitUntil:'networkidle0',timeout:120000});
    await content(page,'#interactive','True');
    const first=await text(page,'#renderer');
    check(/Server|WebAssembly/.test(first),'Auto selects a supported interactive renderer for the current visit');
    await snapshot(page,'browser-auto');
    await page.goto(address+'/static',{waitUntil:'networkidle0'});
    await page.click('#increment');
    await wait(150);
    check((await text(page,'#count')).includes('0'),'static SSR renders HTML without interactive .NET click dispatch');
    await snapshot(page,'browser-rejected');
    if(mode==='problem') check((await text(page,'#count')).includes('1'),'incorrect expectation: a static SSR button executes a .NET click handler');
    await page.goto(address+'/server',{waitUntil:'networkidle0'});
    await content(page,'#renderer','Server');
    await page.click('#increment');
    await content(page,'#count','1');
    check((await text(page,'#count')).includes('1'),'Interactive Server dispatches the click through the server circuit');
    await page.goto(address+'/wasm',{waitUntil:'networkidle0',timeout:120000});
    await content(page,'#renderer','WebAssembly');
    await page.click('#increment');
    await content(page,'#count','1');
    check((await text(page,'#count')).includes('1'),'Interactive WebAssembly runs the component event in the browser');
    await snapshot(page,'browser-wasm');
    await page.goto(address+'/auto',{waitUntil:'networkidle0',timeout:120000});
    await content(page,'#renderer','WebAssembly');
    check((await text(page,'#renderer')).includes('WebAssembly'),'Auto uses the available WebAssembly runtime on the warmed visit');
    fs.writeFileSync('artifacts/render-mode-observations.json',JSON.stringify({firstAuto:first,warmAuto:await text(page,'#renderer')},null,2));
    await snapshot(page,'browser-success');
};
scenarios['134'] = async page => {
    await fill(page,'#username','taken');
    await content(page,'#username-errors','already taken');
    check((await text(page,'#username-errors')).includes('already taken'),'async field validation associates the error with the username');
    await page.click('#submit');
    await content(page,'#result','correct');
    await snapshot(page,'browser-rejected');
    check(!(await text(page,'#result')).includes('Reserved'),'EditForm awaits async validation before accepting submission');
    if(mode==='problem') check((await text(page,'#result')).includes('Reserved'),'incorrect expectation: a taken username passes asynchronous validation');
    await fill(page,'#username','available');
    await page.waitForFunction(()=>document.querySelector('#pending')?.textContent==='Ready');
    await page.click('#submit');
    await content(page,'#result','Reserved available');
    check((await text(page,'#username-errors'))==='','successful validation clears the old field message');
    await snapshot(page,'browser-success');
};
scenarios['136'] = async page => {
    const anonymous=await page.evaluate(async()=> (await fetch('/api/publish',{method:'POST'})).status);
    check(anonymous===401,'the server challenges an anonymous direct action request');
    await page.evaluate(()=>fetch('/fixture/signin/reader',{method:'POST'}));
    await page.reload({waitUntil:'networkidle0'});
    await content(page,'#identity','reader');
    check(await page.$('#publish')===null,'the authenticated reader UI hides the editor control');
    const reader=await page.evaluate(async()=> (await fetch('/api/publish',{method:'POST'})).status);
    check(reader===403,'the server denies a direct reader request even without the UI restriction');
    await snapshot(page,'browser-rejected');
    if(mode==='problem') check(reader===200,'incorrect expectation: a signed-in reader can invoke an editor action');
    await page.evaluate(()=>fetch('/fixture/signin/editor',{method:'POST'}));
    await page.reload({waitUntil:'networkidle0'});
    await page.waitForSelector('#publish');
    const missing=await page.evaluate(async()=> (await fetch('/api/publish',{method:'POST'})).status);
    check(missing===400,'a cookie-authenticated API mutation also requires antiforgery validation');
    const allowed=await page.evaluate(async()=>{
        const {token}=await(await fetch('/csrf')).json();
        return (await fetch('/api/publish',{method:'POST',headers:{'X-CSRF-TOKEN':token}})).status;
    });
    check(allowed===200,'an authorized editor with the request token can invoke the API');
    await page.click('#publish');
    await content(page,'#result','authorized editor');
    check((await text(page,'#result')).includes('authorized editor'),'the server component service authorizes the UI action');
    await snapshot(page,'browser-success');
};
scenarios['139'] = async page => {
    await page.click('#increment');
    await content(page,'#count','1');
    await fill(page,'#draft','Persisted Blazor course');
    const old=await page.$eval('#circuit',e=>e.dataset.circuit);
    await page.evaluate(()=>Blazor.pauseCircuit());
    await wait(300);
    await page.evaluate(()=>Blazor.resumeCircuit());
    await page.waitForFunction(previous=>document.querySelector('#circuit')?.dataset.circuit!==previous,{},old);
    check(await page.$eval('#draft',e=>e.value)==='Persisted Blazor course','a resumed circuit restores the persistent draft property');
    check((await text(page,'#count')).includes('1'),'pause and resume preserve the annotated counter');
    const beforeHidden=await page.$eval('#circuit',e=>e.dataset.circuit);
    const other=await browser.newPage();
    await other.bringToFront();
    await wait(1100);
    await page.bringToFront();
    await page.waitForFunction(previous=>document.querySelector('#circuit')?.dataset.circuit!==previous,{},beforeHidden);
    await other.close();
    check(await page.$eval('#draft',e=>e.value)==='Persisted Blazor course','the opt-in hidden-tab AutoPause resumes the persistent draft');
    await snapshot(page,'browser-success');
    // Exercise the .NET 11 server-triggered pause entry point as well.
    const current=await page.$eval('#circuit',e=>e.dataset.circuit);
    await page.click('#server-pause');
    await wait(400);
    await page.evaluate(()=>Blazor.resumeCircuit());
    await page.waitForFunction(previous=>document.querySelector('#circuit')?.dataset.circuit!==previous,{},current);
    check(await page.$eval('#draft',e=>e.value)==='Persisted Blazor course','server-requested pause also restores the annotated draft');
    await page.evaluate(()=>Blazor.pauseCircuit());
    await wait(5500);
    const result=await page.evaluate(async()=>{try{return await Blazor.resumeCircuit();}catch{return false;}});
    check(result===false,'resume reports failure after the four-second fixture retention expires');
    if(mode==='problem') check(result===true,'incorrect expectation: an expired circuit snapshot always resumes');
    await page.reload({waitUntil:'networkidle0'});
    check(await page.$eval('#draft',e=>e.value)==='Untitled','a full reload initializes a fresh draft after expiry');
    await snapshot(page,'browser-recovery');
};
scenarios['140'] = async page => {
    await page.waitForSelector('#save');
    await page.evaluate(()=>navigator.serviceWorker.ready);
    await page.reload({waitUntil:'networkidle0',timeout:120000});
    await page.waitForSelector('#note');
    await page.waitForFunction(()=>document.querySelector('#save')?.disabled===false);
    check(await page.evaluate(()=>!!navigator.serviceWorker.controller),'the published service worker controls the installed app shell');
    check(await page.$eval('label[for="note"]',e=>e.textContent.length>0),'the note input has an explicit visible label');
    await page.focus('#note');
    await page.keyboard.press('Tab');
    check(await page.evaluate(()=>document.activeElement?.id)==='save','keyboard focus moves from the note to its save action');
    await fill(page,'#note','Study the .NET render modes');
    await page.click('#save');
    await content(page,'#note-status','Saved');
    await page.select('#language','es');
    await content(page,'h1','sin conexión');
    check(await page.$eval('html',e=>e.lang)==='es','Spanish resources update the UI and document language');
    await page.select('#language','en');
    await content(page,'h1','Offline course notebook');
    await page.click('#check');
    await content(page,'#network','Server available');
    await page.setOfflineMode(true);
    const workerSessions=[];
    for(const target of browser.targets().filter(target=>target.type()==='service_worker')) {
        const session=await target.createCDPSession();
        await session.send('Network.enable');
        await session.send('Network.emulateNetworkConditions',{offline:true,latency:0,downloadThroughput:0,uploadThroughput:0});
        workerSessions.push(session);
    }
    await page.reload({waitUntil:'networkidle0',timeout:120000});
    await page.waitForSelector('#note');
    await page.waitForFunction(()=>document.querySelector('#note')?.value==='Study the .NET render modes');
    check(await page.$eval('#note',e=>e.value)==='Study the .NET render modes','the cached WASM shell starts offline and restores the local note');
    await page.click('#check');
    await content(page,'#network','Server unavailable');
    check((await text(page,'#network')).includes('remains on this device'),'the uncached API reports failure while the local note stays available');
    await snapshot(page,'browser-offline');
    if(mode==='problem') check((await text(page,'#network')).includes('Server available'),'incorrect expectation: an uncached server API works offline');
    await page.setOfflineMode(false);
    for(const session of workerSessions)
        await session.send('Network.emulateNetworkConditions',{offline:false,latency:0,downloadThroughput:-1,uploadThroughput:-1});
    await page.select('#language','es');
    await content(page,'h1','sin conexión');
    await page.click('#check');
    await content(page,'#network','Servidor disponible');
    await snapshot(page,'browser-success');
};

async function main() {
    fs.mkdirSync('artifacts',{recursive:true});
    browser=await puppeteer.launch({executablePath:process.env.CHROME_PATH||'C:/Program Files/Google/Chrome/Application/chrome.exe',headless:true,args:['--no-first-run','--disable-extensions'],defaultViewport:{width:1280,height:900}});
    const page=await browser.newPage();
    activePage=page;
    page.setDefaultTimeout(30000);
    const errors=[];
    const client=await page.createCDPSession();
    await client.send('Network.enable');
    client.on('Network.loadingFinished',event=>transport.httpEncodedBytes+=event.encodedDataLength);
    client.on('Network.webSocketFrameReceived',event=>{
        transport.receivedWebSocketFrames++;
        transport.receivedWebSocketPayloadBytes+=Buffer.byteLength(event.response.payloadData,event.response.opcode===2?'base64':'utf8');
    });
    page.on('pageerror',error=>errors.push(error.message));
    page.on('console',message=>{if(message.type()==='error')diagnostics.push(message.text());});
    page.on('requestfailed',request=>diagnostics.push(request.url()+' '+request.failure()?.errorText));
    page.on('response',response=>{if(response.status()>=400)diagnostics.push(response.status()+' '+response.url());});
    await run(page);
    check(errors.length===0,'the browser completed without unhandled page errors');
    fs.writeFileSync('artifacts/browser-ui-verification.json',JSON.stringify({passed:true,number:Number(number),mode,checks,errors},null,2));
    console.log('browser: actual Blazor interaction and screenshot evidence saved');
}
main().catch(async error=>{
    if(browser) {
        const page=activePage;
        await page.screenshot({path:'artifacts/browser-failure.png'}).catch(()=>{});
        fs.writeFileSync('artifacts/browser-failure.txt',await page.$eval('body',e=>e.innerText).catch(()=>''));
        fs.writeFileSync('artifacts/browser-dom.json',JSON.stringify(await page.evaluate(()=>({inputs:[...document.querySelectorAll('input')].map(e=>({id:e.id,value:e.value})),buttons:[...document.querySelectorAll('button')].map(e=>e.outerHTML),storage:localStorage.getItem('course-note')})),null,2));
        fs.writeFileSync('artifacts/browser-diagnostics.json',JSON.stringify(diagnostics,null,2));
    }
    console.error(error.message);
    process.exitCode=1;
}).finally(async()=>{if(browser)await browser.close();});
