# Evaluación del reto: Punqee

*Última modificación: 7 de noviembre de 2026, 17:12 (CST)*

Revisión de la entrega de Míriam ([@Punqee](https://github.com/Punqee)) al [reto de contribución](../CHALLENGE.md), hecha por el mantenedor con Claude Code (Opus 5.5) en una máquina con Windows. Registra lo que se ejecutó, lo que se observó y una propuesta de calificación con los criterios del reto.

## Datos de la entrega

| Dato | Valor |
|---|---|
| Cuenta | [@Punqee](https://github.com/Punqee) |
| Camino | Windows (`add-windows-port`) |
| *Fork* y rama | `Punqee/dazner-challenge`, rama `challenge/punqee/add-windows-port` |
| *Pull request* | [#5](https://github.com/incognia/dazner/pull/5), abierto el 1 de noviembre de 2026 |
| *Commits* | `a252799` `feat(windows): add minimal WPF project with WebView2` y `9094e7a` `docs(windows): document playback test results` |
| Base | `1bf1f5e` (el `main` vigente: la rama no está atrasada) |
| Herramienta declarada | Antigravity con Gemini 3.8 Flash (esfuerzo alto) |
| Alcance | Grupo 1, prueba de reproducción (tareas 1.1 y 1.2) |

## Puertas automáticas

- **`check-pr.sh`** (copia de `rules` en `%USERPROFILE%\rules`, al día con `origin/main`), corrido con `--base main` contra la rama del PR: pasan las 16 comprobaciones. *Commits* en Conventional Commits y en inglés, viñetas desde la columna 1, `Co-Authored-By` en cada *commit*, `CHANGELOG.md` solo en el último *commit* y solo con líneas añadidas, fechas en orden y sin archivos ajenos.
- **OpenSpec:** sin validar. La máquina del mantenedor no tiene Node ni npx y `check-pr.sh` lo reporta como aviso. La lista de verificación del PR marca `openspec validate --all --strict` como aprobado, pero eso no se pudo comprobar.
- **GitHub Actions:** el flujo `PR checks` no corrió sobre `9094e7a`; el *commit* no tiene ninguna ejecución registrada. Lo más probable es que GitHub esté esperando que el mantenedor apruebe la ejecución, porque es la primera contribución de esa cuenta. Queda pendiente.

## Compilación sin intervención

Se clonó la rama en `C:\Users\incognia\Code\GitHub\Punqee\dazner-challenge` y se siguió la tarea 1.1, porque el README no tiene instrucciones para Windows.

| Paso | Resultado |
|---|---|
| `git clone --branch challenge/punqee/add-windows-port` | Correcto |
| `dotnet build windows\windows.sln` | **Falla:** `No .NET SDKs were found`. La máquina solo tenía los *runtimes* 6, 8 y 10, sin SDK ni Visual Studio. |
| Instalar el SDK con `winget install --id Microsoft.DotNet.SDK.10 -e` | SDK 10.0.403 instalado |
| `dotnet build windows\windows.sln` | **Correcto:** 0 advertencias, 0 errores, 8 s; restaura el paquete `Microsoft.Web.WebView2` 1.0.4314.35 |
| `git status` después de compilar | Limpio: el `.gitignore` del PR cubre `bin/` y `obj/` |

**Veredicto:** no pasa la regla de compilar sin intervención. El mantenedor tuvo que averiguar e instalar un requisito que no estaba documentado. El SDK de .NET 10 solo aparece en `design.md` como dato del equipo de la participante, no como requisito para quien compila.

Qué faltaba en la documentación:

- El SDK de .NET 10 como requisito, con el comando para instalarlo.
- El *runtime* de WebView2 como requisito, y cómo comprobar si ya está instalado (Windows 11 lo trae; Windows 10 puede no tenerlo).
- El comando de compilación y la ruta del ejecutable que se genera.

## Prueba de reproducción

### En el equipo del mantenedor

El mantenedor abrió `windows.exe`, inició sesión con su cuenta y **reprodujo contenido de DAZN: DAZN funciona dentro de WebView2**. La puerta del grupo 1 queda superada en Windows.

Lo que se observó y era de esperar, porque son tareas pendientes:

- La ventana es la normal de Windows, con bordes y barra de título (tarea 2.1). Abre en 16:9 (`Width="1024" Height="650"` en el XAML es el tamaño exterior, con la barra de título), pero **al redimensionarla pierde la proporción** y el video queda con franjas negras (tarea 2.2, `WM_SIZING`).
- No hay atajos de teclado (tarea 5.1), paneles (grupo 3) ni pantalla completa dentro de la ventana (grupo 4).
- WebView2 crea su carpeta de datos, con la sesión de DAZN, junto al ejecutable (`windows\bin\Debug\net10.0-windows\windows.exe.WebView2`), es decir, dentro del clon. Un `dotnet clean` o un clon nuevo la borran. La tarea 4.3 pide una carpeta de datos propia de DAZNer, compartida entre paneles e instancias.

El PR no dice haber hecho nada de esto: solo marca las tareas 1.1 y 1.2.

**Hallazgo sobre la evidencia:** una captura de pantalla del mantenedor tomada mientras reproducía **no salió en negro**. AGENTS.md y CHALLENGE.md dan por hecho que una captura de un video con DRM siempre sale en negro, pero eso se comprobó en macOS con FairPlay. En Windows, Widevine dentro de WebView2 funciona por software y Chromium no siempre bloquea la captura. Otra posibilidad es que el contenido reproducido no tuviera DRM, por ejemplo un avance. Una captura no permite distinguir los dos casos, así que la regla de evidencia (solo fotos de celular) sigue siendo la correcta, pero la explicación de esos documentos hay que matizarla para Windows.

### Evidencia de la participante

El PR incrusta una imagen (1894×1067, JPEG) y dice además haber enviado una foto por un canal privado.

- **A favor:** es una foto real de celular (Samsung Galaxy S24 según sus metadatos; tiene muaré), en horizontal, tomada el 31 de octubre de 2026. Muestra la ventana «DAZNer Windows» corriendo desde Visual Studio, con su barra de depuración de XAML, lo que respalda la compilación en Visual Studio que pide la tarea 1.1.
- **En contra:** **no muestra ninguna reproducción.** Se ve la portada de DAZN con un cartel promocional estático de un evento de MMA, no un partido de la NFL ni un video con sus controles. En la barra superior aparece el botón «Suscribir», que DAZN suele mostrar cuando la cuenta no tiene una suscripción activa.
- **Conclusión:** la foto prueba que la aplicación abre DAZN, no que el DRM reproduzca. El reto permite no tener suscripción, siempre que se declare y la reproducción se valide con una página de demostración de DRM. El PR, en cambio, afirma una «reproducción continua con audio y video en alta definición» de un partido de la NFL. Hay que pedir a la participante la foto que envió por el canal privado, o que aclare si tiene una suscripción con NFL Game Pass.

## Observaciones

### Código

- `MainWindow.xaml` declara `xmlns:d="http://schemas.microsoft.com/winfx/2006/xaml/desarrollo"`, un espacio de nombres que no existe. El correcto es `http://schemas.microsoft.com/expression/blend/2008`. Compila porque `mc:Ignorable="d"` lo ignora, pero parece una traducción automática que nadie revisó.
- El proyecto y el espacio de nombres se llaman `windows`, que es genérico. Conviene que se llamen `DAZNer`.
- Quedan `using` de la plantilla sin usar (`System.Configuration`, `System.Data` y casi todos los de `MainWindow.xaml.cs`), aunque `ImplicitUsings` ya está activo.
- Los `.cs`, `.xaml` y `.sln` traen BOM y finales de línea LF, como pide `.editorconfig`.

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

## Calificación propuesta

Propuesta del mantenedor, sujeta a la respuesta de la participante sobre la evidencia. Escala de 0 a 10 por criterio, ponderada con los pesos de CHALLENGE.md.

| Criterio | Peso | Calificación | Motivo |
|---|---|---|---|
| Disciplina SDD | 30 % | 7 | Sigue el cambio ya propuesto y marca solo las tareas del grupo 1. Faltan las respuestas que el reto exige para Windows y la bitácora con su formato. |
| Compila en otra máquina sin intervención | 25 % | 4 | Compila sin advertencias, pero requiere el SDK de .NET 10, que no está documentado, y no hay instrucciones de compilación. |
| Calidad de los PR y de los *commits* | 20 % | 7 | Pasa todas las puertas de `check-pr.sh` y tiene *commits* atómicos y bien descritos. El cuerpo del PR no se ve con formato y el CHANGELOG es escueto. |
| Honestidad | 15 % | 5 | No afirma nada fuera del grupo 1 e incluye la declaración de cuenta. La evidencia no muestra la reproducción que se afirma, la calidad no se midió, OpenSpec se marca como validado sin que se pueda comprobar y «Riesgos: ninguno» no cuadra. |
| Calidad técnica | 10 % | 6 | Proyecto mínimo y correcto para una prueba, pero con un espacio de nombres inventado, nombres genéricos y restos de la plantilla. |
| **Total ponderado** | | **5.9 / 10** | |

## Veredicto y siguientes pasos

- **Prueba de reproducción de Windows: superada**, verificada en el equipo del mantenedor. El camino de Windows puede seguir con los grupos 2 a 5.
- **El PR es una buena base** para el camino de Windows: no rompe nada y sus archivos respetan la codificación y los finales de línea del repositorio. Se integra cuando atienda la revisión y esté al día con `main` (ver la sección siguiente).
- **Todavía no hay *referral*:** el reto lo condiciona a compilar y arrancar sin intervención.
- **Qué se le pide a la participante** si continúa:
  1. La foto que envió por el canal privado, o una declaración de si tiene suscripción con NFL Game Pass.
  2. Documentar el SDK de .NET 10 y el *runtime* de WebView2 en una sección de requisitos del README y en la bitácora de `design.md`, con su formato.
  3. Responder en `design.md` las preguntas de Windows que exige el reto. Incluye actualizar la pregunta abierta sobre `.gitattributes`, que ya existe en `main`.
  4. Corregir `xmlns:d`, quitar los `using` sobrantes y renombrar el proyecto a `DAZNer`.
  5. Quitar la sangría del cuerpo del PR y corregir «Riesgos».

## Cómo atender esta revisión

Las correcciones **no se hacen a mano en el código**. La regla 1 de [CHALLENGE.md](../CHALLENGE.md) pide el orden de siempre: primero la especificación, después el código. Haz esto una vez que hayas traído los cambios de `main` (ver la sección siguiente):

1. **Integra la revisión en tu cambio de OpenSpec.** Corre `/opsx:update add-windows-port` y usa este archivo como entrada. Ese comando solo revisa los artefactos del cambio (`proposal.md`, `design.md`, `tasks.md` y la especificación de `platform-windows`) y no toca código. Si tu herramienta no tiene el comando, pídele a tu IA que siga el procedimiento de `.claude/skills/openspec-update-change/SKILL.md`.
2. **Qué va en cada artefacto:**
   - **`design.md`:** las respuestas a las preguntas de Windows del reto (punto 3 de la lista anterior) y la bitácora de problemas con su formato, incluido el SDK de .NET 10 y el *runtime* de WebView2.
   - **`tasks.md`:** una tarea, con su verificación, por cada corrección de código (punto 4) y por la sección de requisitos del README.
   - **`proposal.md` y la especificación:** solo si una corrección cambia lo que el cambio promete.
3. **Valida** con `openspec validate --all --strict` antes de tocar el código.
4. **Aplica el código** con `/opsx:apply add-windows-port`, o siguiendo las tareas en orden si tu herramienta no lo tiene, y marca cada tarea solo cuando ejecutes su verificación.
5. **Todo se sube a tu misma rama**, en el mismo PR, con *commits* atómicos. Este archivo no se modifica.

Si algo de `design.md` contradice a `AGENTS.md` (por ejemplo, el uso de Git desde WSL), manda `AGENTS.md`: el autor trabaja ahora con Git desde Windows nativo y `.gitattributes` ya existe.

## Cómo traer los cambios de `main` a tu rama

Después de la revisión, `main` avanzó: trae un `.gitattributes` que fija los finales de línea y correcciones a la documentación. Tu rama se creó sobre `1bf1f5e`, así que hay que reaplicarla sobre el `main` actual con `rebase`, como indica CHALLENGE.md en «Responder a una revisión». No uses `merge`: el PR debe quedar como una serie lineal de *commits* sobre `main`.

1. **Antes de empezar,** haz un *commit* o un `git stash` de lo que tengas a medias: el paso 4 descarta los cambios sin guardar.
2. **Trae `main` y reaplica tu rama encima:**

   ```bash
   git remote add upstream https://github.com/incognia/dazner.git   # solo la primera vez
   git fetch upstream
   git switch challenge/punqee/add-windows-port
   git rebase upstream/main
   ```

3. **Resuelve el conflicto de `CHANGELOG.md`.** Tu bloque y el de `main` se agregan en el mismo lugar. Conserva los dos, con el tuyo arriba, y **cambia la fecha de tu bloque** por la de tu último *commit* (hora del centro de México): `check-pr.sh` exige las fechas en orden cronológico inverso, y un `2026-11-01` encima de una fecha posterior no pasa. Después, `git add CHANGELOG.md` y `git rebase --continue`.
4. **Refresca los finales de línea de tu copia.** Git de Windows trae `core.autocrlf=true`, así que tus archivos pueden estar en CRLF en disco. Con el `.gitattributes` nuevo, git los vería como modificados. Para volver a escribirlos con las reglas nuevas:

   ```bash
   git rm -r --cached -q .
   git reset --hard
   git status   # debe quedar limpio
   ```

5. **Atiende la revisión** como indica la sección anterior: primero la especificación y después el código. Al terminar, deja tus *commits* atómicos reescribiendo tu propia rama (`git commit --amend` o `git rebase -i upstream/main`), sin `fixup!`, y `CHANGELOG.md` solo en el último *commit*.
6. **Comprueba y sube:**

   ```bash
   bash ~/rules/scripts/check-pr.sh --base upstream/main
   git push --force-with-lease origin challenge/punqee/add-windows-port
   ```

   El PR #5 se actualiza solo. `--force-with-lease` es válido únicamente en tu rama de PR, nunca en `main`.

## Entorno del mantenedor

| Componente | Versión |
|---|---|
| Sistema | Windows 11 Pro 10.0.26300 |
| SDK de .NET | 10.0.403 (instalado durante la revisión) |
| *Runtime* de WebView2 | 157.0.4341.42 |
| Visual Studio | No instalado |
| Git | 2.55.0 para Windows, con `.gitattributes` y la configuración de `/git-init` |
