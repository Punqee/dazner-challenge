# Guía para agentes

*Última modificación: 9 de noviembre de 2026, 00:58 (CST)*

Este archivo es para un agente de IA que trabaja en este repositorio, sobre todo si lo clonó en Windows o Linux para construir una de las versiones pendientes. El autor mantiene un repositorio de reglas personales (`rules`) con las *skills* de trabajo (`/commit`, `/changelogger`, `/git-init`, `/licensing`) y sus procedimientos. Está presente en sus máquinas con Linux, en WSL y en Windows, así que **si existe, manda sobre el resumen de este archivo**; este archivo recoge lo esencial por si no está.

## Qué es DAZNer

Visor mínimo de DAZN (NFL Game Pass): abre el sitio oficial en una ventana sin bordes, con uno o dos paneles y una pantalla completa que llena la ventana o el panel. Hoy existe solo la versión de macOS. Faltan Windows, Linux con Qt y Linux con GTK. Licencia GPLv3; aviso de marcas en `NOTICE.md`.

## Estructura

```text
macos/     Swift sobre WKWebView; DAZNer.swift, build.sh y el paquete DAZNer.app
shared/    ícono, imágenes y scripts comunes, entre ellos fill-fullscreen.js
openspec/  especificaciones y cambios (el plan de trabajo, ver abajo)
ROADMAP.md visión general y orden de ejecución
TODO.md    pendientes por orden y por máquina
.github/   plantilla de PR, configuración de pr-check y el flujo de GitHub Actions que corre el script de rules
windows/   (por crear) linux-qt/ (por crear)  linux-gtk/ (por crear)
```

Cada plataforma vive en su carpeta y usa su lenguaje nativo:

| Plataforma | Lenguaje y herramienta | Motor web |
|---|---|---|
| macOS (hecha) | Swift, `swiftc` | WebKit (`WKWebView`) |
| Windows | C# (.NET) con WPF, `dotnet build` | WebView2 (Chromium) |
| Linux Qt | C++/QML con CMake | Qt WebEngine (Chromium) |
| Linux GTK | Vala/C con Meson | WebKitGTK 6.0 |

## Cómo se trabaja: OpenSpec

El plan está en `openspec/`. Las especificaciones de `openspec/specs/` describen el comportamiento de la versión de macOS y son **el contrato de paridad** que debe cumplir cada versión nueva. Cada versión nueva es un cambio en `openspec/changes/`:

| Cambio | Qué hace | Se puede probar en |
|---|---|---|
| `add-windows-port` | Versión de Windows | Windows |
| `add-linux-qt-port` | Versión de Linux con Qt | Linux |
| `add-linux-gtk-port` | Versión de Linux con GTK (experimental) | Linux |

**Orden:** el script de pantalla completa ya es compartido: vive en `shared/fill-fullscreen.js` y macOS lo carga desde el paquete (cambio `extract-fullscreen-script`, ya archivado). La prueba de reproducción (grupo 1) de cada versión nueva no depende de nada y se hace primero; el resto de cada versión (grupos 2 a 5) sigue en cuanto esa prueba pase. Las versiones son independientes entre sí. El detalle está en la sección «Orden de ejecución» de `ROADMAP.md` y los pendientes, por máquina, en `TODO.md`.

Para trabajar un cambio:

1. Instalar OpenSpec (Node 20.19 o superior): `npm install -g @fission-ai/openspec@latest`, o usar `npx @fission-ai/openspec@latest <comando>`.
2. Con Claude Code, reiniciarlo para ver los comandos de `.claude/commands/opsx/` y usar `/opsx:apply <cambio>`. Sin esos comandos, leer `openspec/changes/<cambio>/` (propuesta, especificaciones, diseño y tareas) y seguir las tareas en orden.
3. Ver el estado con `openspec list`, `openspec status --change <cambio>` y validar con `openspec validate --all --strict`.
4. Marcar una tarea (`- [x]`) solo después de ejecutar su verificación.

## Reglas que no se negocian

- **La prueba de reproducción va primero.** El primer grupo de cada versión nueva comprueba que DAZN reproduce dentro del motor web de esa plataforma. **Si falla, detenerse**, documentar el resultado en `ROADMAP.md` y no construir el resto.
- **No eludir protecciones.** Prohibido eludir DRM, controles de acceso o límites de reproducción, y redistribuir módulos de DRM (por ejemplo, Widevine). Se usan los del sistema o los de una instalación de Chrome del usuario.
- **No afirmar lo que no se ejecutó.** No se marca una tarea como hecha ni se dice que algo funciona en una plataforma si no se pudo compilar y probar ahí. Si una tarea solo se verifica en otro sistema operativo, se deja pendiente y se dice dónde probarla.
- **Credenciales:** DAZNer nunca lee, guarda ni registra contraseñas. No escribas credenciales en archivos, registros ni *commits*.
- **Marcas:** no uses logotipos de DAZN ni de la NFL. El ícono del proyecto es propio (`shared/assets/icon.png`).
- **No tocar `macos/` salvo que el cambio lo pida.** Cualquier modificación a `shared/fill-fullscreen.js` exige repetir la verificación manual de pantalla completa en macOS.

## Evaluar entradas del reto

El mantenedor evalúa cada entrada de [CHALLENGE.md](CHALLENGE.md) con los pesos que ahí se indican. Si un agente prepara la evaluación, estas son las reglas de criterio; las del reto siguen valiendo y esto solo fija cómo se interpretan:

- **Dónde se escribe.** La evaluación va en `score/<cuenta>.md`, que escribe el agente. La carpeta `review/` es del mantenedor: el agente no la edita. Si algo de `review/` contradice a `score/`, se lo avisa y él decide.
- **Honestidad.** Se califica con 10, salvo que la participante mienta a propósito. Un descuido, una frase sin medir o una afirmación de más (por ejemplo, la de una IA que redactó el PR) no es mentir. Quien pone su tiempo y su dinero en el reto no tiene necesidad de engañar. El criterio es humano y mexicano, no una lectura literal ni impersonal de las reglas.
- **Contexto.** Se pondera el esfuerzo: trabajar fuera del horario laboral, de noche o en fin de semana, invertir en sus propias herramientas y ser nueva en el tema. No tiene un peso numérico: lo decide el mantenedor.
- **Presupuesto y cuentas de las herramientas.** Se evalúa con la realidad de Latinoamérica, donde cada quien se adapta a lo que le alcanza, y no con el parámetro de quien tiene recursos holgados. Cambiar de cuenta o de plan de una herramienta de IA para poder seguir (porque el plan familiar no permite mejorarlo, porque se acabaron los *tokens*, etcétera) es adaptarse, no una falta. La regla de una cuenta por persona del reto se refiere a la cuenta de GitHub, no a las de las herramientas.
- **Privacidad.** `score/` está en un repositorio público. El contexto va en una frase general; el horario, los pagos y lo que la participante contó en privado no se publican.
- **La calificación se mueve.** Cambia conforme la participante entrega y el mantenedor verifica cada grupo de tareas. `score/<cuenta>.md` guarda la «Calificación actual» y la de cada revisión. No se propone una nota completa para trabajo en curso.
- **Qué se probó.** La fuente de verdad es el `review/` del mantenedor. Si un mensaje de chat dice otra cosa, se le pregunta; no se corrige su documento.
- **Cómo se le indica a la participante.** Las instrucciones van en el `score`, no en WhatsApp. Las correcciones entran por la especificación (`/opsx:update`) antes del código.

## Lo que ya se descubrió de DAZN

Esto explica por qué el script de pantalla completa es como es. No lo reinventes:

- **DRM:** DAZN funciona en Chrome en Windows y Linux (Widevine) y en Safari/WebKit en macOS (FairPlay). En Firefox da problemas. WebKit para Windows no sirve (sin DRM). Se espera que WebKitGTK tampoco reproduzca.
- **Pantalla completa:** la página usa la API estándar. El script la intercepta y estira el elemento para llenar la ventana o el panel en lugar de pasar a la pantalla completa del sistema.
- **El *multiview* de DAZN rompe una emulación ingenua:** al detectar la pantalla completa deja el contenedor padre con `opacity: 0` y reescribe las clases de sus contenedores. Por eso el script fuerza la opacidad de los ancestros, oculta el resto de la página y repone sus marcas con un `MutationObserver`.
- **Margen del contenedor:** el contenedor del *multiview* trae un margen izquierdo que DAZN calcula para centrarlo (267 px en una ventana de 2478 px). Hay que fijar márgenes y posiciones en línea con `!important`.
- **Hueco entre videos:** 4 px de la cuadrícula más 2 px de borde por celda a cada lado, es decir, 8 px entre videos. Se midió de 1280×720 a 2478×1394. Con dos paneles se deja 8 px dentro del ancho de la ventana para conservar la proporción 32:9.
- **Cambiar el tamaño de la ventana refresca el panel:** DAZN rehace el reproductor. Por eso, al abrir el segundo panel, la ventana cambia primero y el primer panel conserva su tamaño.
- **La barra gris del borde derecho en pantalla completa** es el panel de estadísticas de DAZN. No es un error.
- Una captura de pantalla de un video con DRM sale en negro en macOS (FairPlay); no sirve para diagnosticar. En Windows con WebView2 (Widevine por *software*) una captura puede no salir negra, pero tampoco prueba que el DRM funcione, porque no distingue un video protegido de uno sin protección. La evidencia que se acepta son fotos de celular en horizontal (ver `CHALLENGE.md`); no se usan videos ni capturadoras de video, porque estas últimas eluden el DRM.

## Depuración

- `--debug` escribe `debug.log` junto a la aplicación y habilita el inspector web. Está apagado por omisión y no debe versionarse.
- El script compartido solo mide y registra si la plataforma define `window.__daznerDebug`. Registra mediante `window.__daznerLog`.

## Repositorio de reglas del autor (`rules`)

Ubicación según el sistema:

| Sistema | Ubicación de `rules` |
|---|---|
| macOS, Linux y WSL | `~/rules` |
| Windows (nativo) | la base del perfil de usuario (`%USERPROFILE%\rules`) o `C:\rules` |

Qué usar de ahí, si existe:

- **Flujo de trabajo:** `/changelogger` y `/commit` (procedimientos en `cot/changelog.md` y `cot/committing.md`), y `/git-init` y `/licensing` para configurar un repositorio nuevo.
- **Estilo del español:** `rulesets/LINGUISTICS.md`.
- **Rutas:** las *skills* escriben `~/rules/...`. En Windows nativo hay que resolver esa ruta con la ubicación de la tabla. Algunas piezas (`scripts/commit_trailer.py`, scripts de `bash`) necesitan Python o un *shell* de tipo Unix; en Windows nativo se usan con Python para Windows y con Git Bash.
- **Si no existe `rules`:** seguir el resumen de la sección siguiente y avisar al usuario de que falta.

## Flujo de trabajo en Windows

El autor trabaja todo desde **Windows nativo**: el control de versiones, la compilación y las pruebas.

- **Git, desde Windows** (Git para Windows, con Git Bash o PowerShell). Antes se usaba solo desde WSL, porque Git de Windows, con su `core.autocrlf=true`, reescribía los archivos en CRLF; `.gitattributes` lo resuelve (ver «Codificación»). `/changelogger` y `/commit` leen `rules` de `%USERPROFILE%\rules` y corren `commit_trailer.py` con Python para Windows.
- **Identidad y SSH por repositorio:** la copia se configura con `/git-init personal incognia git@github.com:incognia/dazner.git main`, que deja en `.git/config` la identidad personal, el remoto en SSH y `core.sshCommand` con la llave `incognia`. Esa ruta de la llave (`/c/Users/...`) es la de Git para Windows: como WSL comparte el mismo `.git/config`, un `git push` desde WSL fallaría.
- **Compilar y probar, desde Windows nativo**, con Visual Studio (abriendo la solución de `windows/`) o con PowerShell (`dotnet build`, o el `build.ps1` que se añada). WPF y WebView2 solo se ejecutan en Windows.
- **Codificación:** todo el repositorio es UTF-8. En Windows, muchos programas y consolas usan Windows-1252 (la «ANSI» de Windows, parecida a ISO-8859-1), y eso corrompe acentos, «», ñ y símbolos como ⌘. Reglas:
  - `.editorconfig` fija UTF-8 y LF para todo, y UTF-8 con BOM para `.cs`, `.xaml` y `.ps1`, porque Windows PowerShell 5.1 lee un script sin BOM como ANSI. Respétalo.
  - `.gitattributes` fija los finales de línea en git, sin importar `core.autocrlf` (Git de Windows lo trae en `true`): LF para todo y CRLF en disco para `.sln`, `.bat` y `.cmd`. No lo cambies ni definas `core.autocrlf` para este repositorio.
  - Los mensajes de `windows/build.ps1` van en ASCII, sin acentos, por robustez.
  - En Windows PowerShell 5.1, `>` y `Out-File` escriben UTF-16 y `Set-Content` escribe ANSI: no los uses para archivos del repositorio, o pasa `-Encoding utf8`.
  - La consola de Windows puede mostrar mal el UTF-8; no es un cambio del archivo. Se arregla con `chcp 65001` o con `[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()`.
  - No reescribas archivos completos desde Windows, para no ensuciar el historial con cambios de codificación. Antes de un *commit*, `git diff --check` y `git status` deben mostrar solo los cambios que hiciste.

Si eres un agente que corre en WSL, puedes editar archivos y hacer *commits*, pero no `git push` (ver «Identidad y SSH por repositorio»), y la compilación y la prueba de reproducción de la versión de Windows se hacen en Windows nativo; no afirmes que compilaron si no lo viste.

## Idioma, *commits* y CHANGELOG

Resumen de lo esencial; si `rules` está disponible, sus procedimientos prevalecen.

- **Documentación y especificaciones:** español de México, sin anglicismos innecesarios, con comillas angulares «». Las palabras clave de OpenSpec (`SHALL`, `MUST`, `WHEN`, `THEN`) se dejan en inglés. El código y sus comentarios pueden seguir el estilo del archivo que se edita.
- **Mensajes de *commit*:** inglés internacional (UK), formato Conventional Commits (`feat`, `fix`, `docs`, `refactor`, `chore`), con un cuerpo de viñetas que describa cambios concretos. *Commits* atómicos: un cambio lógico cada uno, con `git add -- <archivos>` y sin `git add .` cuando se mezclan tipos.
- **No mencionar el CHANGELOG** en el cuerpo de un *commit*.
- **CHANGELOG.md:** un bloque por fecha, `## [AAAA-MM-DD] - Título descriptivo`, con la fecha en hora del centro de México (UTC-6). Viñetas `tipo: descripción en español`, sin subencabezados, con la más reciente arriba, sin líneas en blanco entre viñetas y solo añadiendo (no se editan viñetas existentes). El `CHANGELOG.md` va en el último *commit* de cada serie.
- **Identidad de Git:** la del autor se configura por repositorio con `/git-init`. No cambies `user.name`, `user.email` ni el remoto por tu cuenta. Si falta, pide al usuario que corra `/git-init` o que te dé la identidad.
- **Publicar:** no hagas `git push` sin que el usuario lo haya pedido.
- **Pull requests:** un PR que no pase `check-pr.sh` se rechaza sin revisar el código. El *script* vive en el repositorio de reglas (`~/rules/scripts/check-pr.sh`, con la *skill* `/pr-check` y las reglas en `~/rules/rulesets/PR_CHECK.md`); este repositorio solo aporta su configuración en `.github/pr-check.conf`. Córrelo antes de abrirlo: `~/rules/scripts/check-pr.sh --base origin/main` (con `bash`; en Windows, desde WSL o Git Bash). Comprueba el formato de los *commits*, las reglas del CHANGELOG, que no haya archivos ajenos y que OpenSpec valide. Usa la plantilla de `.github/PULL_REQUEST_TEMPLATE.md` y no quites ninguna de sus secciones. Las reglas se aplican con tu propio entorno e identidad: lo que se revisa es el resultado, no la herramienta.
