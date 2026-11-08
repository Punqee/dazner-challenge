# Tasks

## 1. Prueba de reproducción y saneamiento inicial (puerta: detenerse si falla)

- [x] 1.1 Crear en `windows/` un proyecto WPF mínimo con un WebView2 que abra `https://www.dazn.com/` y una solución que Visual Studio pueda abrir; verificar con `dotnet build` en PowerShell y compilando la solución en Visual Studio, sin errores en ambas vías (solo se puede verificar en Windows).
- [x] 1.2 Iniciar sesión y reproducir un partido; anotar en `design.md` si hay audio y video, la calidad máxima y cualquier error; verificar que el resultado queda escrito. **Si no reproduce, detenerse aquí**, documentar el resultado en `ROADMAP.md` y no seguir con los grupos siguientes (solo se puede verificar en Windows).
- [x] 1.3 Documentar los requisitos de Windows (SDK de .NET 10 con comando winget, runtime de WebView2, comando de compilación y ruta del binario) en la sección «Compilar (Windows)» de `README.md`; verificar que una persona pueda compilar en un clon nuevo siguiendo solo las instrucciones del README (solo se puede verificar en Windows).
- [ ] 1.4 Renombrar el proyecto y espacio de nombres a `DAZNer` (`windows/DAZNer.csproj`, `windows/DAZNer.sln`), corregir el espacio de nombres `xmlns:d` en `MainWindow.xaml` y retirar los `using` sobrantes en `App.xaml.cs` y `MainWindow.xaml.cs`; verificar con `dotnet build windows\DAZNer.sln` con 0 advertencias y 0 errores (solo se puede verificar en Windows).

## 2. Ventana

- [ ] 2.1 Ventana sin bordes con franja de arrastre de 14 px y Alt + arrastrar; verificar los escenarios de «Mover la ventana» de `window-chrome` (solo se puede verificar en Windows).
- [ ] 2.2 Proporción fija 16:9 y 32:9 al arrastrar los bordes con `WM_SIZING`; verificar los escenarios de «Proporción fija» en un monitor y en dos (solo se puede verificar en Windows).
- [ ] 2.3 Rellenar y centrar sobre el área de trabajo del monitor, y siempre encima; verificar los escenarios de ambos requisitos (solo se puede verificar en Windows).

## 3. Paneles

- [ ] 3.1 Modo de uno y dos paneles con hueco de 8 px dentro del ancho de la ventana; verificar las medidas del escenario de «Hueco entre paneles» (solo se puede verificar en Windows).
- [ ] 3.2 Crear el segundo panel a demanda y liberarlo al volver a uno, sin refrescar el primero; verificar que el primer panel sigue reproduciendo y que el proceso del segundo desaparece del Administrador de tareas (solo se puede verificar en Windows).
- [ ] 3.3 Audio por foco con `IsMuted`, cambio de panel y abrir dirección; verificar los escenarios de «Audio por foco» y de abrir dirección (solo se puede verificar en Windows).
- [ ] 3.4 Abrir en el mismo panel los enlaces de ventana nueva; verificar el escenario de `playback` (solo se puede verificar en Windows).

## 4. Pantalla completa y sesión

- [ ] 4.1 Cargar `shared/fill-fullscreen.js` con las variables previas y enlazar `window.__daznerLog` al mensaje web; verificar con `--debug` que `debug.log` recibe eventos (solo se puede verificar en Windows).
- [ ] 4.2 Probar la pantalla completa del video principal y de uno secundario, la general con cuatro videos y Esc; verificar los escenarios de `fullscreen-emulation` (solo se puede verificar en Windows).
- [ ] 4.3 Sesión persistente en la carpeta de datos propia, compartida entre paneles e instancias; verificar los escenarios de `session` abriendo tres instancias (solo se puede verificar en Windows).

## 5. Atajos, paquete y documentación

- [ ] 5.1 Atajos con Ctrl y acciones de edición; verificar los escenarios de `shortcuts` (solo se puede verificar en Windows).
- [ ] 5.2 Generar el `.ico` desde `shared/assets/icon.png` y añadir `windows/build.ps1`, que compile y copie el script compartido; con mensajes solo en ASCII y guardado con BOM según `.editorconfig`; verificar compilando en un clon nuevo con `build.ps1` en Windows PowerShell 5.1 y en PowerShell 7, y con Visual Studio, siguiendo solo el README y sin caracteres corruptos (solo se puede verificar en Windows).
- [ ] 5.3 Documentar requisitos y compilación en `README.md` y actualizar el estado en `ROADMAP.md`; verificar con `openspec validate add-windows-port --strict`.
- [ ] 5.4 Ejecutar la lista de verificación de paridad completa y anotar las excepciones con su motivo; verificar que `platform-windows` queda cumplida o con excepciones documentadas (solo se puede verificar en Windows).

## Workflow follow-up

- Archivar con `/opsx:archive` y registrar los cambios con `/changelogger` y `/commit`.
