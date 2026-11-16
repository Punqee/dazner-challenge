---
title: "DAZNer"
description: "Visor nativo y mínimo de DAZN (NFL Game Pass) para macOS y Windows: ventana sin bordes, dos paneles y pantalla completa dentro de la ventana."
---

# DAZNer

*Última modificación: 15 de noviembre de 2026, 22:45 (CST)*

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0.html)
[![Platform: macOS](https://img.shields.io/badge/platform-macOS-lightgrey.svg)](https://www.apple.com/macos/)
[![Platform: Windows](https://img.shields.io/badge/platform-Windows-0078D6.svg)](https://www.microsoft.com/windows/)
[![Swift](https://img.shields.io/badge/Swift-F05138?logo=swift&logoColor=white)](https://www.swift.org/)
[![WebKit](https://img.shields.io/badge/WebKit-WKWebView-informational.svg)](https://webkit.org/)
[![C#](https://img.shields.io/badge/C%23-.NET%2010-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![WebView2](https://img.shields.io/badge/WebView2-Chromium-0078D7.svg)](https://learn.microsoft.com/microsoft-edge/webview2/)

Visor nativo y mínimo de DAZN (NFL Game Pass) para macOS (WebKit) y Windows (WPF con WebView2). Muestra los partidos en una ventana sin bordes, ideal para pantallas grandes y para usarla junto al *multiview* de DAZN. Proyecto personal de Rodrigo Álvarez (@incognia).

## Estado

| Plataforma | Estado | Motor web |
|---|---|---|
| macOS | Disponible | WebKit (`WKWebView`) |
| Windows | Disponible | WebView2 (Chromium) |
| Linux (Qt) | Planeada | Qt WebEngine (Chromium) |
| Linux (GTK) | Experimental: depende del DRM | WebKitGTK |

En cada plataforma nueva se comprueba primero que DAZN reproduce en su motor web, y solo después se construye el resto. El orden de trabajo está en [ROADMAP.md](ROADMAP.md) y los pendientes, por máquina, en [TODO.md](TODO.md).

## Por qué WebKit y WebView2

- DAZN usa DRM. En macOS, WebKit reproduce con FairPlay igual que Safari; en Windows, WebView2 reproduce con Widevine igual que Edge y Chrome, sin configuración adicional.
- Cada versión usa el lenguaje y motor web nativos de su sistema: Swift con `swiftc` en macOS y C# (.NET) con WPF y WebView2 en Windows.

## Compilar (macOS)

```bash
./macos/build.sh --open
```

El *script* compila con `swiftc`, copia al paquete el *script* de pantalla completa de `shared/fill-fullscreen.js`, firma la aplicación de forma local y, con `--open`, la abre. El resultado queda en `macos/DAZNer.app`.

Para regenerar el ícono a partir de `shared/assets/icon.png`:

```bash
swiftc shared/scripts/make-icon.swift -o /tmp/make-icon
/tmp/make-icon shared/assets/icon.png macos/DAZNer.app/Contents/Resources/app.icns
```

## Compilar (Windows)

### Requisitos

- **Sistema:** Windows 10 o Windows 11.
- **SDK de .NET 10:** necesario para compilar el proyecto WPF (`net10.0-windows`). Se instala en PowerShell con:

  ```powershell
  winget install --id Microsoft.DotNet.SDK.10 -e
  ```

- **Runtime de WebView2:** WebView2 Evergreen Runtime viene preinstalado en Windows 11. En Windows 10 puede comprobarse en PowerShell con:

  ```powershell
  Get-ItemProperty -Path "HKLM:\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-F501-41E0-9E13-176E0D246995}" -ErrorAction SilentlyContinue
  ```

  Si no está instalado, se obtiene desde la página oficial del instalador de WebView2 Evergreen.

### Compilación

Desde PowerShell (Windows PowerShell 5.1 o PowerShell 7) en la raíz del repositorio (con `-ExecutionPolicy Bypass` por si la política predeterminada de Windows bloquea la ejecución de *scripts* locales):

```powershell
powershell -ExecutionPolicy Bypass -File .\windows\build.ps1
```

O con `--open` para abrir la aplicación al terminar (en PowerShell 7 puedes cambiar `powershell` por `pwsh`, o ejecutar directamente `.\windows\build.ps1 --open` si tu sesión ya permite correr *scripts*):

```powershell
powershell -ExecutionPolicy Bypass -File .\windows\build.ps1 --open
```

El *script* comprueba `shared\fill-fullscreen.js` (y regenera `windows\app.ico` desde `shared\assets\icon.png` si no existe), compila `windows\DAZNer.sln` en configuración `Release` con `dotnet build -c Release` y copia `shared\fill-fullscreen.js` junto al ejecutable. El resultado queda en:

```text
windows\bin\Release\net10.0-windows\DAZNer.exe
```

También puedes abrir la solución `windows\DAZNer.sln` en Visual Studio y compilarla (en `Release` o `Debug`), o ejecutar de forma directa `dotnet build windows\DAZNer.sln`: el proyecto `windows\DAZNer.csproj` integra `windows\app.ico` y copia `shared\fill-fullscreen.js` a la carpeta de salida.

## Estructura del repositorio

```text
DAZNer/
├── macos/     código de la versión de macOS y su paquete DAZNer.app
├── windows/   código de la versión de Windows (C# / WPF con WebView2) y build.ps1
├── shared/    ícono, imágenes y scripts comunes, entre ellos el de pantalla completa
├── openspec/  especificaciones del comportamiento y cambios en curso
├── ROADMAP.md orden de ejecución y plan para Windows y Linux
├── TODO.md    pendientes por orden y por máquina
├── AGENTS.md  guía para agentes que trabajan en el repositorio
└── ...        README, CHANGELOG, NOTICE y LICENSE
```

Las versiones para Linux todavía no existen: su plan está en [ROADMAP.md](ROADMAP.md) y sus pendientes en [TODO.md](TODO.md).

## Desarrollo con OpenSpec

El trabajo pendiente se planea con [OpenSpec](https://openspec.dev/): las especificaciones de `openspec/specs/` describen el comportamiento de la versión de macOS y son el contrato que debe cumplir cada plataforma nueva, y cada versión es un cambio en `openspec/changes/` (`add-windows-port`, `add-linux-qt-port` y `add-linux-gtk-port`); los cambios terminados, como `extract-fullscreen-script`, quedan archivados en `openspec/changes/archive/`. Quien clone el repositorio para trabajar en uno de ellos debe leer primero [AGENTS.md](AGENTS.md); el orden y los pendientes están en [ROADMAP.md](ROADMAP.md) y [TODO.md](TODO.md).

## Contribuir

Todo cambio entra por un *pull request* con la plantilla de [.github/PULL_REQUEST_TEMPLATE.md](.github/PULL_REQUEST_TEMPLATE.md), a partir de un cambio de OpenSpec. Antes de abrirlo, corre `~/rules/scripts/check-pr.sh --base origin/main` (o `/pr-check`), el mismo que corre GitHub Actions; el *script* y sus reglas están en el repositorio [incognia/rules](https://github.com/incognia/rules) y la configuración de este proyecto, en `.github/pr-check.conf`. Un PR que no lo pase se rechaza sin revisar el código: *commits* atómicos en inglés (UK) con Conventional Commits y `Co-Authored-By`, viñetas del CHANGELOG solo añadidas y en el último *commit*, y ningún archivo ajeno. Los detalles para agentes están en [AGENTS.md](AGENTS.md). Si quieres demostrar cómo trabajas con IA en un proyecto real, el reto abierto está en [CHALLENGE.md](CHALLENGE.md).

## Uso

- **Mover la ventana:** arrastra desde la franja invisible del borde superior (14 px) o mantén ⌥ Option (en macOS) o `Alt` (en Windows) y arrastra desde cualquier punto. El doble clic en la franja maximiza o restaura.
- **Redimensionar:** desde los bordes, con proporción fija (16:9 con un panel, 32:9 con dos).
- **Rellenar y centrar:** ⌘F (macOS) o `Ctrl+F` (Windows) agranda la ventana al máximo que cabe en el área útil del monitor sin perder la proporción y la centra.
- **Paneles:** ⌘1 / `Ctrl+1` un panel, ⌘2 / `Ctrl+2` dos paneles, con un hueco de 8 px entre ellos como en el *multiview* de DAZN. El segundo se crea al elegirlo y se cierra al volver a un panel, para liberar memoria.
- **Pantalla completa:** el botón de pantalla completa de DAZN llena la ventana o el panel en lugar de pasar a la pantalla completa del sistema. Esc regresa al modo normal.
- **Atajos:** ⌘L / `Ctrl+L` abre una URL en el panel con foco, ⌘R / `Ctrl+R` recarga, ⌘H / `Ctrl+H` vuelve al inicio de DAZN, ⌘` / `` Ctrl+` `` (o `Ctrl+Tab` en Windows) cambia el panel con audio, ⌘T / `Ctrl+T` deja la ventana siempre encima, ⌘W / `Ctrl+W` cierra y ⌘Q / `Ctrl+Q` sale.
- **Varias instancias:** `open -n macos/DAZNer.app` en macOS, o ejecutar otra instancia de `DAZNer.exe` en Windows, abre otra ventana independiente. Comparten la misma sesión.

## Modo de diagnóstico

En macOS:

```bash
open -n macos/DAZNer.app --args --debug
```

En Windows:

```powershell
.\windows\bin\Release\net10.0-windows\DAZNer.exe --debug
```

Escribe `debug.log` junto a la aplicación con los eventos de pantalla completa y de cada panel, y habilita el inspector web (Safari en macOS; DevTools de WebView2 con `Ctrl+Shift+I` en Windows). Está desactivado de forma predeterminada.

## Notas

- DAZNer es un proyecto no oficial y no está afiliado a DAZN ni a la NFL. Consulta `NOTICE.md`.
- La contraseña no se guarda. Solo se conserva la sesión de DAZN, en `~/Library/WebKit/mx.org.faraday.dazner` en macOS (el identificador de la aplicación es `mx.org.faraday.dazner`, el dominio faraday.org.mx al revés) y en `%LOCALAPPDATA%\DAZNer` en Windows.
- Cambiar el tamaño de la ventana hace que DAZN rehaga el reproductor y refresque el panel. Conviene dejar la ventana en el tamaño deseado antes de abrir el segundo panel.
- El límite de reproducción de DAZN depende de la ubicación (red) y no del número de ventanas.
- La aplicación está firmada solo de forma local en macOS (si el sistema la bloquea, ábrela con clic derecho → Abrir) y sin firma comercial en Windows.
- Las opciones de 3 y 4 paneles están comentadas en el código de macOS para reutilizarlas después.

## Licencia

DAZNer es software libre y se distribuye bajo la licencia GPLv3. Consulta el texto completo en [LICENSE](LICENSE) y el aviso de proyecto no oficial y de marcas en [NOTICE.md](NOTICE.md).

<!-- licence:start -->

---

*Este proyecto fue elaborado por Rodrigo Álvarez (@incognia) y se distribuye bajo la licencia GPLv3. Para más detalles, consulta el archivo LICENSE.*

*Copyright © 2026, Rodrigo Ernesto Álvarez Aguilera. Este es software libre bajo los términos de la GNU General Public License v3.*

<!-- licence:end -->
