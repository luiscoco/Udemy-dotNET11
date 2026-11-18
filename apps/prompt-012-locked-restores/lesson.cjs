const fs=require('fs'),path=require('path');
const central=v=>`<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Course.Invoice.Contracts" Version="${v}" />
  </ItemGroup>
</Project>
`;
async function reset(c){c.write('Directory.Packages.props',central('1.0.0'));if(c.exists('artifacts/baseline.lock.json'))c.write('src/Checkout/packages.lock.json',c.read('artifacts/baseline.lock.json'));}
module.exports={number:12,title:'Invoice contracts / reproducible restores',files:['Directory.Packages.props','NuGet.Config','src/Contracts/Contracts.csproj','src/Contracts/InvoiceSummary.cs','src/Checkout/Checkout.csproj','src/Checkout/Program.cs','src/Checkout/packages.lock.json','EXERCISE.txt'],reset,
actions:{
reset:{label:'Reset inputs',run:reset},
prepare:{label:'1 Prepare packages',run:async c=>{await c.run(['pack','src/Contracts','-c','Release','-o','artifacts/feed','-p:Version=1.0.0']);await c.run(['pack','src/Contracts','-c','Release','-o','artifacts/feed','-p:Version=1.1.0']);c.note('Prepared both local teaching packages.');}},
restore:{label:'2 Restore + lock',run:async c=>{await c.run(['restore','src/Checkout','--force-evaluate']);c.write('artifacts/baseline.lock.json',c.read('src/Checkout/packages.lock.json'));c.check(JSON.parse(c.read('src/Checkout/packages.lock.json')).dependencies['net11.0']['Course.Invoice.Contracts'].resolved==='1.0.0','reviewed lock resolves 1.0.0');}},
locked:{label:'3 Locked restore',run:c=>c.dotnet(['restore','src/Checkout','--locked-mode'])},
run:{label:'4 Run checkout',run:async c=>{await c.run(['build','src/Checkout','-c','Release','--no-restore']);return c.dotnet(['run','--project','src/Checkout','-c','Release','--no-build']);}},
change:{label:'5 Change version',run:c=>{c.write('Directory.Packages.props',central('1.1.0'));c.note('Central version changed to 1.1.0; reviewed lock still records 1.0.0.');}},
update:{label:'6 Review update',run:async c=>{await c.run(['restore','src/Checkout','--force-evaluate']);const lock=JSON.parse(c.read('src/Checkout/packages.lock.json'));c.check(lock.dependencies['net11.0']['Course.Invoice.Contracts'].resolved==='1.1.0','intentional lock update resolves 1.1.0');c.note(c.read('src/Checkout/packages.lock.json'));}},
verify:{label:'7 Verify final',run:async c=>{await c.run(['restore','src/Checkout','--locked-mode']);await c.run(['build','src/Checkout','-c','Release','--no-restore']);return c.dotnet(['run','--project','src/Checkout','-c','Release','--no-build']);}}
},sequence:[{action:'prepare'},{action:'restore',contains:['resolves 1.0.0']},{action:'locked'},{action:'run',contains:['1.0.0.0','EUR 154.68']},{action:'change'},{action:'locked',failure:true,contains:['NU1004']},{action:'update',contains:['resolves 1.1.0']},{action:'verify',contains:['1.1.0.0','EUR 154.68']} ]};
