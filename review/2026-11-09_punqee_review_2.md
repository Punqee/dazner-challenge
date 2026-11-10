# Tercera revisión del reto: Punqee

*Última modificación: 9 de noviembre de 2026, 20:54 (CST)*

Tercera revisión de la entrega de Míriam ([@Punqee](https://github.com/Punqee)) al [reto de contribución](../CHALLENGE.md), hecha por el mantenedor con Claude Code (Opus 5.5) en una máquina con Windows. Revisa lo que entregó la noche del 9 de noviembre, después de la [segunda revisión](2026-11-09_punqee_review.md) de esa madrugada. La calificación vigente está en [score/punqee.md](../score/punqee.md).

## Datos de la entrega

| Dato | Valor |
|---|---|
| PR | [#5](https://github.com/incognia/dazner/pull/5), abierto |
| Rama | `challenge/punqee/add-windows-port`, en `6891cdc` |
| Base | `3054a7a`, el `main` actual |
| Último *push* | 9 de noviembre de 2026, con reescritura de la rama sobre el `main` actual |
| Herramienta de IA | Antigravity con Gemini 3.8 Flash (esfuerzo alto) y Gemini 4 Argon (esfuerzo medio) |

La rama tiene 10 *commits*. Los seis primeros son los de la segunda revisión, reaplicados, y en ellos el renombre del proyecto ya está en su *commit* `refactor`. Los cuatro nuevos:

| *Commit* | Asunto |
|---|---|
| `36f22cd` | `docs(windows): update design and tasks following second review` |
| `d868ca0` | `refactor(windows): remove unused drag border and mouse handlers` |
| `84a723c` | `feat(windows): enforce fixed aspect ratio on window resize` |
| `6891cdc` | `feat(windows): add fill and center and always-on-top window actions` |

## Puertas automáticas

- **`check-pr.sh --base upstream/main`:** pasan las 16 comprobaciones, con el aviso de siempre: OpenSpec no se validó porque la máquina del mantenedor no tiene Node.
- **Finales de línea:** `git ls-files --eol` no muestra ningún archivo con CRLF en el repositorio.
- **CHANGELOG:** sus viñetas están dentro del bloque del 9 de noviembre que ya existía en `main`, arriba de las del mantenedor, una por *commit* y en el orden de los *commits*.

## Compilación

`dotnet build windows\DAZNer.sln` en la copia de su rama: compilación correcta, 0 advertencias y 0 errores. La compilación en Visual Studio sigue sin verificarse, porque la máquina del mantenedor no lo tiene.

## Lo que atendió de la segunda revisión

- **La afirmación sobre la calidad:** `design.md` ya no dice que el mantenedor verificó la alta definición; dice que la resolución no se midió.
- **El riesgo del *hook*:** está en «Risks / Trade-offs» de `design.md`, con la alternativa de `IsNonClientRegionSupportEnabled` y `app-region: drag`.
- **El *commit* mezclado:** el renombre de `windows.csproj` y `windows.sln` pasó al *commit* `refactor`.
- **El CHANGELOG:** tiene la viñeta que faltaba y todas están en orden.
- **El código que no se ejecutaba:** se retiraron el `Border` y los dos manejadores de ratón de WPF.
- **La tarea 2.1 en el PR:** el PR detalla los tres escenarios de «Mover la ventana» que probó.
- **Menores:** se quitó la ruta personal de `design.md`.

Todo se hizo por la especificación: `36f22cd` actualiza `design.md` y `tasks.md` antes de tocar el código.

## Lo nuevo: tareas 2.2 y 2.3

- **Proporción fija (tarea 2.2):** maneja `WM_SIZING` para conservar 16:9 con un panel y 32:9 con dos al arrastrar bordes y esquinas. Como WebView2 cubre toda la ventana y no deja llegar el clic a los bordes, desactiva un momento el control (`EnableWindow`) mientras el puntero está en el borde. La bitácora de `design.md` explica el problema y la solución. Dejó la tarea **sin marcar**, porque solo tiene un monitor y los dos paneles son del grupo 3.
- **Rellenar y centrar, y siempre encima (tarea 2.3):** `Ctrl+F` calcula el mayor rectángulo con la proporción actual dentro del área útil del monitor y lo centra; `Ctrl+T` alterna `Topmost`. Son las teclas que fija la especificación de atajos para Windows. Marcó la tarea como hecha.

## Prueba en el equipo del mantenedor

El mantenedor abrió el ejecutable compilado, por escritorio remoto (RDP), y todo funcionó:

- **Proporción fija:** la ventana conserva 16:9 al arrastrar los cuatro bordes y las esquinas. Es el problema que se encontró en la primera revisión.
- **`Ctrl+F`:** con un video reproduciéndose, llena y centra la ventana en el área útil del monitor.
- **`Ctrl+T`:** deja la ventana siempre encima, y al repetirlo la regresa a su nivel normal.
- **Clics junto al borde:** los controles de DAZN pegados al borde responden, aunque el control se desactive ahí para redimensionar.
- **Lo de la tarea 2.1:** el arrastre desde la franja, `Alt` + arrastrar y el doble clic siguen funcionando.

## Observaciones

1. **Falta un escenario de la tarea 2.3.** «Pantalla más alta que la proporción» de `window-chrome` no se probó: en un monitor de 16:9 con un panel no se presenta, y con dos paneles (32:9) sí. La tarea puede quedar marcada, pero el escenario debe ir en «Qué no se probó» del PR; el mantenedor lo verifica cuando exista el segundo panel.
2. **El *hook* trabaja más.** Ahora procesa cada movimiento del ratón del sistema y en cada uno consulta el rectángulo y los DPI de la ventana para saber si el puntero está en el borde. Aumenta el riesgo que ya está documentado y que el mantenedor asume, porque su equipo con Windows no tiene monitor y solo se usa por RDP. No bloquea el PR; se le sugiere consultar solo cuando el puntero esté sobre la ventana, o guardar el rectángulo al moverla o redimensionarla.

## Qué se le pide a la participante

1. Anotar en «Qué no se probó» del PR el escenario «Pantalla más alta que la proporción» de la tarea 2.3.
2. Seguir con el grupo 3 (paneles), primero `design.md` y `tasks.md` y después el código.
3. Opcional: aligerar el trabajo del *hook* en cada movimiento del ratón.

## Veredicto

- **Segunda revisión: atendida por completo.**
- **Tarea 2.3: verificada** en el equipo del mantenedor, salvo el escenario de la observación 1.
- **Tarea 2.2: funciona con un panel en un monitor.** Sigue abierta hasta probarla con dos paneles y en dos monitores.
- **Avance verificado:** 6 de las 18 tareas de `add-windows-port` (1.1 a 1.4, 2.1 y 2.3).
- **El PR sigue bien encaminado.** Puede seguir con el grupo 3.

## Entorno del mantenedor

| Componente | Versión |
|---|---|
| Sistema | Windows 11 Pro 10.0.26300, usado por RDP |
| SDK de .NET | 10.0.403 |
| *Runtime* de WebView2 | 157.0.4341.42 |
| Visual Studio | no instalado |
| Git | Git para Windows, con `.gitattributes` y la configuración de `/git-init` |
