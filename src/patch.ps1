$ErrorActionPreference = 'Stop'
$root = 'C:\Users\NITRO PC\Desktop\NATRIA'
$path = Join-Path $root 'index.html'
$t = [System.IO.File]::ReadAllText($path)

# ---- brand colors: purple/cyan -> natria blue ----
$pairs = @(
  @('124,92,255','58,151,255'),
  @('77,225,255','143,214,255'),
  @('#7c5cff','#3a97ff'),
  @('#4de1ff','#8fd6ff'),
  @('#8b6bff','#5aa7ff'),
  @('#a68bff','#a8d4ff'),
  @('#6b4af0','#2f7be8'),
  @('#3b28ac','#164a99'),
  @('#b9a7ff','#9cc9ff')
)
foreach ($pair in $pairs) { $t = $t.Replace($pair[0], $pair[1]) }

# ---- head: real favicons + manifest + og:image ----
$headLinks = @'
<link rel="icon" href="assets/favicon.ico" sizes="any">
<link rel="icon" type="image/png" sizes="96x96" href="assets/favicon-96.png">
<link rel="icon" type="image/png" sizes="192x192" href="assets/favicon-192.png">
<link rel="icon" type="image/png" sizes="256x256" href="assets/favicon-256.png">
<link rel="apple-touch-icon" sizes="180x180" href="assets/apple-touch-icon.png">
'@
$t = [regex]::Replace($t, '(?m)^<link rel="icon"[^\r\n]*', $headLinks)
$t = [regex]::Replace($t, '(<meta name="description"[^>]*>)', '$1' + "`r`n" + '<link rel="manifest" href="assets/site.webmanifest">' + "`r`n" + '<meta property="og:image" content="assets/logo.png">')

# ---- nav logo css ----
$t = [regex]::Replace($t, '\.brand-mark\{[^\r\n]*\}', ".brand-mark{width:38px;height:38px;display:grid;place-items:center;flex-shrink:0}`r`n.brand-mark img{width:100%;height:100%;object-fit:contain;filter:drop-shadow(0 0 10px rgba(58,151,255,.45))}")

# ---- app brand dot css ----
$t = [regex]::Replace($t, '\.app-dot\{[^\r\n]*\}', '.app-logo{width:18px;height:18px;object-fit:contain;filter:drop-shadow(0 0 6px rgba(58,151,255,.7))}')

# ---- power block css -> atom image ----
$newCss = @'
.power-wrap{position:relative;height:230px;display:grid;place-items:center;margin-top:2px}
.atom-glow{position:absolute;width:250px;height:250px;border-radius:50%;background:radial-gradient(circle,rgba(58,151,255,.35),rgba(143,214,255,.12) 55%,transparent 72%);filter:blur(14px);animation:atomGlow 4.5s ease-in-out infinite}
.atom{position:relative;width:196px;height:196px;filter:drop-shadow(0 0 26px rgba(58,151,255,.5));animation:atomFloat 6.5s ease-in-out infinite}
@keyframes atomFloat{0%,100%{transform:translateY(0) rotate(-2deg)}50%{transform:translateY(-9px) rotate(2deg)}}
@keyframes atomGlow{0%,100%{opacity:.75;transform:scale(1)}50%{opacity:1;transform:scale(1.06)}}
'@
$t = [regex]::Replace($t, '(?s)\.power-wrap\{.*?@keyframes pulsePower\{[^\r\n]*\}', $newCss.TrimEnd() + "`r`n")

# ---- banner atom css ----
$t = $t.Replace('.banner h2{position:relative}', ".banner-atom{position:absolute;right:-60px;top:50%;transform:translateY(-50%);width:340px;opacity:.28;pointer-events:none;filter:drop-shadow(0 0 40px rgba(58,151,255,.45))}`r`n.banner h2{position:relative}")

# hide banner atom on small screens
$t = $t.Replace('  .hero-visual{margin-top:6px}', "  .hero-visual{margin-top:6px}`r`n  .banner-atom{display:none}")

# ---- markup: nav + footer brand ----
$t = $t.Replace('<span class="brand-mark">N</span>', '<span class="brand-mark"><img src="assets/logo-transparent.svg" alt="Natria VPN"></span>')

# ---- markup: app brand ----
$t = $t.Replace('<div class="app-brand"><span class="app-dot"></span> NATRIA VPN</div>', '<div class="app-brand"><img class="app-logo" src="assets/logo-transparent.svg" alt=""> NATRIA VPN</div>')

# ---- markup: power -> atom image ----
$oldPower = @'
        <div class="power-wrap">
          <div class="power-ring"></div>
          <div class="power">
            <svg viewBox="0 0 48 48" fill="none"><path d="M24 9v13" stroke="currentColor" stroke-width="4" stroke-linecap="round"/><path d="M13.5 14.5a15 15 0 1 0 21 0" stroke="currentColor" stroke-width="4" stroke-linecap="round"/></svg>
          </div>
        </div>
'@
$newPower = @'
        <div class="power-wrap">
          <div class="atom-glow"></div>
          <img class="atom" src="assets/atom-hero.svg" alt="Natria VPN">
        </div>
'@
$t = $t.Replace($oldPower, $newPower)

# ---- markup: banner atom ----
$t = $t.Replace('<div class="banner reveal">', '<div class="banner reveal">' + "`r`n    " + '<img class="banner-atom" src="assets/atom-hero.svg" alt="">')

# ---- manifest with relative paths ----
$manifest = @'
{"name":"Natria VPN","short_name":"Natria","icons":[{"src":"favicon-192.png","sizes":"192x192","type":"image/png"},{"src":"favicon-256.png","sizes":"256x256","type":"image/png"},{"src":"logo.png","sizes":"512x512","type":"image/png"}],"theme_color":"#05101c","background_color":"#05101c","display":"standalone"}
'@
[System.IO.File]::WriteAllText((Join-Path $root 'assets\site.webmanifest'), $manifest.Trim(), (New-Object System.Text.UTF8Encoding($false)))

# ---- save ----
[System.IO.File]::WriteAllText($path, $t, (New-Object System.Text.UTF8Encoding($false)))
Write-Output ('OK bytes=' + (Get-Item $path).Length)
