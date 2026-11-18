const {context}=require('./lab-runtime.cjs'),profile=require('./lesson.cjs');
profile.actions.watch.run(context(__dirname,e=>{if(e.text)process.stdout.write(e.text+'\n');})).catch(e=>{console.error(e.message);process.exitCode=1;}).finally(()=>profile.reset(context(__dirname,()=>{})));
