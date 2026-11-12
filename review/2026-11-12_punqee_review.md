# Quinta revisión del reto: Punqee

*Última modificación: 12 de noviembre de 2026, 00:22 (CST)*

Quinta revisión de la entrega de Míriam ([@Punqee](https://github.com/Punqee)) al [reto de contribución](../CHALLENGE.md), hecha por el mantenedor con Claude Code (Opus 5.5) en una máquina con Windows. Revisa lo que entregó el 9 y el 11 de noviembre después de la [cuarta revisión](2026-11-09_punqee_review_3.md): las correcciones que se le pidieron y el grupo 4 (pantalla completa y sesión). La calificación vigente está en [score/punqee.md](../score/punqee.md).

## Datos de la entrega

| Dato | Valor |
|---|---|
| PR | [#5](https://github.com/incognia/dazner/pull/5), abierto |
| Rama | `challenge/punqee/add-windows-port`, en `255c5d5` |
| Base | `977a0ae`, el `main` actual |
| *Commits* | 18 (los catorce de la cuarta revisión, reaplicados, y cuatro nuevos) |
| Herramienta de IA | Antigravity con Gemini 3.8 Flash (esfuerzo alto) y Gemini 4 Argon (esfuerzo medio) |

| *Commit* | Asunto |
|---|---|
| `f07a63b` | `docs(windows): document pane resize order and next-pane shortcut keys` |
| `88a9129` | `refactor(windows): resize window before updating pane grid layout and trim shortcut keys` |
| `1ed709c` | `docs(windows): document fullscreen script injection and persistent session design` |
| `255c5d5` | `feat(windows): add shared fullscreen script, debug logging and persistent session` |

## Puertas automáticas

- **`check-pr.sh --base upstream/main`: falla una comprobación.** `CHANGELOG.md` cambia en dos *commits* (`88a9129` y `255c5d5`) y solo puede cambiar en el último. Según AGENTS.md, un PR que no pasa `check-pr.sh` se rechaza sin revisar el código. Es un error de proceso y se corrige reescribiendo la rama; el mantenedor revisó y probó el código de todos modos para no frenar el avance.
- **OpenSpec:** sin validar, como siempre, porque la máquina del mantenedor no tiene Node.
- **Finales de línea:** `git ls-files --eol` no muestra ningún archivo con CRLF en el repositorio.
- **Archivos ajenos:** no toca `shared/` ni `macos/`. El script de pantalla completa se copia desde `shared/` con un vínculo en `DAZNer.csproj`.

## Compilación

`dotnet build windows\DAZNer.sln` en la copia de su rama: compilación correcta, 0 advertencias y 0 errores. `fill-fullscreen.js` queda junto al ejecutable.

## Lo que atendió de la cuarta revisión

- **Orden al abrir el segundo panel:** ahora cambia primero el tamaño de la ventana y después las columnas del `Grid`, como `macos/DAZNer.swift`. Lo documentó en `design.md` antes del código.
- **Teclas para cambiar de panel:** quedan cuatro (`Oem3`, `Oem5`, `Oem1` y `Tab`), cada una documentada en `design.md` con la distribución de teclado que la motiva.

## Lo nuevo: grupo 4

- **Script compartido (tarea 4.1):** se registra con `AddScriptToExecuteOnDocumentCreatedAsync` después de las variables de la plataforma (`window.__daznerDebug`, `window.__daznerLog` y `window.__pane`), y la página se carga cuando los scripts ya están registrados. Como en WebView2 `document.head` todavía no existe cuando corre el script, agrega un puente que difiere el primer `appendChild` hasta que exista `<head>`, sin modificar el script compartido. Lo explica en la bitácora de `design.md`.
- **Depuración:** con `--debug`, los mensajes llegan por `WebMessageReceived` a `debug.log`, junto al ejecutable, y se habilita el inspector (`Ctrl+Shift+I`). Sin `--debug`, las herramientas de desarrollo quedan apagadas.
- **Sesión (tarea 4.3):** carpeta de datos propia en `%LOCALAPPDATA%\DAZNer`, con un solo entorno de WebView2 para los dos paneles, y el guardado de contraseñas y el autocompletado apagados.
- **Pantalla completa (tarea 4.2):** la dejó sin marcar. Probó el video principal en uno y dos paneles, y declaró el *multiview* sin probar porque su cuenta no tiene NFL Game Pass.

## Prueba en el equipo del mantenedor

Por escritorio remoto (RDP), ahora con dos monitores, con la cuenta del mantenedor:

| Prueba | Resultado |
|---|---|
| Pantalla completa con un panel | **Funciona.** El video llena la ventana de DAZNer, no el monitor, y `Esc` lo regresa a la página. |
| Pantalla completa con dos paneles | **Funciona.** El video llena solo su panel, el otro sigue reproduciendo, y `Esc` lo regresa. |
| Pantalla completa de un video del *multiview* | **Falla.** No se ve ninguno de los videos; `Esc` sí regresa a la vista normal. |
| Pantalla completa general del *multiview* (dos videos) | **Falla.** Los dos videos llenan la ventana, pero quedan visibles el encabezado, el título, la barra de DAZN y el fondo de la página. |
| `--debug` | **Funciona.** `debug.log` registra los eventos del script (`page:script-start`, `enter`, `after`, `grid`, `deep`, `mvgap` y `exit`). |
| Sesión al reabrir | **Funciona.** Sin `--debug`, abre con la sesión iniciada y `Ctrl+Shift+I` no abre el inspector. |
| Tres instancias a la vez | **Funciona.** Las tres abren con la sesión y reproducen transmisiones distintas al mismo tiempo (hay foto). |
| Dos monitores (tarea 2.2) | **Funciona.** En el otro monitor la ventana conserva 16:9 con un panel y 32:9 con dos al arrastrar bordes y esquinas, y `Ctrl+F` la llena y la centra en ese monitor. |

## Diagnóstico de la falla del *multiview*

Lo que muestra `debug.log` del mantenedor:

- El script sí detecta la pantalla completa: pone la clase `__fs` en el contenedor del video y `__fsa` en sus ancestros, y el evento `after` reporta opacidad 1 y visibilidad `visible`.
- **Pero la hoja de estilo del script no se está aplicando.** La regla `.__fs` fija `position: fixed` y `z-index: 2147483647`. En 8 de los 10 eventos `after`, el elemento con `__fs` tiene `position: relative` y `z-index: auto`. Los otros 2 traen `fixed` con `z-index: 9999`, que es el modo de pantalla completa de la propia DAZN, no el del script.
- Por eso no se ocultan el resto de la página (`__fsh`) ni las capas de DAZN: en el centro de la ventana queda `DIV._15485uv0`, encima del video. Y el contenedor del segundo video se queda en su celda (`644,260,640,357`) en lugar de llenar la ventana.
- Con un solo video fuera del *multiview* funciona porque basta con el modo de pantalla completa de DAZN y los estilos en línea que el script fija con `!important`.

La causa probable es que el `<style>` del script nunca entra al documento, o entra y luego desaparece. El primer sospechoso es el puente de `document.head`. El mantenedor no pudo confirmarlo en la consola del inspector.

Cómo comprobarlo sin una cuenta con *multiview*: con `--debug`, después de cargar DAZN, evaluar en la consola del inspector:

```js
[...document.querySelectorAll('style')].some(s => s.textContent.includes('.__fs'))
```

Si devuelve `false`, el puente falla. También sirve poner la clase `__fs` a mano en cualquier elemento y comprobar que `getComputedStyle(el).position` sea `fixed`.

## Observaciones

1. **`check-pr.sh` falla** por el CHANGELOG en dos *commits* (ver «Puertas automáticas»).
2. **La pantalla completa del *multiview* no funciona** (ver «Diagnóstico»). Es el caso por el que existe el script compartido. Su verificación en `design.md` revisó las clases (`curClass`) y los estilos en línea (`curStyle`), pero no el estilo calculado; por eso no lo detectó.
3. **Atajos fuera de su grupo.** `255c5d5` agrega `Ctrl+R` (recargar) y `Ctrl+H` (inicio), que son de la tarea 5.1. No aparecen en `tasks.md`, en `design.md` ni en el mensaje del *commit*.

## Qué se le pide a la participante

1. Corregir el CHANGELOG: que cambie solo en el último *commit*, reescribiendo su rama, hasta que `check-pr.sh` pase sin fallos.
2. Arreglar la carga de la hoja de estilo de `fill-fullscreen.js` sin modificar el script compartido, primero en `design.md` y `tasks.md` y después en el código. Agregar a la verificación de la tarea 4.2 la comprobación del estilo calculado de arriba. El mantenedor vuelve a probar el *multiview* con su cuenta.
3. Mover `Ctrl+R` y `Ctrl+H` a la tarea 5.1, o documentarlos en el diseño y en el *commit* en el que entran.

## Veredicto

- **Cuarta revisión: atendida por completo.**
- **Tareas 2.2, 4.1 y 4.3: verificadas** en el equipo del mantenedor. La 2.2 ya puede marcarse.
- **Tarea 4.2: no pasa.** La pantalla completa funciona con el video principal en uno y dos paneles, pero no en el *multiview*.
- **Avance verificado:** 13 de las 18 tareas de `add-windows-port` (1.1 a 1.4, 2.1 a 2.3, 3.1 a 3.4, 4.1 y 4.3).
- **El PR no puede integrarse** mientras `check-pr.sh` falle. Lo demás va bien encaminado.

## Entorno del mantenedor

| Componente | Versión |
|---|---|
| Sistema | Windows 11 Pro 10.0.26300, usado por RDP con dos monitores |
| SDK de .NET | 10.0.403 |
| *Runtime* de WebView2 | 157.0.4341.42 |
| Visual Studio | no instalado |
| Git | Git para Windows, con `.gitattributes` y la configuración de `/git-init` |
