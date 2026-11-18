require('./lab-runtime.cjs').verify(__dirname).catch(e=>{console.error(e.message);process.exitCode=1});
