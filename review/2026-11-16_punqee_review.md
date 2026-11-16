# Séptima revisión del reto: Punqee

*Última modificación: 16 de noviembre de 2026, 00:10 (CST)*

Séptima revisión de la entrega de Míriam ([@Punqee](https://github.com/Punqee)) al [reto de contribución](../CHALLENGE.md), hecha por el mantenedor con Claude Code (Opus 5.5) en una máquina con Windows. Revisa lo que entregó el 15 de noviembre para atender la [sexta revisión](2026-11-15_punqee_review.md): el tamaño intermedio del panel 1 y el grupo 5 (atajos, paquete y documentación). La calificación vigente está en [score/punqee.md](../score/punqee.md).

## Datos de la entrega

| Dato | Valor |
|---|---|
| PR | [#5](https://github.com/incognia/dazner/pull/5), abierto |
| Rama | `challenge/punqee/add-windows-port`, en `4eb3cbf` |
| Base | `7d5cff4`, el `main` actual |
| *Commits* | 25 (los veinte de la sexta revisión, reaplicados, y cinco nuevos) |
| Herramienta de IA | Antigravity con Gemini 3.8 Flash (esfuerzo alto) y Gemini 4 Argon (esfuerzo medio) |

| *Commit* | Asunto |
|---|---|
| `6e08e19` | `docs(windows): update design and tasks following sixth review` |
| `8ed7c6d` | `fix(windows): lock first pane width during pane transitions and narrow style observer` |
| `4d98342` | `feat(windows): add reload, home, close and quit shortcuts` |
| `cb4da6b` | `feat(windows): add application icon and PowerShell build script` |
| `4eb3cbf` | `docs(windows): document Windows build and usage in README and ROADMAP and record parity check` |

## Puertas automáticas

- **`check-pr.sh --base upstream/main`:** pasan todas las comprobaciones, con el aviso de siempre: OpenSpec no se validó porque la máquina del mantenedor no tiene Node.
- **CHANGELOG:** cambia solo en el último *commit*. Como su último *commit* es del 15 de noviembre y ese bloque ya existía en `main`, puso sus viñetas dentro de él, arriba de las del mantenedor, como indica `score/punqee.md`: solo puede haber un bloque por fecha.
- **Finales de línea:** `git ls-files --eol` no muestra ningún archivo con CRLF en el repositorio.
- **Archivos ajenos:** no toca `shared/` ni `macos/`.

## Compilación

- **`dotnet build windows\DAZNer.sln`** en la copia de su rama: compilación correcta, 0 advertencias y 0 errores.
- **`windows\build.ps1` en dos clones nuevos**, siguiendo el README:
  - Con Windows PowerShell 5.1 (`powershell -ExecutionPolicy Bypass -File .\windows\build.ps1`): compilación correcta en `Release`, 0 advertencias y 0 errores, sin caracteres corruptos.
  - Con PowerShell 7 (`pwsh -File .\windows\build.ps1`): igual.
  - En los dos casos, `fill-fullscreen.js` queda junto a `DAZNer.exe` y es idéntico al de `shared/`.
- **El *script*** está en UTF-8 con BOM, con finales de línea LF y sin caracteres fuera de ASCII.
- **El ícono** (`windows/app.ico`) trae siete tamaños, de 16 a 256 px, generados desde `shared/assets/icon.png`.

La participante no tiene PowerShell 7 y lo declaró como excepción de entorno. Con esta prueba, esa parte de la tarea 5.2 queda verificada en el equipo del mantenedor. La compilación en Visual Studio la verificó ella (MSBuild 18.11.1); el mantenedor no tiene Visual Studio.

## Lo que atendió de la sexta revisión

- **El tamaño intermedio del panel 1:** lo documentó en `design.md`, en el diseño y en la bitácora, antes del código. Antes de la transición fija la columna del panel 1 a su ancho en píxeles, cambia la ventana, agrega o quita la otra columna y al final devuelve la columna a `*`. Agregó a la tarea 3.2 la comprobación de `page:resize` y la del *multiview*, y la dejó sin marcar porque el *multiview* lo prueba el mantenedor.
- **La tarea 4.2:** la marcó y anotó que el mantenedor verificó el *multiview*.
- **El `MutationObserver` (opcional):** ahora vigila solo `<head>` y `documentElement`, sin `subtree`.
- **El grupo 5:** lo entregó completo, con su diseño en `design.md` antes del código y la lista de verificación de paridad.

## Lo nuevo: grupo 5

- **Atajos (tarea 5.1):** `Ctrl+R` recarga y `Ctrl+H` lleva al inicio de DAZN, los dos en el panel con foco; `Ctrl+W` cierra la ventana y `Ctrl+Q` sale de la aplicación. Los atajos de edición no se interceptan y los resuelven WebView2 y el diálogo de `Ctrl+L`.
- **Paquete (tarea 5.2):** el ícono, integrado en `DAZNer.csproj` y en `MainWindow.xaml`, y `windows/build.ps1`, que comprueba el script compartido, regenera el ícono si falta, compila en `Release`, copia el script y admite `--open`.
- **Documentación (tarea 5.3):** el README explica la compilación con `build.ps1` y con Visual Studio, el uso con los atajos de Windows, el modo de diagnóstico y la carpeta de la sesión. `ROADMAP.md` marca Windows como disponible y llena su columna en la tabla de funciones.
- **Paridad (tarea 5.4):** una tabla en `design.md` con cada requisito de las capacidades, su estado en Windows y cómo se verificó, con PowerShell 7 como única excepción.

## Prueba en el equipo del mantenedor

Por escritorio remoto (RDP), en el monitor grande, con la cuenta del mantenedor y con `--debug`:

| Prueba | Resultado |
|---|---|
| Volver a un panel con el *multiview* en el panel 1 | **Funciona.** Con cuatro videos en pantalla completa en el panel 1, `Ctrl+2` y después `Ctrl+1` conservan el *multiview*. |
| `Ctrl+R` y `Ctrl+H` con el foco en el panel 2 | **Funcionan.** Solo el panel 2 se recarga y va al inicio; el panel 1 sigue reproduciendo. |
| Edición de texto | **Funciona.** En un campo de DAZN, `Ctrl+A`, `Ctrl+C`, `Ctrl+V`, `Ctrl+Z` y el menú contextual; en el diálogo de `Ctrl+L`, pegar con `Ctrl+V`. |
| `Ctrl+W` y `Ctrl+Q` | **Funcionan.** `Ctrl+W` cierra la ventana y `Ctrl+Q` sale de la aplicación. |
| Reabrir sin `--debug` | **Funciona.** Abre con la sesión iniciada. |
| Ícono | **Funciona.** Aparece en la barra de tareas y en `Alt+Tab`. |

`debug.log` confirma el arreglo: con la ventana de 1280 px, el panel 1 solo pasa de 1280 a 1276 px al abrir el segundo panel y de 1276 a 1280 px al volver a uno. Son los 4 px del hueco, sin tamaños a la mitad ni al doble. Al abrir el segundo panel registra dos veces el mismo ancho (1276), lo que no afecta.

## Observaciones

1. **La tarea 5.3 pide `openspec validate add-windows-port --strict`,** que nadie ha podido correr: ni la participante ni el mantenedor tienen Node. Lo declaró en `design.md`. El flujo de GitHub Actions del PR instala Node y corre `check-pr.sh`, que incluye esa validación; está retenido desde la primera revisión hasta que el mantenedor lo apruebe, y ahora que el PR está completo es el momento de hacerlo. No se le pide nada a la participante si el flujo pasa.
2. **Dos listas de tamaños del ícono.** `design.md` dice en «Decisions» que el ícono tiene seis tamaños (sin 24 px) y en la verificación de la tarea 5.2, siete. El archivo trae siete. Es menor.
3. **`ROADMAP.md`** marca Windows como disponible, pero su introducción todavía dice que describe cómo llevar el visor a Windows, y la sección «Orden de ejecución» no se actualizó. Es menor; el mantenedor puede ajustarlo al integrar.
4. **El hueco del *multiview*** (13/10 px contra los 8 px de macOS) sigue pendiente de comparar en macOS, como dice la sexta revisión. No se le pide nada a la participante.

## Qué se le pide a la participante

1. Marcar la tarea 3.2 y anotar en el PR que el mantenedor verificó el *multiview* con `Ctrl+2` y `Ctrl+1`.
2. Corregir en `design.md` la lista de tamaños del ícono para que diga los siete que trae el archivo.
3. Si el flujo de GitHub Actions marca un error de OpenSpec, corregirlo. Si pasa, no hay nada más que hacer.

## Veredicto

- **Sexta revisión: atendida por completo**, incluida la sugerencia opcional del `MutationObserver`.
- **Tarea 3.2: verificada.** El *multiview* del panel 1 sobrevive a `Ctrl+2` y a `Ctrl+1`, y el panel 1 solo cambia los 4 px del hueco.
- **Tareas 5.1, 5.2 y 5.4: verificadas** en el equipo del mantenedor, la 5.2 también con PowerShell 7.
- **Tarea 5.3:** la documentación está completa; falta solo la validación de OpenSpec, que corre el flujo de GitHub Actions.
- **Avance verificado:** 17 de las 18 tareas de `add-windows-port` (1.1 a 1.4, 2.1 a 2.3, 3.1 a 3.4, 4.1 a 4.3, 5.1, 5.2 y 5.4). Queda la 5.3, pendiente de la validación de OpenSpec.
- **El PR está listo para integrarse** en cuanto el flujo de GitHub Actions pase y se atiendan los dos puntos menores de arriba.

## Entorno del mantenedor

| Componente | Versión |
|---|---|
| Sistema | Windows 11 Pro 10.0.26300, usado por RDP |
| SDK de .NET | 10.0.403 |
| *Runtime* de WebView2 | 157.0.4341.42 |
| Windows PowerShell | 5.1 |
| PowerShell | 7 |
| Visual Studio | no instalado |
| Git | Git para Windows, con `.gitattributes` y la configuración de `/git-init` |
