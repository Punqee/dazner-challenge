# Proposal

## Why

DAZNer solo existe para macOS, pero DAZN ya funciona bien en Chrome en Windows, lo que indica que el motor basado en Chromium reproduce el contenido con DRM. Una versión nativa permite usar en Windows el mismo visor: ventana sin bordes, proporción fija, dos paneles y pantalla completa dentro de la ventana.

## What Changes

- Añadir la versión de Windows en `windows/`, escrita en C# con WPF y un control de WebView2 por panel.
- Cumplir el contrato de paridad de las capacidades neutrales (`window-chrome`, `panes`, `fullscreen-emulation`, `session`, `playback`, `diagnostics`, `shortcuts`), con Ctrl como tecla de comando.
- Cargar el script compartido `shared/fill-fullscreen.js` y enlazar su gancho de registro.
- Empezar con una prueba mínima de reproducción de DAZN, que decide si se sigue.
- Documentar requisitos y compilación en el README, y actualizar la hoja de ruta.

Fuera del alcance: instalador, firma de código, publicación en tiendas, versiones ARM y cualquier cambio en las demás plataformas.

## Capabilities

### New Capabilities

- `platform-windows`: requisitos propios de la versión de Windows (paridad con el contrato, prueba de reproducción previa, proporción y centrado en el monitor, carpeta de sesión propia, requisitos documentados).

### Modified Capabilities

## Impact

- Nueva carpeta `windows/` con el proyecto de C#.
- `README.md` y `ROADMAP.md`: estado de Windows, requisitos y compilación.
- La prueba de reproducción (grupo 1 de las tareas) ya fue verificada. El script compartido `shared/fill-fullscreen.js` ya está disponible en el repositorio para los grupos siguientes.
- Requiere una máquina con Windows para compilar y probar; no se puede verificar desde macOS.
