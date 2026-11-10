# Cuarta revisión del reto: Punqee

*Última modificación: 9 de noviembre de 2026, 22:50 (CST)*

Cuarta revisión de la entrega de Míriam ([@Punqee](https://github.com/Punqee)) al [reto de contribución](../CHALLENGE.md), hecha por el mantenedor con Claude Code (Opus 5.5) en una máquina con Windows. Revisa el grupo 3 (paneles), que entregó la noche del 9 de noviembre, después de la [tercera revisión](2026-11-09_punqee_review_2.md). La calificación vigente está en [score/punqee.md](../score/punqee.md).

## Datos de la entrega

| Dato | Valor |
|---|---|
| PR | [#5](https://github.com/incognia/dazner/pull/5), abierto |
| Rama | `challenge/punqee/add-windows-port`, en `1bc1891` |
| Base | `32c3faf`, el `main` actual |
| Último *push* | 9 de noviembre de 2026, con reescritura de la rama sobre el `main` actual |
| Herramienta de IA | Antigravity con Gemini 3.8 Flash (esfuerzo alto) y Gemini 4 Argon (esfuerzo medio) |

La rama tiene 14 *commits*: los diez de la tercera revisión, reaplicados, y cuatro nuevos:

| *Commit* | Asunto |
|---|---|
| `c95fd37` | `docs(windows): update design for mouse hook caching and pane management` |
| `27ce634` | `perf(windows): cache window bounds and DPI scale for low-level mouse hook` |
| `24df4aa` | `feat(windows): add one and two-pane layout with on-demand lifecycle` |
| `1bc1891` | `feat(windows): add focus audio muting, pane switching, URL dialog and same-pane links` |

## Puertas automáticas

- **`check-pr.sh --base upstream/main`:** pasan las 16 comprobaciones, con el aviso de siempre: OpenSpec no se validó porque la máquina del mantenedor no tiene Node.
- **Finales de línea:** `git ls-files --eol` no muestra ningún archivo con CRLF en el repositorio. En disco, solo `DAZNer.sln` está en CRLF, como fija `.gitattributes`.
- **CHANGELOG:** una viñeta por *commit*, en el orden de los *commits*, dentro del bloque del 9 de noviembre.

## Compilación

`dotnet build windows\DAZNer.sln` en la copia de su rama: compilación correcta, 0 advertencias y 0 errores. La compilación en Visual Studio sigue sin verificarse, porque la máquina del mantenedor no lo tiene.

## Lo que atendió de la tercera revisión

- **El escenario que faltaba de la tarea 2.3:** probó «Pantalla más alta que la proporción» con dos paneles (ventana de 1920×540 centrada en vertical) y anotó en «Qué no se probó» que con un panel requiere un monitor más alto que 16:9.
- **El trabajo del *hook* (opcional):** el rectángulo y la escala de DPI de la ventana se guardan al recibir `WM_MOVE`, `WM_SIZE` o `WM_DPICHANGED`, y el *hook* ya no llama a `GetWindowRect` ni a `GetDpiForWindow` en cada movimiento del ratón. Lo documentó en «Risks / Trade-offs» de `design.md`.
- **El grupo 3 por la especificación:** `c95fd37` actualiza `design.md` antes del código, y cada tarea se marca en el *commit* que la implementa, con su prueba descrita en el PR.

## Lo nuevo: grupo 3 (paneles)

- **Uno y dos paneles (tarea 3.1):** `Ctrl+1` y `Ctrl+2`. Un `Grid` con dos columnas de contenido y una de 8 px dentro del ancho de la ventana, de modo que cada panel mide (W − 8) / 2. La ventana pasa de 16:9 a 32:9 con la misma fórmula que la versión de macOS, limitada al ancho del área útil del monitor.
- **Segundo panel a demanda (tarea 3.2):** el segundo `WebView2` se crea al pedir dos paneles y, al volver a uno, se detiene (`Stop`), se quita del `Grid` y se libera (`Dispose`).
- **Audio por foco, cambio de panel y abrir dirección (tarea 3.3):** `CoreWebView2.IsMuted` silencia el panel sin foco sin tocar el silencio del reproductor. El foco cambia con un clic en el otro panel o con un atajo, y `Ctrl+L` abre un diálogo para cargar una dirección en el panel con foco.
- **Enlaces de ventana nueva (tarea 3.4):** `NewWindowRequested` marca el evento como atendido y navega en el mismo panel.

## Prueba en el equipo del mantenedor

El mantenedor abrió el ejecutable compilado, por escritorio remoto (RDP), y todo funcionó:

1. **Abrir el segundo panel:** con un video en el panel 1, `Ctrl+2` abre el segundo panel a la derecha, la ventana pasa a 32:9 y **el panel 1 sigue reproduciendo sin recargarse**.
2. **Audio por foco:** solo suena el panel en el que se hizo clic; `Ctrl+Tab` también pasa el foco y el audio.
3. **Silencio manual:** un panel silenciado desde el reproductor de DAZN sigue silenciado al recibir el foco.
4. **Abrir dirección:** `Ctrl+L` carga la dirección solo en el panel con foco.
5. **Enlaces de ventana nueva:** «Ayuda», en el pie de página de DAZN, se abre en el mismo panel, sin otra ventana.
6. **Bordes y `Ctrl+F` con dos paneles:** la ventana conserva 32:9 al arrastrar bordes y esquinas, y `Ctrl+F` la llena a lo ancho y la centra. Con esto se cumple el escenario «Pantalla más alta que la proporción» de la tarea 2.3.
7. **Volver a un panel:** `Ctrl+1` quita el segundo panel y el primero sigue reproduciendo y con sonido.

## Observaciones

1. **Orden al abrir el segundo panel.** macOS cambia primero el tamaño de la ventana y después el *layout*, para que DAZN no rehaga el reproductor del primer panel. La versión de Windows ajusta primero las columnas del `Grid` y después la ventana. En la prueba el primer panel no se recargó, porque WPF calcula el *layout* después de `SetWindowPos`, pero el orden distinto del de macOS es frágil. Se sugiere invertirlo o explicar en `design.md` por qué no afecta.
2. **Teclas de más para cambiar de panel.** La especificación de atajos pide `` Ctrl+` ``. El código acepta seis teclas (`Oem3`, `Oem5`, `Oem1`, `OemOpenBrackets`, `OemBackslash` y `Tab`), y `design.md` menciona solo tres (`` Ctrl+` ``, `Ctrl+|` y `Ctrl+Tab`). Es razonable cubrir varias distribuciones de teclado, pero cada tecla debe estar documentada en `design.md` con la distribución que la motiva, o retirarse. Conviene revisarlo con la tarea 5.1 (atajos).
3. **La tarea 2.2 en dos monitores.** Con esta entrega la proporción fija queda probada con uno y con dos paneles. Solo falta el escenario en un segundo monitor, que ni la participante ni el mantenedor (su equipo no tiene monitor y se usa por RDP) pueden probar hoy. El mantenedor decide cómo cerrarla.

## Qué se le pide a la participante

1. Resolver la observación 1: cambiar primero el tamaño de la ventana y después las columnas, como macOS, o justificar el orden en `design.md`.
2. Documentar en `design.md` las teclas de «siguiente panel» que acepta el código, o quitar las que sobran.
3. Seguir con el grupo 4 (pantalla completa y sesión), primero `design.md` y `tasks.md` y después el código.

## Veredicto

- **Tercera revisión: atendida por completo**, incluida la sugerencia opcional del *hook*.
- **Tarea 2.3: verificada por completo**, ahora también con el escenario «Pantalla más alta que la proporción».
- **Grupo 3 (tareas 3.1 a 3.4): verificado** en el equipo del mantenedor.
- **Tarea 2.2:** funciona con uno y dos paneles en un monitor; sigue abierta por el escenario de dos monitores.
- **Avance verificado:** 10 de las 18 tareas de `add-windows-port` (1.1 a 1.4, 2.1, 2.3 y 3.1 a 3.4).
- **El PR sigue bien encaminado.** Puede seguir con el grupo 4.

## Entorno del mantenedor

| Componente | Versión |
|---|---|
| Sistema | Windows 11 Pro 10.0.26300, usado por RDP |
| SDK de .NET | 10.0.403 |
| *Runtime* de WebView2 | 157.0.4341.42 |
| Visual Studio | no instalado |
| Git | Git para Windows, con `.gitattributes` y la configuración de `/git-init` |
