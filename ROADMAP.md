# Hoja de ruta

*Última modificación: 7 de noviembre de 2026, 15:30 (CST)*

DAZNer hoy existe solo para macOS. Esta hoja de ruta describe cómo reorganizar el repositorio y llevar el mismo visor a Windows y a Linux (GTK y Qt), sin perder lo que lo hace útil: ventana sin bordes, proporción fija, dos paneles y pantalla completa dentro de la ventana.

El detalle de cada fase (requisitos, diseño y tareas) vive en `openspec/changes/`, y el comportamiento que debe cumplir cada plataforma, en `openspec/specs/`. Esta hoja de ruta es la visión general; las instrucciones para un agente que construya una versión están en `AGENTS.md`.

## Principios

1. **Primero se prueba el DRM, después se construye la interfaz.** DAZN usa DRM y cada motor web lo trata distinto. En cada plataforma se hace una prueba de concepto mínima (abrir DAZN, iniciar sesión y reproducir un partido) antes de invertir en el resto. Si no reproduce, la fase termina ahí y se documenta.
2. **Motor web nativo de cada sistema**, mínimo y sin dependencias pesadas. Se recurre a un *framework* más grande solo si es la única vía con DRM.
3. **Lenguaje nativo de cada plataforma:** Swift en macOS, C# en Windows, C++/QML en Qt (el estilo de KDE) y Vala/C en GTK (el estilo de GNOME). El costo son cuatro bases de código; se compensa compartiendo lo que sí se puede (el *script* de pantalla completa, los íconos y la documentación) y vigilando la paridad con la tabla de funciones.
4. **Compartir lo compartible:** el *script* de pantalla completa, los íconos y la documentación viven en un solo lugar.
5. **La paridad se mide con la tabla de funciones** (al final), no a ojo.
6. **Sin eludir protecciones.** DAZNer abre el sitio oficial con una suscripción propia. No se evade DRM, límites de reproducción ni controles de acceso (ver `NOTICE.md`).

## Estado actual

| Plataforma | Estado |
|---|---|
| macOS (WebKit, `WKWebView`) | Publicada, versión inicial |
| Windows | Sin empezar |
| Linux GTK | Sin empezar |
| Linux Qt | Sin empezar |

## Orden de ejecución

La prueba de reproducción de cada plataforma (el primer grupo de tareas de su cambio de OpenSpec) no depende del *script* compartido: solo abre DAZN en el motor web de esa plataforma, y sus resultados deciden si se sigue con cada versión. El resto de cada versión carga `shared/fill-fullscreen.js`, que ya existe desde la fase 1.

| Paso | Qué | Dónde | Depende de |
|---|---|---|---|
| 0 | Reorganizar el repositorio (fase 0) | macOS | Hecho |
| 1a | Extraer el *script* de pantalla completa (`extract-fullscreen-script`, fase 1) | macOS | Hecho |
| 1b | Prueba de reproducción en Windows (`add-windows-port`, grupo 1) | Windows | Nada |
| 1c | Prueba de reproducción con Qt (`add-linux-qt-port`, grupo 1) | Linux | Nada |
| 1d | Prueba de reproducción con GTK (`add-linux-gtk-port`, grupo 1) | Linux | Nada |
| 2 | Decidir con los resultados y anotarlos | Cualquiera | 1b, 1c y 1d |
| 3 | Terminar la versión de Windows (grupos 2 a 5 de `add-windows-port`) | Windows | 1a, y que 1b pase |
| 4 | Terminar la versión con Qt (grupos 2 a 5 de `add-linux-qt-port`) | Linux | 1a, y que 1c pase |
| 5 | Terminar la versión con GTK (grupos 2 a 5 de `add-linux-gtk-port`) | Linux | 1a, y que 1d pase |
| 6 | Paridad, empaquetado, CI y publicación (fase 4) | Todas | 3, 4 y 5 |

Reglas del orden:

- Los pasos 1a, 1b, 1c y 1d son independientes entre sí y pueden hacerse a la vez en máquinas distintas.
- Si una prueba falla, se documenta el resultado y el paso siguiente de esa plataforma no se ejecuta.
- Los pasos 3, 4 y 5 son independientes entre sí. Si hay que elegir, el orden de prioridad es Windows, luego Qt y por último GTK, que es experimental.
- La fase 4 todavía no tiene un cambio de OpenSpec; se propone con `/opsx:propose` al llegar al paso 6.
- Los pendientes de cada paso, por máquina, están en `TODO.md`.

## Lo que ya sabemos del DRM

Datos confirmados en las pruebas del autor:

- **DAZN funciona bien en Chrome, en Windows y en Linux.** Chrome trae Widevine, el DRM que DAZN usa fuera de Apple.
- **En Firefox da problemas** por el DRM.

Qué implica para las fases:

- **Los motores basados en Chromium son la apuesta principal:** WebView2 en Windows y Qt WebEngine en Linux. Ninguno de los dos es Chrome, así que la prueba de concepto sigue siendo obligatoria, pero el riesgo baja mucho.
- **Gecko (Firefox) queda descartado como motor.** Además de los problemas con el DRM, incrustarlo en una aplicación propia es poco práctico.
- **WebKitGTK sigue siendo la duda.** Que Chrome funcione en Linux no dice nada sobre WebKitGTK, que no puede cargar el módulo Widevine de Chrome. Por eso la variante GTK pasa a ser experimental y la de Qt, la prioritaria.

## Fase 0: Reorganizar el repositorio (hecha)

Mover todo lo específico de macOS a su propia carpeta para dejar sitio a las demás plataformas.

Estructura propuesta:

```text
DAZNer/
├── macos/              DAZNer.swift, DAZNer.app/ (Info.plist e ícono), build.sh
├── windows/            (fase 2)
├── linux-gtk/          (fase 3a)
├── linux-qt/           (fase 3b)
├── shared/             assets/, scripts/make-icon.swift y fill-fullscreen.js
├── README.md           visión general y enlaces por plataforma
├── CHANGELOG.md        un solo historial para todo el proyecto
├── ROADMAP.md
├── NOTICE.md
└── LICENSE
```

Tareas:

- Mover con `git mv` para conservar el historial.
- Ajustar los comandos de compilación del README, el `.gitignore` y las rutas de `make-icon.swift`.
- Añadir `macos/build.sh` (compilar, firmar localmente y copiar los recursos) para que compilar no dependa de comandos sueltos en el README.
- Conservar el identificador `mx.org.faraday.dazner` y la ruta de la sesión, para que nadie pierda su sesión guardada.

Criterio de salida: la app de macOS compila y abre desde `macos/`, y el historial de los archivos movidos sigue visible en GitHub.

## Fase 1: Extraer el *script* de pantalla completa (hecha)

Antes de esta fase, el *script* que emula la pantalla completa vivía como cadena de texto dentro de `DAZNer.swift`. Es la parte más delicada del proyecto (cada comportamiento de DAZN que descubrimos está ahí) y es JavaScript puro, así que sirve en cualquier motor web.

Tareas:

- Moverlo a `shared/fill-fullscreen.js`, con el modo de diagnóstico controlado desde fuera.
- Reemplazar la llamada propia de WebKit para el registro (`webkit.messageHandlers.dbg`) por un gancho neutro, por ejemplo `window.__daznerLog(mensaje)`, que cada plataforma enlaza a su mecanismo.
- Hacer que macOS lo cargue desde `Contents/Resources` en lugar de tenerlo incrustado.

Criterio de salida: macOS se comporta igual que antes, con el *script* cargado desde `shared/`. Cumplido y verificado en macOS con el modo `--debug`: el botón general del *multiview* con cuatro videos (en sus dos diseños), los botones individuales del video principal y de los secundarios, el partido solo, la pantalla completa dentro de cada uno de los dos paneles y la salida con Esc.

## Fase 2: Windows

- **Motor:** WebView2 (Chromium de Edge).
- **Lenguaje y interfaz:** C# (.NET) con WPF. Microsoft documenta WebView2 para C# en WPF, WinForms y WinUI 3, y para C/C++ en Win32. Se elige WPF por ser el camino más directo para una ventana sin bordes con dos controles web.
- **Prueba de DRM (obligatoria):** WebView2 comparte el motor con Chrome y Edge, y DAZN ya funciona en Chrome para Windows, así que se espera que reproduzca. La prueba confirma que lo hace dentro de WebView2 y a qué calidad máxima.

Funciones y cómo se resuelven:

| Función | Enfoque en Windows |
|---|---|
| Ventana sin bordes y arrastre | Estilo sin bordes y `DragMove` desde una franja superior |
| Proporción fija 16:9 / 32:9 | Intercepción de `WM_SIZING` |
| Rellenar y centrar | Área de trabajo del monitor, con la proporción actual |
| Dos paneles con hueco de 8 px | Dos controles WebView2 en una cuadrícula |
| Audio por panel | `IsMuted` de cada WebView2 (API nativa, sin JavaScript) |
| Pantalla completa dentro del panel | `shared/fill-fullscreen.js` con `AddScriptToExecuteOnDocumentCreated` |
| Sesión persistente | Carpeta de datos de usuario de WebView2 |
| Siempre encima | `Topmost` |

Criterio de salida: paridad con la tabla de funciones, o con las limitaciones documentadas, y un ejecutable que no necesite instalador.

## Fase 3: Linux

Dos versiones, con una prueba de DRM antes de cada una. **Prioridad: primero Qt (3b)**, que es la que más probabilidades tiene de reproducir. GTK (3a) queda como variante experimental.

### 3a. GTK (GTK4 + WebKitGTK)

- **Riesgo principal:** WebKitGTK no trae un módulo de DRM comercial utilizable y no puede cargar el Widevine de Chrome. Aunque DAZN funcione en Chrome para Linux, se espera que no reproduzca aquí. La prueba de concepto decide si esta variante se construye o se descarta.
- **Si resulta viable:** Vala/C con GTK4 y WebKitGTK 6.0, el estilo de GNOME: la aplicación en Vala y C solo para lo que Vala no cubra, ya que Vala compila a C y se mezcla con él sin problema. Se compila con Meson.
- **Enlaces disponibles (verificado el 18 de octubre de 2026):** el repositorio de Vala trae `webkitgtk-6.0.vapi`, `gtk4.vapi` y `libsoup-3.0.vapi`, así que no hay que generar nada.
- **Por qué Vala:** su sintaxis es muy parecida a la de C#, así que la lógica de la versión de Windows se traslada casi directo.
- **Alternativa descartada por ahora:** Rust. Existe el *crate* `webkit6` (versión 0.6.1, de marzo de 2026), compatible con `gtk4` 0.11, así que sigue siendo viable si se cambia de opinión.
- **Si no lo es:** se documenta el resultado en este archivo y se conserva GTK como idea futura, por si cambia el soporte de DRM.

### 3b. Qt (Qt 6 + Qt WebEngine)

- **Motor:** Qt WebEngine (Chromium). Admite DRM si se le indica el módulo Widevine, que se toma de una instalación de Chrome o Firefox del usuario (por ejemplo con `--widevine-path`). DAZNer no lo redistribuye.
- **Lenguaje:** C++/QML, el estilo de KDE: la interfaz en QML (Qt Quick y el tipo `WebEngineView`) sobre un núcleo en C++. El C++ se mantiene mínimo: un `main.cpp` que arranca el motor de QML y cubre lo que QML no alcance (por ejemplo, la geometría de pantalla para «Rellenar y centrar»). Se compila con CMake.
- **Qué cubre QML sin código extra:** `userScripts` para el *script* de pantalla completa, `audioMuted` por panel, un perfil con almacenamiento persistente para la sesión, y `startSystemMove` y `startSystemResize` para mover y redimensionar una ventana sin bordes. Esto se confirma en la prueba de concepto.
- **Expectativa:** es la variante de Linux con más probabilidades de funcionar, porque comparte el motor con Chrome, donde DAZN ya reproduce bien. En Linux Widevine suele limitarse a un nivel de seguridad bajo, así que la calidad máxima puede ser menor que en macOS. Hay que medirlo.

### Limitaciones esperadas de Linux (según el servidor gráfico)

| Función | X11 | Wayland |
|---|---|---|
| Mover y redimensionar sin bordes | Sí, con las llamadas del sistema de ventanas | Sí, mediante `startSystemMove` y `startSystemResize` |
| Proporción fija | Sí, con sugerencias de tamaño | Sin API estándar: se emula corrigiendo el tamaño al redimensionar |
| Rellenar y centrar | Sí | El cliente no puede fijar su posición: solo se puede ajustar el tamaño |
| Siempre encima | Sí | Depende del compositor |

Criterio de salida: cada variante reproduce un partido, o queda documentado por qué no.

## Fase 4: Paridad, empaquetado y publicación

- Cerrar la tabla de funciones con el estado real de cada plataforma.
- Automatizar la compilación con GitHub Actions, con una matriz por sistema operativo. Cuando exista, se agrega el *badge* de CI al README.
- Artefactos de cada versión: macOS (carpeta `.app` comprimida), Windows (ejecutable comprimido o MSIX) y Linux (AppImage o Flatpak, según lo que permita el DRM).
- Íconos por plataforma, generados desde el mismo `shared/assets/icon.png`: `.icns` en macOS, `.ico` en Windows y PNG en varios tamaños para Linux.
- Un solo CHANGELOG y un solo número de versión del proyecto.
- Actualizar el README con enlaces y *badges* por plataforma.

## Tabla de funciones

Leyenda: **Sí** = hecho, **Pendiente** = planeado, **Probable** = el motor es el mismo de un navegador donde DAZN ya funciona y falta confirmarlo, **Dudoso** = se espera que falle y la prueba lo decide, **Limitada** = con restricciones conocidas.

| Función | macOS | Windows | Linux GTK | Linux Qt |
|---|---|---|---|---|
| Reproducción con DRM | Sí | Probable | Dudoso | Probable |
| Ventana sin bordes y arrastre | Sí | Pendiente | Pendiente | Pendiente |
| Proporción fija 16:9 / 32:9 | Sí | Pendiente | Limitada | Limitada |
| Rellenar y centrar | Sí | Pendiente | Limitada | Limitada |
| Dos paneles con hueco de 8 px | Sí | Pendiente | Pendiente | Pendiente |
| Segundo panel creado y liberado a demanda | Sí | Pendiente | Pendiente | Pendiente |
| Pantalla completa dentro de la ventana o el panel | Sí | Pendiente | Pendiente | Pendiente |
| Audio por panel | Sí | Pendiente | Pendiente | Pendiente |
| Sesión persistente | Sí | Pendiente | Pendiente | Pendiente |
| Siempre encima | Sí | Pendiente | Limitada | Limitada |
| Modo de diagnóstico `--debug` | Sí | Pendiente | Pendiente | Pendiente |
| Varias instancias | Sí | Pendiente | Pendiente | Pendiente |

## Riesgos y decisiones abiertas

- **DRM en Linux:** Chrome ya reproduce DAZN, así que Qt WebEngine debería hacerlo. El riesgo se concentra en GTK con WebKitGTK, y en que la calidad máxima en Linux sea menor que en macOS. Si GTK no reproduce, queda documentado como resultado válido.
- **Cambios en DAZN:** el *script* de pantalla completa depende de la estructura de la página (clases, contenedores y estilos de DAZN). Si DAZN la cambia, hay que ajustarlo en las cuatro plataformas a la vez; por eso vive en `shared/`.
- **Términos de DAZN:** DAZNer solo abre el sitio oficial, pero el uso del servicio queda sujeto a sus términos y condiciones.
- **Cuatro bases de código:** cada plataforma usa su lenguaje nativo, así que la lógica común (paneles, hueco, proporción, audio) se escribe cuatro veces. Se mitiga con el *script* compartido, la tabla de paridad y un comportamiento documentado que cada versión debe cumplir.
- **Distribución:** sin firma comercial en macOS ni en Windows habrá avisos del sistema. Firmar es una decisión de costo, aún sin tomar.

## Fuera de alcance

- Versiones para iOS y Android.
- Cualquier forma de eludir DRM, controles de acceso o límites de reproducción.
- Reproducir contenido que no sea el del sitio oficial de DAZN.
