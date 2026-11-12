# Evaluación del reto: Punqee

*Última modificación: 12 de noviembre de 2026, 00:24 (CST)*

Revisión de la entrega de Míriam ([@Punqee](https://github.com/Punqee)) al [reto de contribución](../CHALLENGE.md), hecha por el mantenedor con Claude Code (Opus 5.5) en una máquina con Windows. Registra lo que se ejecutó, lo que se observó y una propuesta de calificación con los criterios del reto.

Cómo leer este archivo: las secciones hasta «Veredicto y siguientes pasos» describen la **primera revisión**, tal como estaba la rama entonces, y hablan de la participante en tercera persona; la excepción es «Calificación actual», que se actualiza con cada entrega. Desde «Cómo atender esta revisión» el texto se dirige a la participante («tú») e incluye lo que se pide hoy. Si algo de una revisión contradice a otra posterior, manda la más reciente.

## Datos de la entrega

| Dato | Valor |
|---|---|
| Cuenta | [@Punqee](https://github.com/Punqee) |
| Camino | Windows (`add-windows-port`) |
| *Fork* y rama | `Punqee/dazner-challenge`, rama `challenge/punqee/add-windows-port` |
| *Pull request* | [#5](https://github.com/incognia/dazner/pull/5), abierto el 1 de noviembre de 2026 |
| *Commits* | `a252799` `feat(windows): add minimal WPF project with WebView2` y `9094e7a` `docs(windows): document playback test results` |
| Base | `1bf1f5e` (entonces el `main` vigente: la rama no estaba atrasada) |
| Herramienta declarada | Antigravity con Gemini 3.8 Flash (esfuerzo alto) |
| Alcance | Grupo 1, prueba de reproducción (tareas 1.1 y 1.2) |

## Puertas automáticas

- **`check-pr.sh`** (copia de `rules` en `%USERPROFILE%\rules`, al día con `origin/main`), corrido con `--base main` contra la rama del PR: pasan las 16 comprobaciones. *Commits* en Conventional Commits y en inglés, viñetas desde la columna 1, `Co-Authored-By` en cada *commit*, `CHANGELOG.md` solo en el último *commit* y solo con líneas añadidas, fechas en orden y sin archivos ajenos.
- **OpenSpec:** sin validar. La máquina del mantenedor no tiene Node ni npx y `check-pr.sh` lo reporta como aviso. La lista de verificación del PR marca `openspec validate --all --strict` como aprobado, pero eso no se pudo comprobar.
- **GitHub Actions:** la ejecución del flujo `PR checks` sobre el PR está retenida (`action_required`) hasta que el mantenedor la apruebe, porque es la primera contribución de esa cuenta. Es a propósito: se aprueba cuando el PR esté completo y no requiere nada de la participante.

## Compilación sin intervención

Se clonó la rama en `C:\Users\incognia\Code\GitHub\Punqee\dazner-challenge` y se siguió la tarea 1.1, porque el README no tiene instrucciones para Windows.

| Paso | Resultado |
|---|---|
| `git clone --branch challenge/punqee/add-windows-port` | Correcto |
| `dotnet build windows\windows.sln` | **Falla:** `No .NET SDKs were found`. La máquina solo tenía los *runtimes* 6, 8 y 10, sin SDK ni Visual Studio. |
| Instalar el SDK con `winget install --id Microsoft.DotNet.SDK.10 -e` | SDK 10.0.403 instalado |
| `dotnet build windows\windows.sln` | **Correcto:** 0 advertencias, 0 errores, 8 s; restaura el paquete `Microsoft.Web.WebView2` 1.0.4314.35 |
| `git status` después de compilar | Limpio: el `.gitignore` del PR cubre `bin/` y `obj/` |

**Veredicto de la primera revisión:** no pasa la regla de compilar sin intervención. El mantenedor tuvo que averiguar e instalar un requisito que no estaba documentado. El SDK de .NET 10 solo aparece en `design.md` como dato del equipo de la participante, no como requisito para quien compila. En la segunda revisión, siguiendo solo la sección «Compilar (Windows)» del README, la compilación fue correcta.

Qué faltaba en la documentación en la primera revisión (ya está en el README):

- El SDK de .NET 10 como requisito, con el comando para instalarlo.
- El *runtime* de WebView2 como requisito, y cómo comprobar si ya está instalado (Windows 11 lo trae; Windows 10 puede no tenerlo).
- El comando de compilación y la ruta del ejecutable que se genera.

## Prueba de reproducción

### En el equipo del mantenedor

El mantenedor abrió `windows.exe`, inició sesión con su cuenta y **reprodujo contenido de DAZN: DAZN funciona dentro de WebView2**. La puerta del grupo 1 queda superada en Windows.

**Evidencia:** foto tomada con un celular (según el mantenedor, un Poco X8 Pro), en horizontal, el 8 de noviembre de 2026, de la pantalla de su máquina con Windows a la que accede por RDP. Muestra la ventana «DAZNer Windows» reproduciendo un partido de la NFL (marcador 21–10, segundo cuarto, 2:00 por jugar) sobre el escritorio de Windows 11, con Visual Studio Code detrás. El mantenedor confirma que la reproducción tiene audio. Es la foto que pide la regla de evidencia, y cierra la prueba de reproducción de Windows.

Lo que se observó en la primera revisión y era de esperar, porque eran tareas pendientes:

- La ventana es la normal de Windows, con bordes y barra de título (tarea 2.1). Abre en 16:9 (`Width="1024" Height="650"` en el XAML es el tamaño exterior, con la barra de título), pero **al redimensionarla pierde la proporción** y el video queda con franjas negras (tarea 2.2, `WM_SIZING`).
- No hay atajos de teclado (tarea 5.1), paneles (grupo 3) ni pantalla completa dentro de la ventana (grupo 4).
- WebView2 crea su carpeta de datos, con la sesión de DAZN, junto al ejecutable (`windows\bin\Debug\net10.0-windows\windows.exe.WebView2`), es decir, dentro del clon. Un `dotnet clean` o un clon nuevo la borran. La tarea 4.3 pide una carpeta de datos propia de DAZNer, compartida entre paneles e instancias.

En la primera revisión, el PR no decía haber hecho nada de esto: solo marcaba las tareas 1.1 y 1.2.

**Hallazgo sobre la evidencia:** una captura de pantalla del mantenedor tomada mientras reproducía **no salió en negro**. AGENTS.md y CHALLENGE.md dan por hecho que una captura de un video con DRM siempre sale en negro, pero eso se comprobó en macOS con FairPlay. En Windows, Widevine dentro de WebView2 funciona por software y Chromium no siempre bloquea la captura. Otra posibilidad es que el contenido reproducido no tuviera DRM, por ejemplo un avance. Una captura no permite distinguir los dos casos, así que la regla de evidencia (solo fotos de celular) sigue siendo la correcta, pero la explicación de esos documentos hay que matizarla para Windows.

### Evidencia de la participante (primera foto, 31 de octubre)

El PR incrusta una imagen (1894×1067, JPEG) y dice además haber enviado una foto por un canal privado.

- **A favor:** es una foto real de celular (Samsung Galaxy S24 según sus metadatos; tiene muaré), en horizontal, tomada el 31 de octubre de 2026. Muestra la ventana «DAZNer Windows» corriendo desde Visual Studio, con su barra de depuración de XAML, lo que respalda la compilación en Visual Studio que pide la tarea 1.1.
- **En contra:** **no muestra ninguna reproducción.** Se ve la portada de DAZN con un cartel promocional estático de un evento de MMA, no un partido de la NFL ni un video con sus controles. En la barra superior aparece el botón «Suscribir», que DAZN suele mostrar cuando la cuenta no tiene una suscripción activa.
- **Conclusión:** la foto prueba que la aplicación abre DAZN, no que el DRM reproduzca. El reto permite no tener suscripción, siempre que se declare y la reproducción se valide con una página de demostración de DRM. El PR, en cambio, afirma una «reproducción continua con audio y video en alta definición» de un partido de la NFL. La participante ya aclaró al mantenedor que probó con su propia cuenta de DAZN, de acceso limitado, con partidos gratuitos. Eso es válido. La segunda foto, que agregó después al PR, sí prueba la reproducción (ver «Segunda revisión»), y su cuenta ya está declarada en el PR y en `design.md`.

## Observaciones

Estas observaciones son de la primera revisión y ya están atendidas, salvo lo que repite «Qué se te pide ahora».

### Código

- `MainWindow.xaml` declara `xmlns:d="http://schemas.microsoft.com/winfx/2006/xaml/desarrollo"`, un espacio de nombres que no existe. El correcto es `http://schemas.microsoft.com/expression/blend/2008`. Compila porque `mc:Ignorable="d"` lo ignora, pero parece una traducción automática que nadie revisó.
- El proyecto y el espacio de nombres se llaman `windows`, que es genérico. Conviene que se llamen `DAZNer`.
- Quedan `using` de la plantilla sin usar (`System.Configuration`, `System.Data` y casi todos los de `MainWindow.xaml.cs`), aunque `ImplicitUsings` ya está activo.
- Los `.cs` y `.xaml` traen BOM, como pide `.editorconfig`, y todos los archivos tienen finales de línea LF.

### `design.md`

- La sección «Resultados de la prueba de reproducción» es una buena idea, pero:
  - La calidad máxima («1080p / 720p según el flujo adaptativo») se afirma «por inspección visual». No se midió; por ejemplo, con las estadísticas del reproductor o con `chrome://media-internals` en el inspector de WebView2.
  - El problema con Visual C++ se resuelve en una línea suelta. No usa la tabla de la bitácora del reto (síntoma con el mensaje exacto, causa, solución y si exige una acción previa de quien compila).
- No responde las preguntas que CHALLENGE.md exige para Windows: dónde vive el clon que se compila, cómo se evita el ruido de codificación y cómo se corren `/commit` y `/changelogger` si el control de versiones se hace desde WSL. Las «Open Questions» sobre el clon y sobre `.gitattributes` quedan igual que antes.

### Descripción del PR

- Usa la plantilla completa, con sus ocho secciones y la declaración de una cuenta por persona en «Decisiones».
- Casi todo el cuerpo está sangrado con cuatro espacios, así que GitHub lo muestra como un bloque de código: los títulos y la tabla de «Cómo se verificó» no se ven con formato.
- «Riesgos: ninguno identificado» contradice el problema con Visual C++ que se documenta en `design.md` y el requisito del SDK.
- En «Cómo se verificó», la tarea 1.1 dice «apertura en Visual Studio sin errores», pero la tarea pide compilar la solución en Visual Studio. La foto sugiere que sí se ejecutó desde ahí.
- Deja sin marcar `TODO.md` hasta que el mantenedor lo apruebe. Es lo correcto.

### CHANGELOG

- El bloque `## [2026-11-01] - Prueba de reproducción en Windows` tiene la fecha correcta en CST y la posición correcta.
- Las viñetas son escuetas y no ponen las rutas entre comillas invertidas (`windows/`, `design.md`), como hace el resto del archivo.

## Calificación de la primera revisión

Califica la entrega tal como se presentó en el PR, con la rama en el estado de la primera revisión. Escala de 0 a 10 por criterio, ponderada con los pesos de CHALLENGE.md.

| Criterio | Peso | Calificación | Motivo |
|---|---|---|---|
| Disciplina SDD | 30 % | 7 | Sigue el cambio ya propuesto y marca solo las tareas del grupo 1. Faltan las respuestas que el reto exige para Windows y la bitácora con su formato. |
| Compila en otra máquina sin intervención | 25 % | 4 | Compila sin advertencias, pero requiere el SDK de .NET 10, que no está documentado, y no hay instrucciones de compilación. |
| Calidad de los PR y de los *commits* | 20 % | 7 | Pasa todas las puertas de `check-pr.sh` y tiene *commits* atómicos y bien descritos. El cuerpo del PR no se ve con formato y el CHANGELOG es escueto. |
| Honestidad | 15 % | 10 | No afirma nada fuera del grupo 1 e incluye la declaración de cuenta. Lo que se dijo de más (la reproducción que la foto no muestra, la calidad sin medir, OpenSpec como validado y «Riesgos: ninguno») son descuidos de quien empieza, no mentiras, y la participante aclaró su cuenta de acceso limitado en cuanto se le preguntó. |
| Calidad técnica | 10 % | 6 | Proyecto mínimo y correcto para una prueba, pero con un espacio de nombres inventado, nombres genéricos y restos de la plantilla. |
| **Total ponderado** | | **6.6 / 10** | |

## Calificación actual

Es la calificación vigente. Se actualiza cada vez que la participante entrega y el mantenedor verifica un grupo de tareas. Escala de 0 a 10 por criterio, ponderada con los pesos de CHALLENGE.md.

| Criterio | Peso | Calificación | Motivo |
|---|---|---|---|
| Disciplina SDD | 30 % | 9 | Atendió cada revisión por la especificación antes que el código y dejó sin marcar lo que no pudo probar (la 2.2 en dos monitores y la 4.2 con *multiview*). Agregó `Ctrl+R` y `Ctrl+H`, de la tarea 5.1, sin registrarlos en `tasks.md` ni en el diseño. |
| Compila en otra máquina sin intervención | 25 % | 8 | Compiló en un clon nuevo siguiendo solo el README, con 0 advertencias y 0 errores, y volvió a compilar igual en la quinta entrega, con el script compartido copiado junto al ejecutable. No se volvió a probar la instalación del SDK ni la compilación en Visual Studio. |
| Calidad de los PR y de los *commits* | 20 % | 7 | `check-pr.sh` falla: `CHANGELOG.md` cambia en dos *commits*, y un PR así se rechaza sin revisar el código. Los *commits* siguen siendo atómicos y el PR detalla qué probó y qué no. Sube en cuanto pase la comprobación. |
| Honestidad | 15 % | 10 | Declara su cuenta de acceso limitado, su único monitor y lo que no probó, incluido el *multiview* de la tarea 4.2. |
| Calidad técnica | 10 % | 7 | La ventana, los paneles, la sesión compartida entre instancias y la pantalla completa del video principal funcionan, y el diseño deja apagados el guardado de contraseñas y el inspector fuera de `--debug`. La pantalla completa del *multiview*, el caso por el que existe el script compartido, no funciona porque su hoja de estilo no se aplica. |
| **Total ponderado** | | **8.3 / 10** | |

Avance verificado: 13 de las 18 tareas del cambio `add-windows-port` (1.1 a 1.4, 2.1 a 2.3, 3.1 a 3.4, 4.1 y 4.3). La 4.2 no pasa con *multiview*. Quedan la 4.2 y de la 5.1 a la 5.4.

## Veredicto y siguientes pasos

- **Prueba de reproducción de Windows: superada**, verificada en el equipo del mantenedor. El camino de Windows puede seguir con los grupos 2 a 5.
- **El PR es una buena base** para el camino de Windows: no rompe nada y sus archivos respetan la codificación y los finales de línea del repositorio. Se integra cuando atienda la revisión, esté al día con `main` y el PR esté completo (ver «Cómo atender esta revisión» y «Cómo traer los cambios de `main` a tu rama»).
- **Todavía no hay *referral*:** se decide cuando el PR esté completo, y el reto lo condiciona a compilar y arrancar sin intervención.
- **Qué se le pidió a la participante** (ya atendido, ver la segunda revisión):
  1. Documentar el SDK de .NET 10 y el *runtime* de WebView2 en una sección de requisitos del README y en la bitácora de `design.md`, con su formato.
  2. Responder en `design.md` las preguntas de Windows que exige el reto. Incluye actualizar la pregunta abierta sobre `.gitattributes`, que ya existe en `main`.
  3. Corregir `xmlns:d`, quitar los `using` sobrantes y renombrar el proyecto a `DAZNer`.
  4. Quitar la sangría del cuerpo del PR y corregir «Riesgos».
  5. Declarar en el PR (sección «Decisiones») y en `design.md` que probó con su propia cuenta de DAZN, de acceso limitado y con partidos gratuitos, y qué contenido reprodujo. Si tiene la foto del canal privado, la adjunta al PR.

## Cómo atender esta revisión

Las correcciones **no se hacen a mano en el código**. La regla 1 de [CHALLENGE.md](../CHALLENGE.md) pide el orden de siempre: primero la especificación, después el código. Haz esto una vez que hayas traído los cambios de `main` (ver «Cómo traer los cambios de `main` a tu rama»):

1. **Integra la revisión en tu cambio de OpenSpec.** Corre `/opsx:update add-windows-port` y usa este archivo y la [segunda revisión](../review/2026-11-09_punqee_review.md) como entrada; si algo se repite, manda la segunda. Ese comando solo revisa los artefactos del cambio (`proposal.md`, `design.md`, `tasks.md` y la especificación de `platform-windows`) y no toca código. Si tu herramienta no tiene el comando, pídele a tu IA que siga el procedimiento de `.claude/skills/openspec-update-change/SKILL.md`.
2. **Qué va en cada artefacto:**
   - **`design.md`:** las respuestas a las preguntas de Windows del reto, la bitácora de problemas con su formato (incluido el SDK de .NET 10 y el *runtime* de WebView2) y los riesgos que pida la revisión.
   - **`tasks.md`:** una tarea, con su verificación, por cada corrección de código y por la sección de requisitos del README.
   - **`proposal.md` y la especificación:** solo si una corrección cambia lo que el cambio promete.
3. **Valida** con `openspec validate --all --strict` antes de tocar el código.
4. **Aplica el código** con `/opsx:apply add-windows-port`, o siguiendo las tareas en orden si tu herramienta no lo tiene, y marca cada tarea solo cuando ejecutes su verificación.
5. **Todo se sube a tu misma rama**, en el mismo PR, con *commits* atómicos. Este archivo no se modifica.

**Lo que no puedas probar.** Hay tareas que dependen de contenido de NFL Game Pass, como la pantalla completa con cuatro videos (tarea 4.2), o de algo que tu cuenta no permite. Esas tareas se quedan **sin marcar** (`- [ ]`), se anotan en la sección «Qué no se probó» del PR y las verifica el mantenedor con su cuenta, en su equipo. Una tarea que no ejecutaste no se marca como hecha, porque así lo piden las reglas del reto; si no pudiste probar algo, basta con declararlo y no pasa nada.

Si algo de `design.md` contradice a `AGENTS.md` (por ejemplo, el uso de Git desde WSL), manda `AGENTS.md`: el autor trabaja ahora con Git desde Windows nativo y `.gitattributes` ya existe.

## Segunda revisión (9 de noviembre de 2026)

El mantenedor revisó la rama que reescribiste, con su último *commit* `feat(windows): add borderless window with drag strip and Alt drag`. El detalle completo está en [review/2026-11-09_punqee_review.md](../review/2026-11-09_punqee_review.md); léelo antes de seguir. Resumen:

- **Atendido de la primera revisión:** el *rebase* sobre `main`, la especificación antes que el código, los requisitos en el README, las respuestas y la bitácora en `design.md`, la limpieza del proyecto (`DAZNer`, `xmlns:d` y los `using`), el cuerpo del PR y la declaración de la cuenta de acceso limitado. La lista de «Veredicto y siguientes pasos» ya está atendida.
- **Prueba de reproducción (grupo 1): superada,** ahora también con la foto de la participante como evidencia.
- **Tarea 2.1: funciona** en el equipo del mantenedor, probada por RDP. La ventana responde al clic, se arrastra desde la franja, se mueve con `Alt` desde cualquier punto, y el doble clic la maximiza y la restaura sin tapar la barra de tareas. El mantenedor no puede probar el efecto del *hook* de ratón en un equipo con monitor y asume ese riesgo.
- **Calificación:** la primera revisión califica la entrega tal como se presentó entonces; su honestidad se ajustó a 10 y el total quedó en 6.6, porque lo que se afirmó de más fueron descuidos y no mentiras. La calificación vigente está en «Calificación actual» y se mueve conforme la participante entregue y el mantenedor verifique cada grupo de tareas.

**Qué se pidió en la segunda revisión** (ya atendido en la tercera entrega):

1. Corregir en `design.md` la afirmación de que la reproducción «en alta definición» fue verificada por el mantenedor: nadie midió la calidad. Quita «en alta definición» o di que no se midió.
2. Agregar a «Risks / Trade-offs» de `design.md` el riesgo del *hook* `WH_MOUSE_LL`: recibe todos los movimientos del ratón del sistema, corre en el hilo de la interfaz, y si la aplicación se traba el ratón de todo Windows se pone lento o Windows quita el *hook* sin avisar y el arrastre deja de funcionar. Quitar o justificar el código que nunca se ejecuta (`DragStrip_MouseLeftButtonDown`, `Window_PreviewMouseLeftButtonDown` y el `Border` de `MainWindow.xaml`). Una alternativa a evaluar es la región de arrastre de WebView2 con `IsNonClientRegionSupportEnabled` y la propiedad CSS `app-region: drag`.
3. Mover el renombre de `windows.csproj` y `windows.sln` a `DAZNer`, que hoy está en el *commit* `docs(windows): document build requirements and steps in README`, al *commit* `refactor(windows): rename project to DAZNer and clean up template code`. Completar el CHANGELOG con la viñeta que falta (la del *commit* `docs(windows): update change specification and design following review`) y dejar las viñetas en el orden de los *commits*.
4. Confirmar en el PR qué probaste de la tarea 2.1: el arrastre desde la franja, `Alt` + arrastrar y el doble clic para maximizar y restaurar.
5. Menores: quitar la ruta personal (`C:\Users\punqe\...`) de `design.md` y dejar una línea en blanco antes de los bloques de código dentro de las listas del README.

Todo se atiende como indica «Cómo atender esta revisión»: primero `design.md` y `tasks.md`, después el código, con *commits* atómicos reescribiendo tu propia rama y `git push --force-with-lease`.

## Tercera revisión (9 de noviembre de 2026, noche)

El mantenedor revisó la rama en su último *commit*, `feat(windows): add fill and center and always-on-top window actions`, ya sobre el `main` actual. El detalle completo está en [review/2026-11-09_punqee_review_2.md](../review/2026-11-09_punqee_review_2.md). Resumen:

- **Puertas:** `check-pr.sh` pasa las 16 comprobaciones (OpenSpec sigue sin validarse por falta de Node en la máquina del mantenedor) y ningún archivo tiene CRLF en el repositorio.
- **Compilación:** `dotnet build windows\DAZNer.sln`, con 0 advertencias y 0 errores.
- **Atendido de la segunda revisión:** todo. `design.md` ya no dice que se verificó la alta definición y documenta el riesgo del *hook* con su alternativa; el renombre del proyecto está en su *commit* `refactor`; el CHANGELOG tiene todas las viñetas en el orden de los *commits*; se retiró el código que no se ejecutaba y la ruta personal; y el PR detalla qué probaste de la tarea 2.1.
- **Prueba del mantenedor, por RDP: todo funciona.** La ventana conserva la proporción 16:9 al arrastrar los bordes y las esquinas; `Ctrl+F` la llena y la centra en el área útil aun con el foco en DAZN; `Ctrl+T` la deja siempre encima y la regresa a su nivel normal; los controles de DAZN junto al borde responden; y el arrastre desde la franja, `Alt` + arrastrar y el doble clic siguen funcionando.
- **Tareas:** la 2.3 queda verificada. La 2.2 se deja abierta, como la dejaste: falta probarla con dos paneles (grupo 3) y en dos monitores.

**Qué se pidió en la tercera revisión** (ya atendido en la cuarta entrega):

1. En la tarea 2.3 falta el escenario «Pantalla más alta que la proporción» de `window-chrome`, que en tu monitor solo se da con dos paneles (32:9). Anótalo en «Qué no se probó» del PR; el mantenedor lo verifica cuando exista el segundo panel. No hace falta desmarcar la tarea.
2. Sigue con el grupo 3 (paneles), igual que hasta ahora: primero `design.md` y `tasks.md`, después el código.
3. Opcional: el *hook* procesa ahora cada movimiento del ratón del sistema y en cada uno consulta el rectángulo y los DPI de la ventana. Si puedes, haz esa consulta solo cuando el puntero esté sobre la ventana o guarda el rectángulo al moverla o redimensionarla.

## Cuarta revisión (9 de noviembre de 2026, noche)

El mantenedor revisó la rama en su último *commit*, `feat(windows): add focus audio muting, pane switching, URL dialog and same-pane links`, ya sobre el `main` actual. El detalle completo está en [review/2026-11-09_punqee_review_3.md](../review/2026-11-09_punqee_review_3.md). Resumen:

- **Puertas:** `check-pr.sh` pasa las 16 comprobaciones (OpenSpec sigue sin validarse por falta de Node en la máquina del mantenedor) y ningún archivo tiene CRLF en el repositorio.
- **Compilación:** `dotnet build windows\DAZNer.sln`, con 0 advertencias y 0 errores.
- **Atendido de la tercera revisión:** todo, incluida la sugerencia opcional. Probaste «Pantalla más alta que la proporción» con dos paneles y anotaste en el PR lo que falta con uno, y el *hook* usa el rectángulo y los DPI guardados en lugar de consultarlos en cada movimiento.
- **Prueba del mantenedor, por RDP: todo funciona.** `Ctrl+2` abre el segundo panel sin recargar el primero; el audio sigue al panel con foco, por clic y con `Ctrl+Tab`; el silencio puesto desde el reproductor se respeta; `Ctrl+L` carga la dirección solo en el panel con foco; los enlaces de ventana nueva se abren en el mismo panel; con dos paneles la ventana conserva 32:9 y `Ctrl+F` la centra; y `Ctrl+1` quita el segundo panel sin interrumpir el primero.
- **Tareas:** quedan verificadas la 2.3 por completo y de la 3.1 a la 3.4. La 2.2 sigue abierta solo por el escenario de dos monitores.

**Qué se pidió en la cuarta revisión** (ya atendido en la quinta entrega):

1. Al abrir el segundo panel, macOS cambia primero el tamaño de la ventana y después el *layout*, para que DAZN no rehaga el reproductor del primer panel. Tu versión ajusta primero las columnas del `Grid` y después la ventana. En la prueba funcionó, pero invierte el orden como en macOS o explica en `design.md` por qué no afecta.
2. Para cambiar de panel, la especificación pide `` Ctrl+` `` y el código acepta seis teclas (`Oem3`, `Oem5`, `Oem1`, `OemOpenBrackets`, `OemBackslash` y `Tab`), aunque `design.md` menciona solo tres. Documenta cada tecla con la distribución de teclado que la motiva, o quita las que sobran.
3. Sigue con el grupo 4 (pantalla completa y sesión), igual que hasta ahora: primero `design.md` y `tasks.md`, después el código.

## Quinta revisión (12 de noviembre de 2026)

El mantenedor revisó la rama en su último *commit*, `feat(windows): add shared fullscreen script, debug logging and persistent session`, ya sobre el `main` actual, y la probó por RDP con dos monitores y con su cuenta. El detalle completo, con el diagnóstico de la falla, está en [review/2026-11-12_punqee_review.md](../review/2026-11-12_punqee_review.md). Resumen:

- **Puertas: `check-pr.sh` falla.** `CHANGELOG.md` cambia en `88a9129` y en `255c5d5`, y solo puede cambiar en el último *commit*.
- **Compilación:** `dotnet build windows\DAZNer.sln`, con 0 advertencias y 0 errores.
- **Atendido de la cuarta revisión:** todo. La ventana cambia de tamaño antes que el `Grid`, como en macOS, y las cuatro teclas de «siguiente panel» están documentadas.
- **Prueba del mantenedor:**
  - **Funciona:**
    - La pantalla completa del video principal con uno y dos paneles, y `Esc`.
    - `--debug`, con `debug.log` y el inspector, que no abre sin `--debug`.
    - La sesión al reabrir y en tres instancias que reproducen a la vez.
    - La proporción fija y `Ctrl+F` en un segundo monitor.
  - **Falla:** la pantalla completa del *multiview*. Con un video, ninguno se ve. Con la general, los videos llenan la ventana, pero se ve el resto de la página. `debug.log` indica que la hoja de estilo de `fill-fullscreen.js` no se aplica: el elemento con `__fs` conserva `position: relative`.
- **Tareas:** quedan verificadas la 2.2 (ya puedes marcarla), la 4.1 y la 4.3. La 4.2 no pasa.

**Qué se te pide ahora:**

1. Corrige el CHANGELOG para que cambie solo en el último *commit*, reescribiendo tu rama, y comprueba que `check-pr.sh` pase sin fallos antes de subir.
2. Haz que la hoja de estilo de `fill-fullscreen.js` se aplique, sin modificar el script compartido; el sospechoso es el puente de `document.head`. Primero en `design.md` y `tasks.md`, después en el código. No necesitas *multiview* para comprobarlo: con `--debug`, evalúa en la consola del inspector `[...document.querySelectorAll('style')].some(s => s.textContent.includes('.__fs'))` (debe dar `true`) y comprueba que un elemento con la clase `__fs` tenga `position: fixed` en su estilo calculado. Agrega esa comprobación a la verificación de la tarea 4.2; el mantenedor vuelve a probar el *multiview* con su cuenta.
3. Marca la tarea 2.2 y anota en el PR que la verificó el mantenedor en dos monitores.
4. Pasa `Ctrl+R` y `Ctrl+H` a la tarea 5.1, o documéntalos en `design.md` y en el *commit* en el que entran.

## Cómo traer los cambios de `main` a tu rama

Si ya hiciste este *rebase* y `git fetch upstream` no trae *commits* nuevos de `main`, salta a «Finales de línea y codificación». Repítelo solo si `main` avanza otra vez.

Después de la primera revisión, `main` avanzó: trae un `.gitattributes` que fija los finales de línea y correcciones a la documentación. Tu rama se creó sobre `1bf1f5e`, así que hay que reaplicarla sobre el `main` actual con `rebase`, como indica CHALLENGE.md en «Responder a una revisión». No uses `merge`: el PR debe quedar como una serie lineal de *commits* sobre `main`.

1. **Antes de empezar,** haz un *commit* o un `git stash` de lo que tengas a medias: el paso 4 descarta los cambios sin guardar.
2. **Trae `main` y reaplica tu rama encima:**

   ```bash
   git remote add upstream https://github.com/incognia/dazner.git   # solo la primera vez
   git fetch upstream
   git switch challenge/punqee/add-windows-port
   git rebase upstream/main
   ```

3. **Resuelve el conflicto de `CHANGELOG.md`.** Tu bloque y el de `main` se agregan en el mismo lugar. Conserva los dos, con el tuyo arriba, y **cambia la fecha de tu bloque** por la de tu último *commit* (hora del centro de México): `check-pr.sh` exige las fechas en orden cronológico inverso, y un `2026-11-01` encima de una fecha posterior no pasa. Si la fecha de tu último *commit* coincide con la de un bloque que ya existe en `main`, pon tus viñetas dentro de ese bloque, arriba de las que tiene: solo puede haber un bloque por fecha. Después, `git add CHANGELOG.md` y `git rebase --continue`.
4. **Refresca los finales de línea de tu copia.** Git de Windows trae `core.autocrlf=true`, así que tus archivos pueden estar en CRLF en disco. Con el `.gitattributes` nuevo, git los vería como modificados. Para volver a escribirlos con las reglas nuevas:

   ```bash
   git rm -r --cached -q .
   git reset --hard
   git status   # debe quedar limpio
   ```

5. **Atiende la revisión** como indica «Cómo atender esta revisión»: primero la especificación y después el código. Al terminar, deja tus *commits* atómicos reescribiendo tu propia rama (`git commit --amend` o `git rebase -i upstream/main`), sin `fixup!`, y `CHANGELOG.md` solo en el último *commit*.
6. **Comprueba y sube:**

   ```bash
   bash ~/rules/scripts/check-pr.sh --base upstream/main
   git push --force-with-lease origin challenge/punqee/add-windows-port
   ```

   El PR #5 se actualiza solo. `--force-with-lease` es válido únicamente en tu rama de PR, nunca en `main`.

## Finales de línea y codificación: lo que debes cuidar siempre

Tu Git de Windows suele convertir los archivos a CRLF, y eso ensucia el repositorio con cambios que no son tuyos. El repositorio ya trae `.gitattributes` y `.editorconfig` para evitarlo; solo tienes que respetarlos:

- **No definas `core.autocrlf` ni borres `.gitattributes`.** El repositorio guarda todo en LF, sin importar esa configuración.
- **El `.sln` es la única excepción visible:** en tu disco queda en CRLF y en el repositorio en LF. Es lo esperado y no es un error.
- **Los `.cs`, `.xaml` y `.ps1` van en UTF-8 con BOM** y el resto en UTF-8 sin BOM, como fija `.editorconfig`. No uses «Guardar como» con otra codificación ni reescribas archivos completos desde el editor.
- **Antes de cada *commit*,** corre `git diff --check` y `git status`: solo deben aparecer los cambios que hiciste.
- **Antes de subir,** comprueba estas dos cosas:
  - `git ls-files --eol | findstr "i/crlf"` no debe mostrar ningún archivo.
  - `git diff upstream/main --stat` debe mostrar solo tus archivos de `windows/`, los de tu cambio en `openspec/changes/add-windows-port/`, `README.md`, `.gitignore` y `CHANGELOG.md`.
- **Si un archivo que no tocaste aparece modificado,** no lo subas: normalmente son finales de línea. Para ese archivo basta con `git restore -- <archivo>`, que lo vuelve a escribir con las reglas de `.gitattributes` y descarta lo que tenga sin guardar. Si son muchos, repite el paso 4 de «Cómo traer los cambios de `main` a tu rama», pero **antes haz un *commit* o un `git stash` de tu trabajo**: `git reset --hard` borra todo lo que no esté guardado.

## Entorno del mantenedor

| Componente | Versión |
|---|---|
| Sistema | Windows 11 Pro 10.0.26300 |
| SDK de .NET | 10.0.403 (instalado durante la revisión) |
| *Runtime* de WebView2 | 157.0.4341.42 |
| Visual Studio | No instalado |
| Git | 2.55.0 para Windows, con `.gitattributes` y la configuración de `/git-init` |
