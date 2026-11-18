const fs=require('fs'),path=require('path');
async function main(){
    await require('./lab-runtime.cjs').verify(__dirname);
    const file=path.join(__dirname,'artifacts/verification.json');
    const report=JSON.parse(fs.readFileSync(file));
    for(const [action,expected] of [['timeout',3],['limit',13]]){
        const step=report.steps.find(item=>item.action===action);
        if(step.exitCode!==expected){report.passed=false;fs.writeFileSync(file,JSON.stringify(report,null,2));throw new Error(action+' returned '+step.exitCode+' instead of '+expected);}
    }
    console.log('PASS SDK timeout exit 3 and failure-limit exit 13');
}
main().catch(error=>{console.error(error.message);process.exitCode=1});
