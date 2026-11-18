using System.Text.Encodings.Web;
public static class Page
{
    public static string Html(string nonce)
    {
        var label=HtmlEncoder.Default.Encode("<img src=x onerror=window.injected=true>");
        return $$$"""
<!doctype html><meta charset="utf-8"><style>body{margin:0;background:#f4f0fb;color:#21133f;font:28px/1.45 Segoe UI,Arial}main{max-width:1000px;margin:45px auto;background:white;border:2px solid #c8b6e8;border-radius:24px;padding:38px}h1{font-size:46px;line-height:1.15}label{display:block;margin-top:18px;font-weight:600}input{display:block;font:inherit;width:90%;padding:12px;border:2px solid #aaa;border-radius:8px}button,a.button{display:inline-block;font:inherit;background:#6134a7;color:white;border:0;border-radius:10px;padding:15px 28px;margin-top:28px;text-decoration:none}.success{background:#e5f7ec;border-left:8px solid #13a56c;padding:24px}.error{background:#fff0f0;color:#a51530;padding:22px}small{color:#7042a8;font-weight:700}</style><main><small>COOKIE-BACKED APP • REAL REQUEST PROTECTION</small><h1>Protect the course update</h1>
<p>Student-supplied display text: {{{label}}}</p><button id="login">Sign in to local account</button>
<button id="unsafe-post">Post without request token</button><button id="safe-post">Post with request token</button><div id="result" aria-live="polite">Sign in before updating.</div></main>
<script nonce="{{{nonce}}}">
document.querySelector('#login').onclick=async()=>{const r=await fetch('/login',{method:'POST'});document.querySelector('#result').textContent=r.ok?'Signed in':'Sign-in failed';};
document.querySelector('#unsafe-post').onclick=async()=>{const r=await fetch('/update',{method:'POST'});const e=document.querySelector('#result');e.className='error';e.textContent='Missing antiforgery token: HTTP '+r.status;};
document.querySelector('#safe-post').onclick=async()=>{const t=await(await fetch('/token')).json();const r=await fetch('/update',{method:'POST',headers:{'X-CSRF-TOKEN':t.token}});const e=document.querySelector('#result');e.className='success';e.textContent='Protected update: HTTP '+r.status;};
</script>
""";
    }
}
