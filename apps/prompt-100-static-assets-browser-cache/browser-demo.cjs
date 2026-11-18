const fs=require('fs'),path=require('path');
let modulePath=process.env.PUPPETEER_MODULE;
for(let base=__dirname;!modulePath;){const local=path.join(base,'tools/node_modules/puppeteer-core'),shared=path.join(base,'output/video/lesson-01/tools/node_modules/puppeteer-core');if(fs.existsSync(local))modulePath=local;else if(fs.existsSync(shared))modulePath=shared;const next=path.dirname(base);if(next===base)break;base=next;}
if(!modulePath)throw Error('Run npm install --prefix tools, or set PUPPETEER_MODULE.');
const puppeteer=require(modulePath),address=process.argv[2].replace(/\/$/,''),scenario=process.argv[3];
(async()=>{const browser=await puppeteer.launch({executablePath:process.env.CHROME_PATH||'C:/Program Files/Google/Chrome/Application/chrome.exe',headless:true,args:['--no-sandbox','--disable-gpu']});const errors=[];
try{const page=await browser.newPage();await page.setViewport({width:1280,height:900});page.on('pageerror',e=>errors.push(e.message));fs.mkdirSync('artifacts',{recursive:true});
if(scenario==='mvc'||scenario==='pages'){
 const route=scenario==='mvc'?'/enroll/create':'/Join';await page.goto(address+route,{waitUntil:'networkidle0'});
 await page.type('#Name','Ada Student');await page.$eval('#Seats',e=>e.value='');await page.type('#Seats','0');await Promise.all([page.waitForNavigation({waitUntil:'networkidle0'}),page.click('button[type=submit]')]);
 if(!(await page.content()).includes('Seats must be between 1 and 4.'))throw Error('Validation message missing');await page.screenshot({path:'artifacts/browser-invalid.png'});
 await page.$eval('#Seats',e=>e.value='');await page.type('#Seats','2');await Promise.all([page.waitForNavigation({waitUntil:'networkidle0'}),page.click('button[type=submit]')]);
 if(!(await page.content()).includes('Enrollment confirmed'))throw Error('Success page missing');await page.screenshot({path:'artifacts/browser-success.png'});
 console.log('browser: invalid form redisplayed with its error; valid form redirected to confirmation.');
}else if(scenario==='static'){
 const session=await page.createCDPSession();await session.send('Network.enable');const responses=[],cached=new Set();session.on('Network.requestServedFromCache',e=>cached.add(e.requestId));session.on('Network.responseReceived',e=>responses.push({requestId:e.requestId,url:e.response.url,status:e.response.status,fromDiskCache:e.response.fromDiskCache||false,headers:e.response.headers}));
 await page.goto(address+'/',{waitUntil:'networkidle0'});await page.goto(address+'/second',{waitUntil:'networkidle0'});
 const css=responses.filter(r=>r.url.includes('/css/site.css')).map(r=>({...r,fromBrowserCache:r.fromDiskCache||cached.has(r.requestId)}));fs.writeFileSync('artifacts/browser-network.json',JSON.stringify(css,null,2));if(css.length<2||!css.at(-1).fromBrowserCache)throw Error('Expected the second document to reuse browser-cached CSS');
 await page.evaluate(rows=>{const output=document.querySelector('#diagnostics');output.textContent=rows.map((r,i)=>(i?'Next document':'Initial document')+': HTTP '+r.status+' • browser cache: '+(r.fromBrowserCache?'yes':'no')).concat('Cache-Control: '+(rows.at(-1).headers['Cache-Control']||rows.at(-1).headers['cache-control'])).join('\n');output.style.fontSize='28px';output.style.lineHeight='1.5';},css);
 await page.screenshot({path:'artifacts/browser-success.png'});console.log('browser: second-page CSS response came from the browser cache.');
}
if(errors.length)throw Error(errors.join('; '));fs.writeFileSync('artifacts/browser-ui-verification.json',JSON.stringify({passed:true,scenario,errors},null,2));
}finally{await browser.close();}})().catch(e=>{console.error(e.message);process.exitCode=1});
