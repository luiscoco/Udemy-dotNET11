const fs=require('fs'),path=require('path'),http=require('http'),{spawn}=require('child_process');
const root=__dirname,workspace=path.resolve(root,'../..');
const files=['Directory.Build.props','Directory.Build.targets','src/Billing.Core/Invoice.cs','src/Checkout/Checkout.csproj','src/Checkout/Program.cs','src/Reporting/Reporting.csproj','src/Reporting/Program.cs','src/Reporting/Directory.Build.props','tests/Billing.Checks/Program.cs','EXERCISE.txt'];
const broken='<Project>\n  <PropertyGroup>\n    <TrainingRegion>LOCAL</TrainingRegion>\n  </PropertyGroup>\n</Project>\n';
const fixed='<Project>\n  <Import Project="../../Directory.Build.props" />\n  <PropertyGroup>\n    <TrainingRegion>LOCAL</TrainingRegion>\n  </PropertyGroup>\n</Project>\n';
const nested=path.join(root,'src/Reporting/Directory.Build.props'),domain=path.join(root,'src/Billing.Core/Invoice.cs');
function writeNested(value){if(fs.existsSync(nested)&&![broken,fixed].includes(fs.readFileSync(nested,'utf8')))throw new Error('Existing nested props has custom edits; preserve it before the lab.');fs.writeFileSync(nested,value);}
function reset(){if(fs.existsSync(nested)){if(![broken,fixed].includes(fs.readFileSync(nested,'utf8')))throw new Error('Custom nested props will not be removed.');fs.unlinkSync(nested);}const current=fs.readFileSync(domain,'utf8');if(current.includes('=> customer.Length;'))fs.writeFileSync(domain,current.replace('=> customer.Length;','=> customer?.Length ?? 0;'));}
function createLabServer(){
 let busy=false;const traces=[];
 const local=path.join(workspace,'.tools/dotnet-11/sdk/dotnet.exe'),dotnet=process.env.DOTNET_EXE||(fs.existsSync(local)?local:'dotnet');
 const env={...process.env,DOTNET_CLI_UI_LANGUAGE:'en-US',VSLANG:'1033',DOTNET_NOLOGO:'1',DOTNET_CLI_TELEMETRY_OPTOUT:'1',DOTNET_CLI_HOME:path.join(root,'artifacts/.cli-home'),NUGET_PACKAGES:path.join(root,'artifacts/.nuget')};
 if(path.isAbsolute(dotnet))env.DOTNET_ROOT=path.dirname(dotnet);
 const normalize=s=>s.replaceAll(root,'<repo>').replaceAll(root.replaceAll('\\','/'),'<repo>').replaceAll(path.dirname(dotnet),'<sdk>');
 const server=http.createServer(async(req,res)=>{
  const url=new URL(req.url,'http://127.0.0.1');
  if(req.method==='GET'&&url.pathname==='/brand.png'){res.writeHead(200,{'Content-Type':'image/png'});res.end(fs.readFileSync(path.join(root,'assets/brand-reference.png')));return;}
  if(req.method==='GET'&&url.pathname==='/'){res.writeHead(200,{'Content-Type':'text/html; charset=utf-8'});res.end(fs.readFileSync(path.join(root,'lab.html')));return;}
  if(req.method==='GET'&&url.pathname==='/api/file'){
   const file=url.searchParams.get('name');if(!files.includes(file)){res.writeHead(404);res.end();return;}
   res.writeHead(200,{'Content-Type':'application/json'});res.end(JSON.stringify({file,text:fs.existsSync(path.join(root,file))?fs.readFileSync(path.join(root,file),'utf8'):'// This file does not exist in the baseline. Use the nested-props exercise.'}));return;
  }
  if(req.method!=='POST'||url.pathname!=='/api/action'){res.writeHead(404);res.end();return;}
  if(req.headers.origin&&req.headers.origin!==`http://${req.headers.host}`){res.writeHead(403);res.end();return;}
  if(busy){res.writeHead(409);res.end('A lab command is already running.');return;}
  let body='';for await(const chunk of req){body+=chunk;if(body.length>2048){res.writeHead(413);res.end();return;}}
  let action;try{action=JSON.parse(body).action;}catch{res.writeHead(400);res.end();return;}
  const commands={build:[['build','InvoiceSuite.slnx','-c','Release','-t:Rebuild']],checkout:[['run','--project','src/Checkout','-c','Release','--no-build']],report:[['run','--project','src/Reporting','-c','Release','--no-build']],propertiesCheckout:[['msbuild','src/Checkout/Checkout.csproj','-nologo','-getProperty:Nullable,ImplicitUsings,TrainingRegion']],propertiesReport:[['msbuild','src/Reporting/Reporting.csproj','-nologo','-getProperty:Nullable,ImplicitUsings,TrainingRegion']],buildReport:[['build','src/Reporting','-c','Release','-t:Rebuild']],buildCheckout:[['build','src/Checkout','-c','Release','-t:Rebuild']],checks:[['build','InvoiceSuite.slnx','-c','Release'],['run','--project','tests/Billing.Checks','-c','Release','--no-build']]};
  if(!Object.hasOwn(commands,action)&&!['breakProps','fixProps','unsafeNullable','reset'].includes(action)){res.writeHead(400);res.end('Unknown lesson action');return;}
  busy=true;res.writeHead(200,{'Content-Type':'application/x-ndjson','Cache-Control':'no-store'});
  const started=Date.now(),trace={action,started:new Date().toISOString(),events:[]};
  const send=event=>{trace.events.push({...event,ms:Date.now()-started});res.write(JSON.stringify(event)+'\n');};
  try{
   if(action==='breakProps'){writeNested(broken);send({text:'Created nested Directory.Build.props WITHOUT a parent import.\n'});}
   else if(action==='fixProps'){writeNested(fixed);send({text:'Added the explicit parent import to the nested props file.\n'});}
   else if(action==='unsafeNullable'){const s=fs.readFileSync(domain,'utf8');if(!s.includes('=> customer?.Length ?? 0;'))throw new Error('Expected guarded source was not found.');fs.writeFileSync(domain,s.replace('=> customer?.Length ?? 0;','=> customer.Length;'));send({text:'Changed the optional-name helper to dereference customer without a guard.\n'});}
   else if(action==='reset'){reset();send({text:'Restored the guarded helper and removed the lab-created nested props file.\n'});}
   let exitCode=0;
   for(const args of commands[action]||[]){send({command:'dotnet '+args.join(' ')});exitCode=await new Promise((resolve,reject)=>{const p=spawn(dotnet,args,{cwd:root,env,windowsHide:true});for(const stream of[p.stdout,p.stderr])stream.on('data',data=>send({text:normalize(data.toString())}));p.on('error',reject);p.on('close',resolve);});if(exitCode!==0)break;}
   send({done:true,exitCode});trace.exitCode=exitCode;
  }catch(e){send({text:e.message+'\n',done:true,exitCode:-1});trace.exitCode=-1;}
  finally{busy=false;traces.push(trace);fs.mkdirSync(path.join(root,'artifacts/browser'),{recursive:true});fs.writeFileSync(path.join(root,'artifacts/browser',String(traces.length).padStart(2,'0')+'-'+action+'.json'),JSON.stringify(trace,null,2));res.end();}
 });
 return{server,reset,traces};
}
module.exports={createLabServer,files};
if(require.main===module){const lab=createLabServer();lab.server.listen(4177,'127.0.0.1',()=>console.log('InvoiceSuite lab: http://127.0.0.1:4177 — requires .NET 11.0.100 or a compatible patch.'));process.on('SIGINT',()=>{try{lab.reset();}finally{lab.server.close(()=>process.exit());}});}
