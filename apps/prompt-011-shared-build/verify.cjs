const fs=require('fs'),path=require('path'),{spawn}=require('child_process');
const root=__dirname,out=path.join(root,'artifacts'),workspace=path.resolve(root,'../..');
const local=path.join(workspace,'.tools/dotnet-11/sdk/dotnet.exe');
const dotnet=process.env.DOTNET_EXE||(fs.existsSync(local)?local:'dotnet');
const env={...process.env,DOTNET_CLI_UI_LANGUAGE:'en-US',VSLANG:'1033',DOTNET_NOLOGO:'1',DOTNET_CLI_TELEMETRY_OPTOUT:'1',DOTNET_SKIP_FIRST_TIME_EXPERIENCE:'1',DOTNET_CLI_HOME:path.join(out,'.cli-home'),NUGET_PACKAGES:path.join(out,'.nuget')};
if(path.isAbsolute(dotnet))env.DOTNET_ROOT=path.dirname(dotnet);
fs.mkdirSync(out,{recursive:true});
const steps=[],assertions=[];
const assert=(condition,name)=>{assertions.push({name,passed:!!condition});if(!condition)throw new Error(name);};
async function run(id,args,expectFailure=false){
 const started=Date.now(),chunks=[];let stdout='',stderr='';
 const exitCode=await new Promise((resolve,reject)=>{const p=spawn(dotnet,args,{cwd:root,env,windowsHide:true});for(const [name,stream]of [['stdout',p.stdout],['stderr',p.stderr]])stream.on('data',data=>{const text=data.toString();chunks.push({stream:name,ms:Date.now()-started,text});if(name==='stdout')stdout+=text;else stderr+=text;});p.on('error',reject);p.on('close',resolve);});
 const result={id,command:'dotnet '+args.join(' '),exitCode,elapsedMs:Date.now()-started,stdout,stderr,chunks};steps.push(result);
 fs.writeFileSync(path.join(out,id+'.txt'),result.command+'\n\n'+stdout+stderr+'\nExit code: '+exitCode+'\n');
 assert(expectFailure?exitCode!==0:exitCode===0,id+' expected exit status');
 console.log(id+': exit '+exitCode+(expectFailure?' (expected failure)':''));return stdout+stderr;
}
const props=path.join(root,'src/Reporting/Directory.Build.props'),domain=path.join(root,'src/Billing.Core/Invoice.cs');
const original=fs.readFileSync(domain,'utf8');let ownNested=false;
async function main(){
 assert(!fs.existsSync(props),'exercise starts without an existing nested props file');
 const sdk=(await run('01-sdk',['--version'])).trim();assert(sdk.startsWith('11.0.'),'executed using the .NET 11 SDK');
 await run('02-restore',['restore','InvoiceSuite.slnx','--configfile','NuGet.Config']);
 await run('03-baseline-build',['build','InvoiceSuite.slnx','-c','Release','-t:Rebuild','--no-restore','-bl:artifacts/baseline.binlog']);
 const checkout=await run('04-checkout',['run','--project','src/Checkout','-c','Release','--no-build']);
 assert(checkout.includes('TOTAL:        EUR 154.68')&&checkout.includes('region: EU'),'checkout receipt and inherited EU region');
 const report=await run('05-reporting',['run','--project','src/Reporting','-c','Release','--no-build']);
 assert(report.includes('Gross sales:  EUR 454.68')&&report.includes('region: US'),'report total and project-level US override');
 const query=['msbuild','src/Reporting/Reporting.csproj','-nologo','-getProperty:Nullable,ImplicitUsings,TrainingRegion'];
 const before=JSON.parse(await run('06-properties-baseline',query));assert(before.Properties.Nullable==='enable'&&before.Properties.ImplicitUsings==='enable'&&before.Properties.TrainingRegion==='US','evaluated reporting defaults and override');
 const broken='<Project>\n  <PropertyGroup>\n    <TrainingRegion>LOCAL</TrainingRegion>\n  </PropertyGroup>\n</Project>\n';
 ownNested=true;fs.writeFileSync(props,broken);fs.writeFileSync(path.join(out,'nested-broken.props'),broken);
 const missing=JSON.parse(await run('07-properties-broken',query));assert(missing.Properties.Nullable===''&&missing.Properties.ImplicitUsings==='','nearest nested props hid the parent defaults');
 const failure=await run('08-nested-failure',['build','src/Reporting','-c','Release','-t:Rebuild','--no-restore'],true);assert(failure.includes('CS0103'),'missing implicit usings causes a real compiler failure');
 const fixed='<Project>\n  <Import Project="../../Directory.Build.props" />\n  <PropertyGroup>\n    <TrainingRegion>LOCAL</TrainingRegion>\n  </PropertyGroup>\n</Project>\n';
 fs.writeFileSync(props,fixed);fs.writeFileSync(path.join(out,'nested-fixed.props'),fixed);
 const restored=JSON.parse(await run('09-properties-fixed',query));assert(restored.Properties.Nullable==='enable'&&restored.Properties.ImplicitUsings==='enable'&&restored.Properties.TrainingRegion==='US','explicit parent import restored defaults; project still overrides region');
 await run('10-fixed-build',['build','src/Reporting','-c','Release','-t:Rebuild','--no-restore']);
 await run('11-fixed-report',['run','--project','src/Reporting','-c','Release','--no-build']);
 fs.unlinkSync(props);ownNested=false;
 const bad=original.replace('customer?.Length ?? 0','customer.Length');assert(bad!==original,'nullable mutation applied');fs.writeFileSync(domain,bad);
 const nullable=await run('12-nullable-failure',['build','src/Checkout','-c','Release','-t:Rebuild','--no-restore'],true);assert(nullable.includes('error CS8602'),'selected null dereference diagnostic blocks the build');
 fs.writeFileSync(domain,original);
 await run('13-final-build',['build','InvoiceSuite.slnx','-c','Release','-t:Rebuild','--no-restore']);
 const checks=await run('14-domain-checks',['run','--project','tests/Billing.Checks','-c','Release','--no-build']);assert(checks.includes('RESULT: 8 checks passed'),'eight behavioral checks pass');
 fs.writeFileSync(path.join(out,'verification.json'),JSON.stringify({passed:true,sdk,assertions,steps,restoredSource:true,notes:['No external packages or test runner; Billing.Checks is a console assertion runner.','Tax is a fixed 20% teaching rule. Region is build metadata, not a tax jurisdiction.']},null,2));
 console.log('PASS: runnable pilot, both deliberate failures, recovery, and eight domain checks.');
}
main().catch(e=>{fs.writeFileSync(path.join(out,'verification-failed.json'),JSON.stringify({passed:false,error:e.message,assertions,steps},null,2));console.error(e.message);process.exitCode=1;}).finally(()=>{fs.writeFileSync(domain,original);if(ownNested&&fs.existsSync(props))fs.unlinkSync(props);});
