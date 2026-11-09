# Segunda revisión del reto: Punqee

*Última modificación: 9 de noviembre de 2026, 00:31 (CST)*

Segunda revisión de la entrega de Míriam ([@Punqee](https://github.com/Punqee)) al [reto de contribución](../CHALLENGE.md), hecha por el mantenedor con Claude Code (Opus 5.5) en una máquina con Windows. Revisa la rama que reescribió después de la [primera revisión](../score/punqee.md): qué atendió, qué se ejecutó y qué falta.

## Datos de la entrega

| Dato | Valor |
|---|---|
| PR | [#5](https://github.com/incognia/dazner/pull/5), abierto |
| Rama | `challenge/punqee/add-windows-port`, en `f65a165` |
| Base | `d6d166b`, el `main` actual |
| *Pushes* | 7 de noviembre de 2026 por la noche y 9 de noviembre a las 00:15 (hora del centro de México), con reescritura de la rama |
| Herramienta de IA | Antigravity con Gemini 3.8 Flash (esfuerzo alto) y Gemini 4 Argon (esfuerzo medio) |

*Commits*, del más antiguo al más reciente:

| *Commit* | Asunto |
|---|---|
| `2872229` | `feat(windows): add minimal WPF project with WebView2` |
| `592d7c8` | `docs(windows): document playback test results` |
| `8908175` | `docs(windows): update change specification and design following review` |
| `89ad385` | `docs(windows): document build requirements and steps in README` |
| `f1969a6` | `refactor(windows): rename project to DAZNer and clean up template code` |
| `f65a165` | `feat(windows): add borderless window with drag strip and Alt drag` |

## Puertas automáticas

- **`check-pr.sh --base upstream/main`:** pasan las 16 comprobaciones, con el aviso de siempre: OpenSpec no se validó porque la máquina del mantenedor no tiene Node.
- **OpenSpec en el PR:** la casilla de `openspec validate --all --strict` quedó sin marcar, con la nota de que le falta Node. Antes se marcaba como aprobada sin poder comprobarse; ahora es honesta.
- **Finales de línea y codificación:** `git ls-files --eol` no muestra ningún archivo con CRLF en el repositorio, y los `.cs` y `.xaml` tienen BOM, como fija `.editorconfig`.
- **GitHub Actions:** sigue retenida hasta que el mantenedor la apruebe.

## Compilación sin intervención

Clon nuevo de la rama en `f65a165` y, siguiendo solo la sección «Compilar (Windows)» del README:

```powershell
dotnet build windows\DAZNer.sln
```

Resultado: compilación correcta, 0 advertencias y 0 errores, en unos 6 segundos. El SDK de .NET 10 ya estaba instalado desde la primera revisión, así que no se volvió a probar su instalación; el README ahora lo pide con su comando de `winget`. La compilación en Visual Studio no se verificó, porque la máquina del mantenedor no lo tiene.

## Prueba en el equipo del mantenedor

El mantenedor abrió el ejecutable compilado del clon nuevo, por escritorio remoto (RDP), y probó:

- **Clic:** la ventana abre y responde al clic.
- **Arrastre:** la ventana se mueve al arrastrarla.
- **Alt + arrastrar:** mueve la ventana desde cualquier punto.
- **Doble clic:** maximiza la ventana, y un segundo doble clic la regresa a su tamaño anterior.

- **El ratón fuera de DAZNer:** no se percibió ningún cambio en otras aplicaciones con DAZNer abierto. La prueba se hizo por escritorio remoto (RDP), cuya latencia puede ocultar un retraso pequeño del *hook*, así que no descarta el riesgo de la observación 5. El equipo de Windows del mantenedor no tiene monitor ni teclado y solo se usa por RDP, así que la prueba local no se hará: **el mantenedor asume ese riesgo**.
- **Maximizada:** la ventana no tapa la barra de tareas.

## Evidencia

El PR agrega una segunda foto: tomada con un Samsung Galaxy S24, en horizontal, el 8 de noviembre de 2026 a las 23:39. Muestra NFL Network en vivo (Lions contra Dolphins) dentro de una ventana sin barra de título, con la cuenta limitada de la participante (el botón «Suscribir» sigue visible). **Esta foto sí prueba la reproducción**, que era lo que faltaba en la primera entrega.

## Lo que atendió de la primera revisión

- ***Rebase* sobre `main`**, sin `merge` ni `fixup!`, y con `.gitattributes` respetado.
- **Primero la especificación y después el código:** `8908175` actualiza `design.md`, `proposal.md` y `tasks.md` antes del renombre y de la ventana.
- **Requisitos en el README:** sección «Compilar (Windows)» con el SDK de .NET 10, el *runtime* de WebView2, el comando de compilación y la ruta del ejecutable.
- **`design.md`:** responde las preguntas de Windows del reto, agrega los resultados de la prueba con las versiones de ambos entornos y una bitácora de problemas con su formato.
- **Limpieza del proyecto:** se llama `DAZNer`, `xmlns:d` usa el espacio de nombres real de Blend y se quitaron los `using` que sobraban.
- **El PR:** el cuerpo ya se ve con formato, «Riesgos» lista riesgos reales y «Decisiones» declara la cuenta de acceso limitado.
- **Tareas sin probar:** lo que depende de NFL Game Pass y los grupos 3 a 5 están en «Qué no se probó».

## Observaciones

1. **Afirma algo que el mantenedor no verificó.** En las preguntas abiertas de `design.md` dice que la reproducción de eventos de pago «en alta definición fue verificada por el mantenedor». El mantenedor confirmó que reproduce con su cuenta, pero nadie midió la calidad. Hay que quitar «en alta definición» o decir que no se midió.
2. **Un *commit* mezcla dos cambios.** `89ad385` (docs, README) incluye el renombre de `windows.csproj` y `windows.sln` a `DAZNer`, que corresponde al `refactor` de `f1969a6`.
3. **CHANGELOG incompleto:** no hay viñeta para `8908175` (la actualización de la especificación y del diseño), y las dos viñetas de abajo están en orden inverso al de los *commits*.
4. **Código que no se ejecuta.** El *hook* de ratón se queda con los clics de la franja y los de Alt, así que `DragStrip_MouseLeftButtonDown`, `Window_PreviewMouseLeftButtonDown` y el `Border` de `MainWindow.xaml` nunca reciben el clic. Sobran o hay que justificarlos.
5. **Riesgo del *hook* sin documentar.** `WH_MOUSE_LL` recibe todos los movimientos del ratón del sistema, no solo los de DAZNer, y corre en el hilo de la interfaz. Si la aplicación se traba, el ratón de todo Windows se pone lento; si se traba más que el límite de Windows (`LowLevelHooksTimeout`), Windows quita el *hook* sin avisar y el arrastre deja de funcionar. La decisión se puede defender por el problema de *airspace* de WebView2, que la bitácora explica bien, pero el riesgo debe ir en «Risks» de `design.md`. El mantenedor lo asume, porque no puede probarlo en una sesión local; documentarlo sirve a quien use DAZNer frente a su equipo. Una alternativa a evaluar para la franja es la región de arrastre que WebView2 permite con `IsNonClientRegionSupportEnabled` y la propiedad CSS `app-region: drag`.
6. **Detalles menores:** `design.md` incluye una ruta personal (`C:\Users\punqe\...`), y en el README los bloques de código dentro de la lista no tienen una línea en blanco antes.

## Qué se le pide a la participante

1. Corregir la afirmación sobre la alta definición en `design.md`.
2. Agregar a «Risks» de `design.md` el riesgo del *hook* de ratón, y quitar o justificar el código que no se ejecuta.
3. Mover el renombre del proyecto al *commit* `refactor` y completar el CHANGELOG con la viñeta que falta, en el orden de los *commits*.
4. Confirmar en el PR qué probó de la tarea 2.1: el arrastre desde la franja, Alt + arrastrar y el doble clic para maximizar y restaurar.

Todo se atiende como indica la primera revisión: primero `design.md` y `tasks.md`, después el código, *commits* atómicos reescribiendo su propia rama y `git push --force-with-lease`.

## Calificación

La calificación de la [primera revisión](../score/punqee.md) se refiere a la entrega tal como se presentó entonces y no cambia. Esta revisión registra el avance; la decisión sobre la nueva calificación queda a cargo del mantenedor.

## Veredicto

- **Prueba de reproducción (grupo 1): superada**, ahora también con evidencia de la participante.
- **Tarea 2.1: funciona en el equipo del mantenedor:** la ventana responde al clic, se arrastra, se mueve con Alt desde cualquier punto y alterna entre maximizada y su tamaño anterior con doble clic. Se cumplen los tres escenarios de «Mover la ventana».
- **El PR va bien encaminado.** Se puede integrar cuando atienda las observaciones 1 a 4; las de la 5 y la 6 pueden atenderse en el mismo ciclo.

## Entorno del mantenedor

| Componente | Versión |
|---|---|
| Sistema | Windows 11 Pro 10.0.26300 |
| SDK de .NET | 10.0.403 |
| *Runtime* de WebView2 | 157.0.4341.42 |
| Visual Studio | no instalado |
| Git | Git para Windows, con `.gitattributes` y la configuración de `/git-init` |
