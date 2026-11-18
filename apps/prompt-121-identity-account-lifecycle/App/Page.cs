public static class Page
{
    public const string Html="""
<!doctype html><meta charset="utf-8"><style>body{margin:0;background:#f4f0fb;color:#21133f;font:28px/1.45 Segoe UI,Arial}main{max-width:1000px;margin:45px auto;background:white;border:2px solid #c8b6e8;border-radius:24px;padding:38px}h1{font-size:46px;line-height:1.15}label{display:block;margin-top:18px;font-weight:600}input{display:block;font:inherit;width:90%;padding:12px;border:2px solid #aaa;border-radius:8px}button,a.button{display:inline-block;font:inherit;background:#6134a7;color:white;border:0;border-radius:10px;padding:15px 28px;margin-top:28px;text-decoration:none}.success{background:#e5f7ec;border-left:8px solid #13a56c;padding:24px}.error{background:#fff0f0;color:#a51530;padding:22px}small{color:#7042a8;font-weight:700}</style>
<main><small>ASP.NET CORE IDENTITY • LOCAL ACCOUNT LAB</small><h1>Sign in to your course account</h1>
<label>Email<input id="email" type="email"></label><label>Password<input id="password" type="password"></label>
<button id="login">Sign in</button><p id="result" aria-live="polite">Use the verified local student account.</p><p id="policy"></p></main>
<script>document.querySelector('#login').onclick=async()=>{const r=await fetch('/login',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({email:document.querySelector('#email').value,password:document.querySelector('#password').value})});const e=document.querySelector('#result');if(r.ok){const me=await fetch('/me');e.className='success';e.textContent='Signed in as '+await me.text();}else{e.className='error';e.textContent='Sign-in rejected: HTTP '+r.status;}};</script>
""";
}
