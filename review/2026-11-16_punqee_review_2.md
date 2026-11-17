# Octava revisión del reto: Punqee (reporte final)

*Última modificación: 16 de noviembre de 2026, 19:37 (CST)*

Octava y última revisión de la entrega de Míriam ([@Punqee](https://github.com/Punqee)) al [reto de contribución](../CHALLENGE.md), hecha por el mantenedor con Claude Code (Opus 5.5) en una máquina con Windows. Revisa lo que entregó el 16 de noviembre para atender la [séptima revisión](2026-11-16_punqee_review.md) y cierra la evaluación del cambio `add-windows-port`. La calificación final está en [score/punqee.md](../score/punqee.md).

## Datos de la entrega

| Dato | Valor |
|---|---|
| PR | [#5](https://github.com/incognia/dazner/pull/5), abierto |
| Rama | `challenge/punqee/add-windows-port`, en `9602a30` |
| Base | `5c7d20d`, el `main` actual |
| *Commits* | 26 (los veinticinco de la séptima revisión, reaplicados, y uno nuevo) |
| Herramienta de IA | Antigravity con Gemini 3.8 Flash (esfuerzo alto) y Gemini 4 Argon (esfuerzo medio) |

| *Commit* | Asunto |
|---|---|
| `9602a30` | `docs(windows): update design and tasks following seventh review` |

## Puertas automáticas

- **`check-pr.sh --base upstream/main`:** pasan todas las comprobaciones, con el aviso de siempre: OpenSpec no se validó porque la máquina del mantenedor no tiene Node.
- **CHANGELOG:** solo agrega líneas y cambia solo en el último *commit*. Sus viñetas quedan en el bloque del 16 de noviembre, arriba de las del mantenedor.
- **Finales de línea:** `git ls-files --eol` no muestra ningún archivo con CRLF en el repositorio.
- **Archivos ajenos:** no toca `shared/`, `macos/`, `review/` ni `score/`.
- **GitHub Actions:** el flujo `PR checks` corrió para `9602a30` y quedó en `action_required`, retenido hasta que el mantenedor lo apruebe. Ese flujo instala Node y es el que valida OpenSpec.

## Lo que atendió de la séptima revisión

Todo, solo en la especificación, porque no se pidió código:

- **Tarea 3.2:** marcada, con la verificación del mantenedor del *multiview* con `Ctrl+2` y `Ctrl+1` anotada en `design.md` y en la lista de paridad.
- **El ícono:** `design.md` dice los siete tamaños en «Decisions» y en la verificación de la tarea 5.2, como el archivo.
- **PowerShell 7:** la excepción de entorno queda como verificada por el mantenedor, en la tarea 5.2 y en la lista de paridad.

El código no cambió desde la séptima revisión, así que no hizo falta repetir la compilación ni la prueba por RDP.

## Resumen del cambio `add-windows-port`

| Grupo | Tareas | Verificación |
|---|---|---|
| 1. Prueba de reproducción | 1.1 a 1.4 | DAZN reproduce en WebView2 con la cuenta del mantenedor y con la de la participante; foto de celular de cada uno. |
| 2. Ventana | 2.1 a 2.3 | Ventana sin bordes con franja y `Alt` + arrastrar, proporción fija 16:9 y 32:9 en uno y dos monitores, `Ctrl+F` y `Ctrl+T`. |
| 3. Paneles | 3.1 a 3.4 | Dos paneles con hueco de 8 px, segundo panel a demanda sin tamaño intermedio, audio por foco, `Ctrl+L` y enlaces en el mismo panel. |
| 4. Pantalla completa y sesión | 4.1 a 4.3 | Script compartido sin modificar, `--debug`, pantalla completa con *multiview* y sesión compartida entre paneles e instancias. |
| 5. Atajos, paquete y documentación | 5.1 a 5.4 | Atajos con `Ctrl` y edición nativa, ícono, `build.ps1` en PowerShell 5.1 y 7, README, ROADMAP y lista de paridad. |

Las 18 tareas están marcadas. El mantenedor verificó 17 en su equipo. La 5.3 tiene la documentación completa y espera solo `openspec validate`, que corre el flujo de GitHub Actions.

## Lo que deja el reto

- **Hallazgos técnicos que quedan documentados** en la bitácora de `design.md`:
  - La franja de arrastre y los bordes de WPF no reciben el ratón sobre WebView2.
  - `document.head` todavía no existe cuando WebView2 inyecta el script.
  - DAZN retira el `<style>` de `<head>` al montar sus rutas.
  - En WPF, `SetWindowPos` provoca un *layout* inmediato.
  - La política de ejecución de Windows PowerShell bloquea `build.ps1`.
- **Para el repositorio:** la evidencia de DRM en Windows (una captura con WebView2 puede no salir en negro) y el ancho al que DAZN deja de ofrecer el *multiview* (956 px) son datos nuevos que conviene llevar a `AGENTS.md`.
- **Pendientes del mantenedor**, que no son de la participante:
  - Comparar en macOS el hueco del *multiview*: 13/10 px en Windows contra 8 px en macOS.
  - Ajustar la introducción de `ROADMAP.md`.
  - El límite de la ventana en un monitor de 1920 px, que comparte con macOS.

## Veredicto final

- **Séptima revisión: atendida por completo.**
- **El PR cumple el reto:**
  - Compila sin intervención en clones nuevos siguiendo solo el README.
  - Arranca y reproduce DAZN.
  - Pasa `check-pr.sh`.
  - Siguió la especificación antes que el código en cada revisión.
  - Declaró siempre lo que no pudo probar.
- **Para integrarlo**, el mantenedor aprueba el flujo de GitHub Actions del PR. Si pasa, la tarea 5.3 queda verificada y el PR se integra. Si OpenSpec marca un error, se le pide a la participante corregirlo antes de integrar.
- **Calificación final: 9.2 / 10**, sujeta a que el flujo de GitHub Actions pase.
- **El PR cumple la condición del *referral*:** compila y arranca sin intervención y cumple las demás reglas, así que, según CHALLENGE.md, corresponde de forma automática.

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
