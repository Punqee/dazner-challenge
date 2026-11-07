---
title: "DAZNer"
description: "Visor nativo y mínimo de DAZN (NFL Game Pass) para macOS con WebKit: ventana sin bordes, dos paneles y pantalla completa dentro de la ventana."
---

# DAZNer

*Última modificación: 7 de noviembre de 2026, 15:30 (CST)*

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0.html)
[![Platform: macOS](https://img.shields.io/badge/platform-macOS-lightgrey.svg)](https://www.apple.com/macos/)
[![Swift](https://img.shields.io/badge/Swift-F05138?logo=swift&logoColor=white)](https://www.swift.org/)
[![WebKit](https://img.shields.io/badge/WebKit-WKWebView-informational.svg)](https://webkit.org/)

Visor nativo y mínimo de DAZN (NFL Game Pass) para macOS, hecho con WebKit. Muestra los partidos en una ventana sin bordes, ideal para pantallas grandes y para usarla junto al *multiview* de DAZN. Proyecto personal de Rodrigo Álvarez (@incognia).

## Estado

| Plataforma | Estado | Motor web |
|---|---|---|
| macOS | Disponible | WebKit (`WKWebView`) |
| Windows | Planeada | WebView2 (Chromium) |
| Linux (Qt) | Planeada | Qt WebEngine (Chromium) |
| Linux (GTK) | Experimental: depende del DRM | WebKitGTK |

En cada plataforma nueva se comprueba primero que DAZN reproduce en su motor web, y solo después se construye el resto. El orden de trabajo está en [ROADMAP.md](ROADMAP.md) y los pendientes, por máquina, en [TODO.md](TODO.md).

## Por qué WebKit

- DAZN usa DRM. WebKit reproduce con FairPlay igual que Safari, sin configuración adicional.
- Es un solo archivo de Swift que se compila con `swiftc`, sin Xcode y sin dependencias.

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

## Estructura del repositorio

```text
DAZNer/
├── macos/     código de la versión de macOS y su paquete DAZNer.app
├── shared/    ícono, imágenes y scripts comunes, entre ellos el de pantalla completa
├── openspec/  especificaciones del comportamiento y cambios en curso
├── ROADMAP.md orden de ejecución y plan para Windows y Linux
├── TODO.md    pendientes por orden y por máquina
├── AGENTS.md  guía para agentes que trabajan en el repositorio
└── ...        README, CHANGELOG, NOTICE y LICENSE
```

Las versiones para Windows y Linux todavía no existen: su plan está en [ROADMAP.md](ROADMAP.md) y sus pendientes en [TODO.md](TODO.md).

## Desarrollo con OpenSpec

El trabajo pendiente se planea con [OpenSpec](https://openspec.dev/): las especificaciones de `openspec/specs/` describen el comportamiento de la versión de macOS y son el contrato que debe cumplir cada plataforma nueva, y cada versión es un cambio en `openspec/changes/` (`add-windows-port`, `add-linux-qt-port` y `add-linux-gtk-port`); los cambios terminados, como `extract-fullscreen-script`, quedan archivados en `openspec/changes/archive/`. Quien clone el repositorio para trabajar en uno de ellos debe leer primero [AGENTS.md](AGENTS.md); el orden y los pendientes están en [ROADMAP.md](ROADMAP.md) y [TODO.md](TODO.md).

## Contribuir

Todo cambio entra por un *pull request* con la plantilla de [.github/PULL_REQUEST_TEMPLATE.md](.github/PULL_REQUEST_TEMPLATE.md), a partir de un cambio de OpenSpec. Antes de abrirlo, corre `~/rules/scripts/check-pr.sh --base origin/main` (o `/pr-check`), el mismo que corre GitHub Actions; el *script* y sus reglas están en el repositorio [incognia/rules](https://github.com/incognia/rules) y la configuración de este proyecto, en `.github/pr-check.conf`. Un PR que no lo pase se rechaza sin revisar el código: *commits* atómicos en inglés (UK) con Conventional Commits y `Co-Authored-By`, viñetas del CHANGELOG solo añadidas y en el último *commit*, y ningún archivo ajeno. Los detalles para agentes están en [AGENTS.md](AGENTS.md). Si quieres demostrar cómo trabajas con IA en un proyecto real, el reto abierto está en [CHALLENGE.md](CHALLENGE.md).

## Uso

- **Mover la ventana:** arrastra desde la franja invisible del borde superior (14 px) o mantén ⌥ Option y arrastra desde cualquier punto. El doble clic en la franja maximiza o restaura.
- **Redimensionar:** desde los bordes, con proporción fija (16:9 con un panel, 32:9 con dos).
- **Rellenar y centrar:** ⌘F agranda la ventana al máximo que cabe en la pantalla sin perder la proporción y la centra.
- **Paneles:** ⌘1 un panel, ⌘2 dos paneles, con un hueco de 8 px entre ellos como en el *multiview* de DAZN. El segundo se crea al elegirlo y se cierra al volver a un panel, para liberar memoria.
- **Pantalla completa:** el botón de pantalla completa de DAZN llena la ventana o el panel en lugar de pasar a la pantalla completa de macOS. Esc regresa al modo normal.
- **Atajos:** ⌘L abre una URL en el panel con foco, ⌘R recarga, ⌘H vuelve al inicio de DAZN, ⌘` cambia el panel con audio, ⌘T deja la ventana siempre encima, ⌘W cierra y ⌘Q sale.
- **Varias instancias:** `open -n macos/DAZNer.app` abre otra ventana independiente. Comparten la misma sesión.

## Modo de diagnóstico

```bash
open -n macos/DAZNer.app --args --debug
```

Escribe `debug.log` junto a la aplicación (en `macos/`) con los eventos de pantalla completa y de cada panel, y habilita el inspector web de Safari. Está desactivado de forma predeterminada.

## Notas

- DAZNer es un proyecto no oficial y no está afiliado a DAZN ni a la NFL. Consulta `NOTICE.md`.
- La contraseña no se guarda. Solo se conserva la sesión de DAZN, en `~/Library/WebKit/mx.org.faraday.dazner`. El identificador de la aplicación es `mx.org.faraday.dazner` (el dominio faraday.org.mx al revés).
- Cambiar el tamaño de la ventana hace que DAZN rehaga el reproductor y refresque el panel. Conviene dejar la ventana en el tamaño deseado antes de abrir el segundo panel.
- El límite de reproducción de DAZN depende de la ubicación (red) y no del número de ventanas.
- La aplicación está firmada solo de forma local. Si macOS la bloquea, ábrela con clic derecho → Abrir.
- Las opciones de 3 y 4 paneles están comentadas en el código para reutilizarlas después.

## Licencia

DAZNer es software libre y se distribuye bajo la licencia GPLv3. Consulta el texto completo en [LICENSE](LICENSE) y el aviso de proyecto no oficial y de marcas en [NOTICE.md](NOTICE.md).

<!-- licence:start -->

---

*Este proyecto fue elaborado por Rodrigo Álvarez (@incognia) y se distribuye bajo la licencia GPLv3. Para más detalles, consulta el archivo LICENSE.*

*Copyright © 2026, Rodrigo Ernesto Álvarez Aguilera. Este es software libre bajo los términos de la GNU General Public License v3.*

<!-- licence:end -->
