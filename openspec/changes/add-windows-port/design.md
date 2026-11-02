## Context

La versión de macOS está escrita en Swift sobre `WKWebView`. DAZN reproduce con FairPlay en Apple y con Widevine en Chrome, así que cada plataforma depende del DRM del motor web que use. En Windows, DAZN ya funciona en Chrome, por lo que un motor basado en Chromium es la apuesta principal; WebKit para Windows queda descartado porque no trae ningún módulo de DRM.

## Goals / Non-Goals

**Goals:**

- Mismo comportamiento que macOS, medido con los escenarios de las especificaciones neutrales.
- Ejecutable que se compile en un clon nuevo con un solo comando.

**Non-Goals:**

- Instalador, firma de código, tiendas, ARM.
- Reescribir el script de pantalla completa: se usa el compartido.

## Decisions

- **Lenguaje e interfaz:** C# (.NET) con WPF. Microsoft documenta WebView2 para C# en WPF, WinForms y WinUI 3; WPF es el camino más directo para una ventana sin bordes con dos controles web. Se usa la versión LTS vigente de .NET.
- **Motor:** WebView2 (Chromium de Edge), un control por panel.
- **Ventana sin bordes:** estilo sin bordes, franja superior de 14 px que llama a `DragMove`, y Alt + arrastrar desde cualquier punto.
- **Proporción fija:** interceptar el mensaje `WM_SIZING` para ajustar el tamaño al arrastrar los bordes.
- **Rellenar y centrar:** área de trabajo del monitor que contiene la ventana, con la proporción actual.
- **Paneles:** una cuadrícula con dos columnas y un hueco de 8 px; el segundo control se crea al elegir dos paneles y se libera (`Dispose`) al volver a uno.
- **Audio por foco:** propiedad `IsMuted` de cada control, sin JavaScript.
- **Script compartido:** `AddScriptToExecuteOnDocumentCreated` con `window.__daznerDebug`, `window.__daznerLog` y `window.__pane` definidos antes; el registro llega por el mensaje web del control.
- **Sesión:** una carpeta de datos del usuario exclusiva de DAZNer, compartida entre paneles e instancias.
- **Compilación:** dos vías equivalentes, porque el autor usa ambas: abrir la solución de `windows/` en Visual Studio, o ejecutar en PowerShell `windows/build.ps1`, que llama a `dotnet build -c Release`, copia el script compartido y el ícono. Ambas usan el mismo proyecto.
- **Ícono:** `.ico` generado a partir de `shared/assets/icon.png`.

## Risks / Trade-offs

- **DRM no verificado en WebView2:** comparte el motor con Chrome, pero no es Chrome. Se mitiga con la prueba de reproducción como primera tarea y con una decisión explícita de detener el trabajo si falla.
- **Entorno de ejecución de WebView2:** puede no estar instalado en Windows 10. Se documenta como requisito.
- **Varias instancias con una misma carpeta de datos:** hay que comprobar que el motor lo admite sin bloqueos.
- **Codificación:** el repositorio es UTF-8, pero Windows usa Windows-1252 en muchos programas, y Git de Windows ensucia el historial con caracteres corruptos; el autor lo evita usando git desde WSL. Los archivos de Windows (`.cs`, `.xaml`, `.ps1`) pueden corromper los acentos si se guardan sin BOM, porque Windows PowerShell 5.1 los lee como ANSI. Se resuelve con `.editorconfig` (UTF-8 con BOM para esos tipos) y con mensajes en ASCII en `build.ps1`.

## Resultados de la prueba de reproducción

- **Inicio de sesión y reproducción:** Se inició sesión en DAZN con credenciales de usuario y se reprodujo un partido de la NFL de forma continua con audio y video sin interrupciones, validando que el DRM Widevine opera correctamente en WebView2.
- **Calidad máxima y medición:** Audio estéreo sincronizado y video fluido en alta definición (1080p / 720p según el flujo adaptativo del reproductor web de DAZN). La medición se realizó por inspección visual directa de la nitidez, fluidez y respuesta del reproductor dentro del control WebView2, sin cortes ni congelamientos.
- **Versiones del entorno:**
  - Sistema operativo: Windows 11 (NT 10.0.26200.0)
  - SDK de .NET: 10.0.402 (compilación de proyecto WPF para `net10.0-windows`)
  - Runtime de WebView2: 156.0.4314.58 (paquete NuGet `Microsoft.Web.WebView2` 1.0.4314.35)
- **Errores observados y resolución:**
  - Se resolvió un obstáculo previo con paquetes de instalación de Visual C++ Runtime mediante la actualización y reparación de componentes desde el instalador de Visual Studio.
  - La compilación en PowerShell mediante `dotnet build` y en Visual Studio se completó con 0 errores y 0 advertencias.
- **Evidencia:** Fotografía tomada con teléfono móvil a la pantalla completa del monitor donde se aprecia la ventana de DAZNer reproduciendo el partido sobre el escritorio de Windows (para evitar la pantalla negra provocada por la captura de DRM), disponible y enviada como evidencia del reto.

## Open Questions

- ¿Dónde vive el clon que se compila? El autor usa git desde WSL y compila desde Windows nativo. Si el repositorio está en el sistema de archivos de WSL, compilar desde Windows pasa por una ruta de red de WSL y puede ser lento o dar problemas; se prueba y, si hace falta, se documenta clonar en una ruta de Windows y trabajar el control de versiones desde WSL con `/mnt/c`.
- ¿Hace falta además un `.gitattributes` para los finales de línea? El problema confirmado es de codificación y no de finales de línea, así que por ahora no; se reconsidera si aparece ruido de CRLF.
- ¿Distribución como carpeta comprimida o como paquete MSIX? No cambia lo que se construye en esta fase; se decide al empaquetar.
- Calidad máxima real de reproducción en WebView2 frente a Chrome: comprobada en alta definición (hasta 1080p/720p adaptativo) durante la prueba de la tarea 1.2 (detallada en la sección de resultados).
