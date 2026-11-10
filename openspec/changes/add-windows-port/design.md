## Context

La versión de macOS está escrita en Swift sobre `WKWebView`. DAZN reproduce con FairPlay en Apple y con Widevine en Chrome, así que cada plataforma depende del DRM del motor web que use. En Windows, DAZN ya funciona en Chrome, por lo que un motor basado en Chromium es la apuesta principal; WebKit para Windows queda descartado porque no trae ningún módulo de DRM.

## Goals / Non-Goals

**Goals:**

- Mismo comportamiento que macOS, medido con los escenarios de las especificaciones neutrales.
- Ejecutable que se compile en un clon nuevo con un solo comando.

**Non-Goals:**

- Instalador, firma de código, tiendas, ARM.
- Reescribir el script de pantalla completa: se usa el compartido (`shared/fill-fullscreen.js`).

## Decisions

- **Lenguaje e interfaz:** C# (.NET) con WPF. Microsoft documenta WebView2 para C# en WPF, WinForms y WinUI 3; WPF es el camino más directo para una ventana sin bordes con dos controles web. Se usa .NET 10.
- **Proyecto y espacio de nombres:** El proyecto y su espacio de nombres se denominan `DAZNer` (`windows/DAZNer.csproj`, `windows/DAZNer.sln`).
- **Motor:** WebView2 (Chromium de Edge), un control por panel.
- **Ventana sin bordes:** estilo sin bordes con `WindowChrome`, franja superior de 14 px (con doble clic para maximizar o restaurar) y `Alt` + arrastrar desde cualquier punto mediante `WH_MOUSE_LL` y `SetWindowPos`, sin dejar elementos XAML (`Border`) ni manejadores de ratón de WPF sin ejecutar.
- **Proporción fija:** interceptar el mensaje `WM_SIZING` en `WndProc` para ajustar el tamaño al arrastrar los bordes (16:9 con un panel y 32:9 con dos), deshabilitando de forma temporal la ventana hija de `WebView2` (`EnableWindow`) cuando el puntero entra al borde de redimensionado para que `MainWindow` reciba `WM_NCHITTEST`. El rectángulo de la ventana y la escala de DPI se guardan en memoria al iniciar y al recibir `WM_MOVE`, `WM_SIZE` o `WM_DPICHANGED`, evitando llamadas Win32 en cada movimiento del ratón dentro del *hook*.
- **Rellenar y centrar y siempre encima:** `Ctrl+F` obtiene el área de trabajo (`rcWork`) del monitor que contiene la ventana mediante `MonitorFromWindow` y `GetMonitorInfo`, calcula el mayor rectángulo con la proporción actual y lo centra con `SetWindowPos`; `Ctrl+T` alterna la propiedad `Topmost`.
- **Paneles (`Ctrl+1` y `Ctrl+2`):** una cuadrícula (`Grid`) con dos columnas de contenido (`*`) separadas por una columna de `8 px` de fondo negro incluida dentro del ancho de la ventana (`(W - 8) / 2` por panel); el segundo control `WebView2` se crea a demanda al elegir dos paneles y se detiene y libera (`Dispose`) al volver a uno, redimensionando la ventana entre 16:9 y 32:9 sin recargar el primer panel.
- **Audio por foco, cambio de panel y abrir dirección:** propiedad `CoreWebView2.IsMuted` de cada control (sin JavaScript, de modo que no altera el silencio manual aplicado dentro del reproductor de DAZN), cambio de foco al hacer clic en un panel o mediante atajo (`Ctrl+\`` / `Ctrl+|` / `Ctrl+Tab`), y diálogo modal con `Ctrl+L` para abrir una dirección web en el panel con foco.
- **Enlaces de ventana nueva:** el evento `CoreWebView2.NewWindowRequested` marca `Handled = true` y navega a la dirección solicitada en el mismo panel.
- **Script compartido:** `AddScriptToExecuteOnDocumentCreated` con `window.__daznerDebug`, `window.__daznerLog` y `window.__pane` definidos antes; el registro llega por el mensaje web del control.
- **Sesión:** una carpeta de datos del usuario exclusiva de DAZNer, compartida entre paneles e instancias.
- **Compilación:** dos vías equivalentes: abrir la solución de `windows/DAZNer.sln` en Visual Studio, o ejecutar en PowerShell `windows/build.ps1`, que llama a `dotnet build -c Release`, copia el script compartido y el ícono. Ambas usan el mismo proyecto.
- **Ícono:** `.ico` generado a partir de `shared/assets/icon.png`.
- **Flujo en Windows y codificación:** Se trabaja desde Windows nativo con Git para Windows y PowerShell. `.gitattributes` fija los finales de línea (LF en repositorio, CRLF en disco para `.sln`, `.bat` y `.cmd`) sin alterar `core.autocrlf`, y `.editorconfig` fija UTF-8 con BOM en `.cs`, `.xaml` y `.ps1`.

## Risks / Trade-offs

- **DRM no verificado en WebView2:** Comparte el motor con Chrome, pero no es Chrome. Se mitigó con la prueba de reproducción del grupo 1, superada en Windows.
- **Requisitos de compilación sin intervención:** La máquina que compila requiere el SDK de .NET 10 y el runtime de WebView2 instalados previamente. Se documentan en la bitácora y en `README.md`.
- **Entorno de ejecución de WebView2:** Preinstalado en Windows 11; en Windows 10 desactualizado puede requerir instalación del runtime Evergreen.
- **Hook global de ratón (`WH_MOUSE_LL`):** Recibe todos los movimientos del ratón del sistema y se ejecuta en el hilo de la interfaz de usuario. Si la aplicación se bloquea, el puntero de todo Windows puede volverse lento; si supera el límite de tiempo del sistema (`LowLevelHooksTimeout`), Windows retira el *hook* sin previo aviso y el arrastre deja de funcionar. Para reducir al mínimo su costo en cada `WM_MOUSEMOVE`, el rectángulo y el DPI de la ventana se mantienen en caché al mover o redimensionar la ventana, sin invocar `GetWindowRect` ni `GetDpiForWindow` en el *hook*. Como alternativa futura para la franja superior puede evaluarse el soporte de regiones no cliente de WebView2 (`IsNonClientRegionSupportEnabled` con la propiedad CSS `app-region: drag`).
- **Varias instancias con una misma carpeta de datos:** Hay que comprobar que el motor lo admite sin bloqueos al implementar el grupo 4.

## Resultados de la prueba de reproducción

- **Inicio de sesión y reproducción:** Se inició sesión con una cuenta propia de DAZN de acceso limitado (sin suscripción de pago a NFL Game Pass), probando con contenido y partidos gratuitos disponibles en la plataforma. Se validó que la aplicación abre DAZN y reproduce video dentro del control WebView2. La prueba de reproducción fue posteriormente confirmada y validada en Windows por el mantenedor con su propia cuenta activa.
- **Calidad y fluidez:** Se observó reproducción continua con audio y video fluido en la ventana. La evaluación de calidad fue por inspección visual subjetiva del comportamiento del reproductor dentro de WebView2, sin mediciones de tasa de bits ni extracción de métricas mediante `chrome://media-internals` o panel de estadísticas.
- **Versiones del entorno de la participante:**
  - Sistema operativo: Windows 11 (NT 10.0.26200.0)
  - Pantalla: 1 monitor físico (`1920×1080`, área de trabajo útil `1920×1032`); el redimensionado con proporción fija (`WM_SIZING`) y «Rellenar y centrar» (`Ctrl+F`) se verificaron en este monitor, declarando sin probar el escenario en un segundo monitor físico
  - SDK de .NET: 10.0.402 (compilación de proyecto WPF para `net10.0-windows`)
  - Runtime de WebView2: 156.0.4314.58 (paquete NuGet `Microsoft.Web.WebView2` 1.0.4314.35)
- **Validación del mantenedor (revisión externa):**
  - Sistema operativo: Windows 11 Pro 10.0.26300
  - SDK de .NET: 10.0.403
  - Runtime de WebView2: 157.0.4341.42
  - Resultado: reproducción con cuenta activa confirmada en WebView2.
- **Evidencia:** Fotografía tomada con teléfono celular (Samsung Galaxy S24) en formato horizontal a la pantalla del monitor con la aplicación corriendo desde Visual Studio.

## Bitácora de problemas y dependencias

| Síntoma | Causa | Solución | ¿Requiere acción previa de quien compila? |
|---|---|---|---|
| `No .NET SDKs were found` al ejecutar `dotnet build windows\DAZNer.sln` | La máquina no tiene instalado el SDK de .NET 10 (solo cuenta con *runtimes* o versiones anteriores) | Instalar el SDK mediante `winget install --id Microsoft.DotNet.SDK.10 -e` | Sí, instalar el SDK de .NET 10 antes de compilar |
| Falta del motor de WebView2 en sistemas Windows 10 no actualizados | Windows 10 no siempre incluye preinstalado WebView2 Evergreen Runtime | Verificar en el registro o instalar mediante el instalador Evergreen de WebView2 | Sí, verificar o instalar WebView2 Runtime en Windows 10 |
| Obstáculo con dependencias de C++ durante pruebas iniciales de ejecución | Falta de bibliotecas de ejecución de Visual C++ en el sistema | Actualizar y reparar los componentes de Visual C++ desde el instalador de Visual Studio | Sí, disponer del Visual C++ Redistributable si el sistema lo requiere |
| La franja de arrastre en XAML (`Border`) y `PreviewMouseLeftButtonDown` no reciben clics sobre `WebView2`, y `PostMessage(WM_NCLBUTTONDOWN, HTCAPTION)` tras bloquear `WM_LBUTTONDOWN` en `WH_MOUSE_LL` no inicia el movimiento | `WebView2` es un `HwndHost` Win32 que se dibuja por encima de los elementos WPF (*airspace issue*); además, devolver `1` en `WH_MOUSE_LL` impide que Windows marque `VK_LBUTTON` como presionado, por lo que `DefWindowProc` cancela `SC_MOVE` | Interceptar `WM_LBUTTONDOWN`, `WM_MOUSEMOVE` y `WM_LBUTTONUP` con un *hook* `WH_MOUSE_LL`, verificando `GetAncestor(WindowFromPoint(pt), GA_ROOT)` y `GetAsyncKeyState(VK_MENU)`, y desplazando la ventana con `SetWindowPos` | No |
| Los bordes de redimensionado de `WindowChrome` (`ResizeBorderThickness`) no reciben `WM_NCHITTEST` porque la ventana hija de `WebView2` cubre toda el área cliente | `WindowFromPoint` devuelve `Chrome_RenderWidgetHostHWND` hasta el último píxel del borde interior de `MainWindow`, por lo que Windows no consulta `WM_NCHITTEST` en la ventana principal | Deshabilitar de forma temporal el `HWND` anfitrión de `WebView2` (`EnableWindow`) únicamente mientras el cursor está en el borde de redimensionado en estado normal, permitiendo que `MainWindow` reciba `WM_NCHITTEST` e inicie el bucle nativo `WM_SIZING` | No |

## Open Questions

- **¿Dónde vive el clon que se compila?** En el sistema de archivos nativo de Windows. El control de versiones con Git para Windows, la compilación con `dotnet` y la ejecución se hacen directamente en Windows nativo.
- **¿Cómo se evita el ruido de codificación?** El repositorio cuenta con `.gitattributes`, que fija LF para todos los archivos del repositorio (CRLF en disco para `.sln`, `.bat` y `.cmd`) sin modificar `core.autocrlf`, y `.editorconfig`, que fija UTF-8 con BOM para `.cs`, `.xaml` y `.ps1`.
- **¿Cómo se ejecutan los comandos y *commits*?** Desde Windows nativo (PowerShell y Git para Windows), asegurando que los archivos respeten los finales de línea y la codificación requerida.
- **¿Hace falta además un `.gitattributes` para los finales de línea?** Resuelta: `.gitattributes` ya fue incorporado a `main` y normalizado en la copia de trabajo.
- **¿Distribución como carpeta comprimida o como paquete MSIX?** No cambia lo que se construye en esta fase; se decide al empaquetar.
- **Calidad máxima real de reproducción en WebView2 frente a Chrome:** La prueba inicial validó fluidez en contenido abierto y el mantenedor confirmó la reproducción protegida de eventos de pago en su entorno, aunque la resolución o calidad máxima no se midió.
