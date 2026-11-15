# Sexta revisión del reto: Punqee

*Última modificación: 15 de noviembre de 2026, 13:18 (CST)*

Sexta revisión de la entrega de Míriam ([@Punqee](https://github.com/Punqee)) al [reto de contribución](../CHALLENGE.md), hecha por el mantenedor con Claude Code (Opus 5.5) en una máquina con Windows. Revisa lo que entregó el 13 de noviembre para atender la [quinta revisión](2026-11-12_punqee_review.md). La calificación vigente está en [score/punqee.md](../score/punqee.md).

## Datos de la entrega

| Dato | Valor |
|---|---|
| PR | [#5](https://github.com/incognia/dazner/pull/5), abierto |
| Rama | `challenge/punqee/add-windows-port`, en `0414942` |
| Base | `7c96cd4`, el `main` actual |
| *Commits* | 20 (los dieciocho de la quinta revisión, reaplicados, y dos nuevos) |
| Herramienta de IA | Antigravity con Gemini 3.8 Flash (esfuerzo alto) y Gemini 4 Argon (esfuerzo medio) |

| *Commit* | Asunto |
|---|---|
| `562dd06` | `docs(windows): document fullscreen style persistence, task 2.2 check and deferred shortcuts` |
| `0414942` | `fix(windows): keep fullscreen stylesheet attached and defer Ctrl+R and Ctrl+H to task 5.1` |

## Puertas automáticas

- **`check-pr.sh --base upstream/main`:** pasan todas las comprobaciones, con el aviso de siempre: OpenSpec no se validó porque la máquina del mantenedor no tiene Node.
- **CHANGELOG:** ya cambia solo en el último *commit*. Todas sus viñetas quedaron en un bloque `## [2026-11-13]`, la fecha de su último *commit*, una por *commit* y en orden.
- **Finales de línea:** `git ls-files --eol` no muestra ningún archivo con CRLF en el repositorio.
- **Archivos ajenos:** no toca `shared/` ni `macos/`.

## Compilación

`dotnet build windows\DAZNer.sln` en la copia de su rama: compilación correcta, 0 advertencias y 0 errores.

## Lo que atendió de la quinta revisión

- **El CHANGELOG:** corregido; `check-pr.sh` pasa.
- **La hoja de estilo de `fill-fullscreen.js`:** encontró dos causas y las documentó en la bitácora de `design.md` antes del código. Su puente escuchaba `readystatechange` en `window`, pero ese evento solo se dispara en `document`. Además, la aplicación de DAZN retira el `<style>` de `<head>` al montar sus rutas. La corrección escucha el evento en `document` y usa un `MutationObserver` que vuelve a insertar el `<style>` si desaparece, sin modificar el script compartido.
- **La tarea 4.2:** agregó a su verificación la comprobación del estilo calculado, dice que en su equipo da el resultado esperado, y la dejó sin marcar porque el *multiview* requiere la cuenta del mantenedor.
- **La tarea 2.2:** la marcó y anotó que la verificó el mantenedor en dos monitores.
- **`Ctrl+R` y `Ctrl+H`:** los retiró y los dejó para la tarea 5.1.

## Prueba en el equipo del mantenedor

Por escritorio remoto (RDP), con la cuenta del mantenedor y con `--debug`, primero en un monitor de 1920 px de ancho y después en uno más grande:

| Prueba | Resultado |
|---|---|
| Pantalla completa de un video del *multiview* | **Funciona.** Llena la ventana, se ve y no queda nada de la página. `Esc` regresa al *multiview*. |
| Pantalla completa del otro video del *multiview* | **Funciona.** Ya no se queda en su mitad. |
| Pantalla completa general con cuatro videos | **Funciona.** La cuadrícula llena la ventana, sin el resto de la página ni margen a la izquierda. `Esc` regresa. |
| Pasar a dos paneles con el *multiview* en el panel 1 | **Funciona.** Con cuatro videos en pantalla completa en el panel 1, `Ctrl+2` los conserva. |
| Volver a un panel con el *multiview* en el panel 1 | **Falla.** `Ctrl+1` deja el panel 1 con un solo video (ver «El panel 1 cambia de tamaño dos veces»). |

`debug.log` confirma el arreglo: el elemento en pantalla completa tiene `position: fixed` y `z-index: 2147483647`, los valores de la regla `.__fs` del script. Con cuatro videos en una ventana de 1280×720, la cuadrícula ocupa toda la ventana, con un `gap` de 4 px.

**Corrección de la quinta revisión:** el `DIV._15485uv0` que aparecía en el centro de la pantalla sigue ahí ahora que el video se ve, así que es una capa transparente de DAZN, probablemente la de los controles. La causa de la falla era solo la hoja de estilo.

## El panel 1 cambia de tamaño dos veces

Lo que registra `debug.log` del panel 1 con dos paneles de 1625 px y el *multiview* activo:

| Acción | Tamaños que recibe el panel 1 |
|---|---|
| `Ctrl+1` | 811 y después 1629 |
| `Ctrl+2` | 3258 y después 1625 |

La página no se recarga (no hay un nuevo `page:script-start` en el panel 1), pero DAZN rehace el reproductor con cada cambio de tamaño. Al crecer de forma momentánea al doble (`Ctrl+2`), DAZN conserva los cuatro videos. Al bajar de forma momentánea a 811 px (`Ctrl+1`), queda por debajo del ancho al que DAZN deja de ofrecer el *multiview* (a 956 px ya oculta el botón MultiView), y DAZN lo abandona y deja un solo video. Con un solo video el cambio no se nota, y por eso pasó la prueba de la tarea 3.2 en la cuarta revisión.

**Por qué pasa:** en WPF, `SetWindowPos` provoca un *layout* inmediato. Si la ventana cambia antes que las columnas, durante un instante el panel 1 ocupa toda la ventana nueva o la mitad de su columna vieja. Si cambian primero las columnas, pasa lo mismo al revés. En macOS los dos cambios se aplican en una sola pasada de *layout* y el panel nunca ve un tamaño intermedio.

**No es una falla de la participante.** La cuarta revisión le pidió usar el mismo orden que macOS, y ella lo hizo. El problema es que en WPF el orden no alcanza.

**Una forma de resolverlo:** fijar la columna del panel 1 a su ancho actual en píxeles antes de la transición, cambiar la ventana, agregar o quitar la otra columna y solo al final devolver la columna del panel 1 a `*`. Así el panel 1 cambia solo los 4 px del hueco. La especificación no exige este método; cualquier otro que evite el tamaño intermedio sirve.

**Un límite aparte, que no es de esta versión:** con una ventana de 1280 px en un monitor de 1920 px de área útil, la ventana no puede crecer al doble y el panel 1 baja de 1280 a 956 px. macOS usa la misma fórmula con el mismo límite. A ese ancho, DAZN también oculta el botón MultiView. Ninguna de las dos cosas se le pide a la participante.

## Observaciones

1. **La tarea 3.2 se reabre** por el tamaño intermedio del panel 1 (ver la sección anterior).
2. **El `MutationObserver` vigila todo el documento** (`subtree: true`), así que corre con cada cambio de la página de DAZN. Lo que hace en cada llamada es barato, pero bastaría con vigilar `<head>` y `documentElement`. No bloquea.
3. **El hueco del *multiview*.** Con cuatro videos en 1280×720, entre los bordes de los videos hay 13 px en horizontal y 10 px en vertical, y AGENTS.md anota 8 px, medidos en macOS. La misma distribución aparece cuando DAZN usa su propia pantalla completa, así que probablemente viene de la cuadrícula de DAZN y no de esta versión. El mantenedor la comparará con macOS; no se le pide nada a la participante.

## Qué se le pide a la participante

1. Evitar el tamaño intermedio del panel 1 al cambiar de paneles, en especial al volver a uno, primero en `design.md` y `tasks.md` y después en el código. Agregar a la verificación de la tarea 3.2 que el *multiview* con cuatro videos sobreviva a `Ctrl+2` y a `Ctrl+1`. Sin *multiview*, se puede comprobar con `--debug`: al cambiar de paneles, `debug.log` debe registrar un solo `page:resize` del panel 1 y no uno a la mitad o al doble de su ancho. El mantenedor vuelve a probarlo con su cuenta.
2. Opcional: limitar el `MutationObserver` a `<head>` y `documentElement`.
3. Seguir con el grupo 5 (atajos, paquete y documentación), igual que hasta ahora.

## Veredicto

- **Quinta revisión: atendida por completo.** `check-pr.sh` pasa.
- **Tarea 4.2: verificada.** La pantalla completa funciona con el video principal, con un video del *multiview*, con la general de cuatro videos, en uno y dos paneles, y `Esc` regresa en todos los casos.
- **Tarea 3.2: se reabre.** Funciona con un solo video y al pasar a dos paneles con *multiview*, pero el *multiview* del panel 1 no sobrevive al volver a un panel.
- **Avance verificado:** 13 de las 18 tareas de `add-windows-port` (1.1 a 1.4, 2.1 a 2.3, 3.1, 3.3, 3.4 y 4.1 a 4.3). Quedan la 3.2 y de la 5.1 a la 5.4.

## Entorno del mantenedor

| Componente | Versión |
|---|---|
| Sistema | Windows 11 Pro 10.0.26300, usado por RDP |
| SDK de .NET | 10.0.403 |
| *Runtime* de WebView2 | 157.0.4341.42 |
| Visual Studio | no instalado |
| Git | Git para Windows, con `.gitattributes` y la configuración de `/git-init` |
