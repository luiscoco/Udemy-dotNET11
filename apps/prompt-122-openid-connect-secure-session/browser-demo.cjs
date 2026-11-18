const fs=require('fs'),path=require('path');let modulePath=process.env.PUPPETEER_MODULE;
for(let base=__dirname;!modulePath;){for(const candidate of[path.join(base,'tools/node_modules/puppeteer-core'),path.join(base,'output/video/lesson-01/tools/node_modules/puppeteer-core')])if(fs.existsSync(candidate))modulePath=candidate;const next=path.dirname(base);if(next===base)break;base=next;}
if(!modulePath)throw Error('Run npm install --prefix tools or set PUPPETEER_MODULE');
const puppeteer=require(modulePath),address=process.argv[2].replace(/\/$/,''),scenario=process.argv[3],pin=process.argv[4];
(async()=>{const browser=await puppeteer.launch({executablePath:process.env.CHROME_PATH||'C:/Program Files/Google/Chrome/Application/chrome.exe',headless:true,args:['--no-sandbox','--disable-gpu','--ignore-certificate-errors-spki-list='+pin]});const errors=[];
try{const page=await browser.newPage();await page.setViewport({width:1280,height:900});page.on('pageerror',e=>errors.push(e.message));fs.mkdirSync('artifacts',{recursive:true});
if(scenario==='identity'){
 await page.goto(address,{waitUntil:'networkidle0'});await page.type('#email','student@example.test');await page.type('#password','Changed_11!Password');await page.click('#login');await page.waitForFunction(()=>document.querySelector('#result').textContent.includes('Signed in as'));
 const cookies=await page.cookies();const session=cookies.find(x=>x.name==='__Host-CourseSession');if(!session?.secure||!session.httpOnly||session.sameSite!=='Lax')throw Error('Cookie policy mismatch');
 if(await page.evaluate(()=>document.cookie.includes('__Host-CourseSession')))throw Error('HttpOnly session exposed to script');
 await page.$eval('#policy',e=>e.textContent='Observed session: Secure • HttpOnly • SameSite=Lax');await page.screenshot({path:'artifacts/browser-success.png'});
 console.log('browser: actual Identity login succeeded with a Secure, HttpOnly session cookie.');
}else if(scenario==='oidc'){
 const authorization=[];page.on('request',r=>{if(r.url().includes('/protocol/openid-connect/auth'))authorization.push(r.url());});
 await page.goto(address,{waitUntil:'networkidle0'});await Promise.all([page.waitForNavigation({waitUntil:'networkidle0'}),page.click('#login')]);
 await page.waitForSelector('#username');await page.type('#username','student');await page.type('#password','Student_11!Local');await Promise.all([page.waitForNavigation({waitUntil:'networkidle0'}),page.click('#kc-login')]);
 await page.waitForSelector('#signed-in');
 const url=new URL(authorization[0]);if(url.searchParams.get('code_challenge_method')!=='S256'||!url.searchParams.has('state')||!url.searchParams.has('nonce'))throw Error('OIDC challenge is missing protections');
 const cookies=await page.cookies();const session=cookies.find(x=>x.name==='__Host-OidcSession');if(!session?.secure||!session.httpOnly)throw Error('OIDC cookie policy mismatch');
 if(new URL(page.url()).searchParams.has('code')||cookies.some(x=>x.value.includes('eyJ')))throw Error('Unexpected token exposure');
 await page.$eval('#policy',e=>e.textContent='Observed flow: code + S256 PKCE • state + nonce • Secure HttpOnly session');await page.screenshot({path:'artifacts/browser-success.png'});
 const bad=await page.evaluate(async()=>{const r=await fetch('/signin-oidc?code=fake&state=invalid',{redirect:'manual'});return r.status;});if(bad!==400)throw Error('Forged callback accepted');
 await page.$eval('#signed-in',e=>e.textContent='Forged callback rejected: HTTP 400');await page.screenshot({path:'artifacts/browser-rejected.png'});
 const logout=await page.evaluate(async()=>{const token=document.querySelector('input[name=csrf]').value;const r=await fetch('/logout',{method:'POST',headers:{'X-CSRF-TOKEN':token}});const me=await fetch('/me');return [r.status,me.status];});
 if(logout[0]!==204||logout[1]!==401)throw Error('Local RP session sign-out failed');
 console.log('browser: Keycloak code login with S256 PKCE succeeded; forged callback rejected; cookie flags and local session sign-out verified.');
}else if(scenario==='antiforgery'){
 await page.goto(address,{waitUntil:'networkidle0'});await page.click('#login');await page.waitForFunction(()=>document.querySelector('#result').textContent.includes('Signed in'));
 await page.click('#unsafe-post');await page.waitForFunction(()=>document.querySelector('#result').textContent.includes('Missing antiforgery token: HTTP 400'));await page.screenshot({path:'artifacts/browser-rejected.png'});
 await page.click('#safe-post');await page.waitForFunction(()=>document.querySelector('#result').textContent.includes('Protected update: HTTP 200'));
 if(await page.evaluate(()=>Boolean(window.injected)))throw Error('Untrusted markup executed');await page.screenshot({path:'artifacts/browser-success.png'});
 console.log('browser: missing antiforgery token rejected; token-protected update succeeded; untrusted markup remained text.');
}else throw Error('Unknown scenario');
if(errors.length)throw Error(errors.join('; '));fs.writeFileSync('artifacts/browser-ui-verification.json',JSON.stringify({passed:true,scenario,errors},null,2));
}finally{await browser.close();}})().catch(e=>{console.error(e);process.exitCode=1});
