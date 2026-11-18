# Paridad del sitio: skins

🌐 [English](skins.md) | [Português](skins.pt.md) | **Español**

La tienda de skins y sus páginas de administrador del sitio antiguo (`l4d2-playstats-web`), portadas al hub.

Las rutas de la columna "Dónde en el sitio antiguo" son relativas a `L4D2PlayStats.Web/L4D2PlayStats.Web`, salvo que empiecen por `Core/` (`L4D2PlayStats.Web/L4D2PlayStats.Core`).

Estado: `done` (mismo contenido y aspecto), `adapted` (presente, cambiado por el motivo indicado), `not ported` (no puede existir en el hub).

## Página de la tienda (`/skins`)

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Ruta `GET /skins`, elemento del menú "Skins" activo (`ViewBag.Skins`) | Controllers/SkinsController.cs:11-14 | done |  |
| Metadatos de página: título "Skins", descripción, imagen y alt de la skin más cara | PageMetadata/SkinsPageMetadata.cs:9-18, Views/Skins/Index.cshtml:18 | adapted | La descripción toma el nombre del sitio de los ajustes en lugar de nombrar "L4D2 Competitive". La imagen es la miniatura del sitio de assets, como dirección absoluta; sin una dirección de assets no hay imagen. |
| Estilos en línea de la página (`.sk-*`, 700 líneas) | Views/Skins/Index.cshtml:34-734 | done | Copiados sin cambios. |
| Token antiforgery en la página | Views/Skins/Index.cshtml:736 | done |  |
| Hero: antetítulo "Tienda de skins", título "Skins", texto introductorio | Views/Skins/Index.cshtml:738-742 | done |  |
| Hero: píldora de la temporada con icono de calendario, último día de la temporada y días restantes | Views/Skins/Index.cshtml:744-747, Core/Skins/SkinSeason.cs:11-14 | done | Las fechas vienen de la temporada del hub en la zona horaria del sitio, sea cual sea su duración. Se oculta cuando el administrador desactiva el reembolso de temporada. |
| Hero: botón "Cómo funciona" que abre el panel plegado (el caret gira) | Views/Skins/Index.cshtml:750-752, 80-86 | done |  |
| Hero: botón "¿Cómo uso mis skins en el juego?" | Views/Skins/Index.cshtml:753-755 | done |  |
| Monedero, sin sesión iniciada: estrella, "Entra con Steam para ver..." y el botón de inicio de sesión con regreso a la tienda | Views/Skins/Index.cshtml:760-769, 31 | done |  |
| Monedero, estrellas no disponibles: tarjeta de advertencia | Views/Skins/Index.cshtml:770-778 | adapted | El marcado y el texto se conservan, pero el hub lee las estrellas de su propia base de datos, así que el monedero de un jugador con sesión iniciada siempre está disponible y esta tarjeta nunca se muestra. |
| Monedero, con sesión iniciada: estrella, saldo, etiqueta "estrella(s) disponible(s)" (singular y plural) | Views/Skins/Index.cshtml:781-788 | done |  |
| Resumen del monedero: estrellas ganadas como MVP y estrellas gastadas | Views/Skins/Index.cshtml:789 | done |  |
| Monedero: número de MVPs de daño a SI y de MVPs de comunes | Views/Skins/Index.cshtml:790, Core/Skins/Results/WalletResult.cs:5-6 | done | Back-end añadido: `SkinStarsService` divide las estrellas en los dos tipos de MVP. |
| Cómo funciona: recuadros "1. Gana estrellas", "2. Compra y equipa", "3. Cambia en el juego" | Views/Skins/Index.cshtml:795-809 | done | Con el reembolso de temporada desactivado, el recuadro 2 termina con "es tuya para siempre" en lugar de "hasta el final de la temporada del ranking". |
| Cómo funciona: banner "Skins por temporada" con el fin de la temporada y los días restantes | Views/Skins/Index.cshtml:811-817 | done | El texto dice la duración real de la temporada ("cada 2 meses" de forma predeterminada, "cada mes" o "cada N meses" en los demás casos). Se oculta con el reembolso de temporada desactivado. |
| Cómo funciona: línea de créditos | Views/Skins/Index.cshtml:819 | done |  |
| Pestañas: "Tienda" con el número de skins | Views/Skins/Index.cshtml:1017-1022 | done |  |
| Pestañas: "Mis skins" con el número de skins adquiridas, solo con sesión iniciada | Views/Skins/Index.cshtml:1023-1030 | done |  |
| Pestañas: "Autores" con el número de autores | Views/Skins/Index.cshtml:1031-1035 | done |  |
| Pestaña conservada en la dirección (`#shop`, `#mine`, `#authors`) y restaurada al cargar | Views/Skins/Index.cshtml:1765-1771 | done |  |
| Barra de herramientas: filtro de armas con "Todas las armas" y los grupos "Armas de fuego" y "Armas cuerpo a cuerpo" | Views/Skins/Index.cshtml:1039-1054, Models/SkinsModel.cs:11-16 | done | Las quince armas del sitio antiguo, más cualquier otra arma que traiga un catálogo importado. |
| Barra de herramientas: filtro de rareza con "Todas las rarezas" y las cinco rarezas, la legendaria primero | Views/Skins/Index.cshtml:1055-1062, Models/SkinsModel.cs:37 | done |  |
| Barra de herramientas: checkbox "Solo las que puedo comprar", solo con monedero | Views/Skins/Index.cshtml:1063-1068 | done |  |
| Filtros aplicados en el navegador a todas las pestañas; secciones vacías ocultas; "Ninguna skin con estos filtros." | Views/Skins/Index.cshtml:1721-1763, 1093, 1118 | done |  |
| Pestaña Tienda: una sección por rareza (de legendaria a común), encabezado en color | Views/Skins/Index.cshtml:1073-1092, 282-326 | done |  |
| Encabezado de la rareza: número de skins y rango de precios | Views/Skins/Index.cshtml:1083 | done |  |
| Orden dentro de una rareza: precio descendente, orden de las armas, título en inglés | Models/SkinsModel.cs:70-80 | done |  |
| Tarjeta: `col-xs-6 col-sm-4 col-md-3`, atributos de datos `weapon`, `rarity`, `affordable` | Views/Skins/Index.cshtml:980-981 | done |  |
| Tarjeta: ancla `id="skin-{id}"` (solo en la pestaña Tienda), borde de skin adquirida, brillo de legendaria | Views/Skins/Index.cshtml:982 | done |  |
| Tarjeta: miniatura, con carga diferida | Views/Skins/Index.cshtml:984-985 | adapted | Se carga del sitio de assets (`{assets}/skins/{id}.jpg`) en lugar de `/imgs/skins`. Sin una dirección de assets, un marcador de posición transparente mantiene la forma de la tarjeta. |
| Tarjeta: etiqueta del arma sobre la miniatura | Views/Skins/Index.cshtml:986 | done |  |
| Tarjeta: etiqueta de estado "Equipada" (con icono de check) o "Comprada" | Views/Skins/Index.cshtml:987-996 | done |  |
| Tarjeta: etiqueta de precio con la estrella | Views/Skins/Index.cshtml:997 | done |  |
| Tarjeta: título con tooltip, en una sola línea con puntos suspensivos | Views/Skins/Index.cshtml:1000 | done |  |
| Tarjeta: "por {author}" con enlace al perfil de Steam del autor | Views/Skins/Index.cshtml:1001-1006 | done | Un autor importado sin Steam ID se muestra sin el enlace. |
| Tarjeta: nombre de la rareza en el color de la rareza | Views/Skins/Index.cshtml:1007 | done |  |
| Tarjeta: las fotos se funden al pasar el cursor (tomas 1, 5 y 6, tamaño `-card`, 1,6 s) | Views/Skins/Index.cshtml:1408-1474 | done | Comprobado frente a las fotos de producción a través de un proxy local. |
| Sondeo de compatibilidad con AVIF; las fotos se piden como `.avif` cuando es compatible, `.webp` en caso contrario | Views/Skins/Index.cshtml:1408-1421 | done |  |
| Acción, sin sesión iniciada: enlace "Entrar con Steam" | Views/Skins/Index.cshtml:829-833 | done |  |
| Acción, equipada: botón "Volver a la predeterminada" | Views/Skins/Index.cshtml:835-841 | done |  |
| Acción, adquirida: botón "Equipar" | Views/Skins/Index.cshtml:843-849 | done |  |
| Acción, sin monedero: "Comprar por {price}" deshabilitado | Views/Skins/Index.cshtml:851-855 | done |  |
| Acción, estrellas insuficientes: "Comprar por {price}" deshabilitado y "Faltan {n}" | Views/Skins/Index.cshtml:857-862 | done |  |
| Acción, puede comprar: botón "Comprar por {price}" con el icono del carrito | Views/Skins/Index.cshtml:864-866 | done |  |
| Pestaña Mis skins: botón "¿Cómo uso mis skins en el juego?" | Views/Skins/Index.cshtml:1099-1103 | done |  |
| Pestaña Mis skins: texto vacío "Todavía no tienes skins..." | Views/Skins/Index.cshtml:1104-1107 | done |  |
| Pestaña Mis skins: skins adquiridas ordenadas por arma, rareza y precio | Views/Skins/Index.cshtml:1110-1118, Models/SkinsModel.cs:45-52 | done | Una skin que salió del catálogo sigue aquí para su dueño, con la etiqueta "Fuera de venta" en los detalles. |
| Pestaña Autores: texto introductorio | Views/Skins/Index.cshtml:1123 | done |  |
| Pestaña Autores: un recuadro por autor con enlace al perfil, número de skins (singular y plural) y los títulos | Views/Skins/Index.cshtml:1124-1139, 25-30 | done | Los autores sin Steam ID se agrupan por nombre y se muestran sin el enlace. |
| Modal de detalles: se abre con un clic en cualquier parte de la tarjeta, salvo las acciones y el enlace del autor | Views/Skins/Index.cshtml:1588-1598, 1622-1628 | done |  |
| Modal de detalles: se abre solo para `/skins#skin-{id}` | Views/Skins/Index.cshtml:1773-1778 | done |  |
| Detalles: galería con la foto principal y seis miniaturas cuando la skin tiene fotos | Views/Skins/Index.cshtml:884-903, 1476-1506 | done | Se muestra cuando la skin tiene fotos y hay una dirección de assets definida. |
| Detalles: una sola imagen cuando la skin no tiene fotos, se abre en el lightbox | Views/Skins/Index.cshtml:904-911 | done |  |
| Detalles: lightbox con zoom y desplazamiento (panzoom), clic para ampliar, zoom con la rueda | Views/Skins/Index.cshtml:1312, 1508-1542 | done |  |
| Detalles: botones anterior y siguiente del lightbox y teclas de flecha | Views/Skins/Index.cshtml:1544-1586, 606-636 | done |  |
| Detalles: etiquetas de rareza, arma, "Equipada" o "Comprada" | Views/Skins/Index.cshtml:913-926 | done |  |
| Detalles: precio | Views/Skins/Index.cshtml:927 | done |  |
| Detalles: autor con enlace al perfil de Steam | Views/Skins/Index.cshtml:928-936 | done |  |
| Detalles: descripción localizada | Views/Skins/Index.cshtml:937-940 | done |  |
| Detalles: título y descripción "Original" del Workshop | Views/Skins/Index.cshtml:880, 941-946 | done |  |
| Detalles: el mismo botón de acción que la tarjeta | Views/Skins/Index.cshtml:947-949 | done |  |
| Detalles: "Comando en el chat" con `!skin {id}` y el botón de copiar, solo cuando la skin es del jugador | Views/Skins/Index.cshtml:950-959 | done | Usa el parcial compartido `_CopyButton`: mismo icono, popover y comportamiento. |
| Detalles: botón "Ver en el Workshop" | Views/Skins/Index.cshtml:960-962 | done | Se oculta para una skin importada sin id del Workshop. |
| Detalles: botón "Regalar (admin)" para administradores | Views/Skins/Index.cshtml:963-968, 1691-1699 | done | No se ofrece para una skin que está fuera de venta. |
| Pie del modal de detalles: enlace "¿Eres el autor?" y "Cerrar" | Views/Skins/Index.cshtml:1143-1161 | done | Se oculta solo cuando las dos direcciones del aviso están vacías en los ajustes de la tienda. |
| Modal del aviso para autores: cuatro párrafos | Views/Skins/Index.cshtml:1163-1180 | done |  |
| Modal del aviso para autores: botón "Escríbeme en Steam" | Views/Skins/Index.cshtml:1182-1184 | adapted | La dirección era el perfil de Steam del dueño, fijo en la vista. Ahora es el "Perfil de Steam para contacto" de los ajustes de la tienda; el botón se oculta mientras está vacío, que es el valor predeterminado. |
| Modal del aviso para autores: botón "Abrir una solicitud en GitHub", prerrellenado con el título de la skin, el id y la dirección del Workshop | Views/Skins/Index.cshtml:1185-1187, 1600-1608 | adapted | La dirección es la "Dirección para solicitudes de retirada" de los ajustes de la tienda. Empieza como la página de nueva issue de `l4d2-zone-server-assets`, como en el sitio antiguo, y se prerrellena de la misma forma. |
| Modal del aviso para autores apilado sobre el modal de detalles | Views/Skins/Index.cshtml:1610-1620 | done |  |
| Modal de confirmación de compra: texto, saldo antes y después, aviso de equipamiento, aviso de temporada, aviso final | Views/Skins/Index.cshtml:1197-1212, 1630-1650 | done | Con el reembolso de temporada desactivado se omite el aviso de temporada y la última línea dice "La compra no se puede deshacer." |
| Compra: `POST /api/shop/skins/{id}/buy`, modal de éxito, la página se recarga al cerrar | Views/Skins/Index.cshtml:1652-1667, Controllers/Api/SkinShopController.cs:31-41 | done |  |
| Equipar: `POST /api/shop/skins/{id}/equip`, modal de éxito, recarga | Views/Skins/Index.cshtml:1669-1683, Controllers/Api/SkinShopController.cs:43-53 | done |  |
| Volver a la predeterminada: `POST /api/shop/weapons/{weapon}/unequip`, modal de éxito, recarga | Views/Skins/Index.cshtml:1701-1714, Controllers/Api/SkinShopController.cs:55-65 | done |  |
| Modal de resultado y los textos de error por estado (`InsufficientStars`, `AlreadyOwned`, `StarsUnavailable`, `Busy`, `NotOwned`, `SkinNotFound`, genérico) | Views/Skins/Index.cshtml:1214-1226, 1333-1341, 1367-1406 | done | El hub devuelve los mismos nombres de estado. |
| Modal "Cómo usar tus skins en el juego": introducción, tabla de seis comandos, nombres de las armas, cinco reglas | Views/Skins/Index.cshtml:1228-1304, 1685-1689 | done |  |
| Modal de regalo disponible para los administradores en la página de la tienda | Views/Skins/Index.cshtml:1306-1309 | done |  |
| Móvil: padding del hero, tamaño del título y ancho del monedero por debajo de 700 px; galería con tres columnas por debajo de 480 px | Views/Skins/Index.cshtml:600-604, 719-732 | done |  |

## Modal de regalo (`_GiftModal`)

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Modal "Regalar una skin" con el texto introductorio | Views/Shared/_GiftModal.cshtml:61-72 | done |  |
| Jugador: recuadro del jugador elegido (avatar, nombre, Steam ID, SteamID64) o "Ningún jugador elegido" | Views/Shared/_GiftModal.cshtml:74-77, 138-148 | done | El avatar viene de `/avatar/{id}`. |
| Jugador: campo de texto para un Steam ID en cualquier formato o un enlace de perfil; "Steam ID escrito a mano" | Views/Shared/_GiftModal.cshtml:78-85, 183-187 | done |  |
| Jugador: "Elegir de la lista" abre el selector de jugadores y vuelve | Views/Shared/_GiftModal.cshtml:166-181 | done |  |
| Skin: recuadro de la skin elegida (miniatura, título, arma, rareza, precio) o "Ninguna skin elegida" | Views/Shared/_GiftModal.cshtml:87-90, 150-164 | done |  |
| Skin: "Elegir skin" abre el selector de skins y vuelve | Views/Shared/_GiftModal.cshtml:91-93, 189-198 | done |  |
| Confirmar: mensajes de validación cuando falta el jugador o la skin | Views/Shared/_GiftModal.cshtml:200-213 | done |  |
| Confirmar: `POST /api/shop/gift`, mensaje de éxito y recarga, error por estado, mensaje de Steam ID no válido con 400 | Views/Shared/_GiftModal.cshtml:215-237, Controllers/Api/SkinShopController.cs:85-96, 130-141 | done | Back-end añadido: `SkinRecipient` lee `STEAM_x:y:z`, `[U:1:n]`, SteamID64 y enlaces de perfil. |
| `GiftModal.open({ skin })` preselecciona una skin | Views/Shared/_GiftModal.cshtml:239-251 | done |  |
| Regalo: gratis, equipado de inmediato, guarda quién lo dio | Core/Skins/Services/SkinShopService.cs:186-228 | done |  |

## Modal del selector de skins (`_SkinPickerModal`)

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Modal grande "Elegir skin" | Views/Shared/_SkinPickerModal.cshtml:117-126 | done |  |
| Campo de búsqueda (nombre, id o arma), con el foco al abrirse el modal | Views/Shared/_SkinPickerModal.cshtml:128, 190-192, 222-224 | done |  |
| Filtro de armas y filtro de rareza | Views/Shared/_SkinPickerModal.cshtml:129-141 | done |  |
| Cuadrícula de mosaicos ordenados por rareza, precio y arma: miniatura, título, arma, rareza, precio | Views/Shared/_SkinPickerModal.cshtml:11-17, 143-162 | done | Miniaturas del sitio de assets. |
| "Ninguna skin con estos filtros." | Views/Shared/_SkinPickerModal.cshtml:163 | done |  |
| `SkinPicker.open(callback)` y `SkinPicker.find(id)` | Views/Shared/_SkinPickerModal.cshtml:237-260 | done |  |
| Móvil: dos mosaicos por fila por debajo de 700 px | Views/Shared/_SkinPickerModal.cshtml:111-115 | done |  |

## Precios de skins (`/admin/skins/prices`)

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Ruta `GET /admin/skins/prices`, solo administradores, menú "Admin" y "Precios de skins" activos | Controllers/SkinsAdminController.cs:14-15, 71-75 | done |  |
| Título "Precios de skins" y texto introductorio | Views/SkinsAdmin/Prices.cshtml:11, 24-28 | done |  |
| Estadística: skins con precio cambiado | Views/SkinsAdmin/Prices.cshtml:30-35 | done |  |
| Filtros: búsqueda por nombre o id, rareza, arma (se envía al cambiar), botón "Buscar", "Limpiar filtros" | Views/SkinsAdmin/Prices.cshtml:37-59, Controllers/SkinsAdminController.cs:81-86 | done |  |
| Lista ordenada por rareza, precio y arma; 20 por página | Controllers/SkinsAdminController.cs:22, 87-92 | done | El id de la skin desempata, para que las páginas sean estables. |
| Fila: miniatura, título, "arma · rareza · id" | Views/SkinsAdmin/Prices.cshtml:66-78 | done |  |
| Fila: precio actual; etiqueta "Cambiado" y el precio predeterminado cuando se cambió | Views/SkinsAdmin/Prices.cshtml:79-87 | done | Una skin que salió del catálogo recibe también la etiqueta "Fuera de venta". |
| "No se encontraron skins." | Views/SkinsAdmin/Prices.cshtml:62-65 | done |  |
| Paginación "Mostrando a–b de n" que conserva los filtros | Views/SkinsAdmin/Prices.cshtml:13, 92 | done |  |
| Modal de precio: imagen, arma, rareza, precio predeterminado, campo de precio (de 1 a 100.000), indicación | Views/SkinsAdmin/Prices.cshtml:94-134, 181-198 | done |  |
| Modal de precio: "Guardar" (`POST /api/shop/skins/{id}/price`), mensaje y recarga | Views/SkinsAdmin/Prices.cshtml:157-179, 200-209, Controllers/Api/SkinShopController.cs:67-83 | done |  |
| Modal de precio: "Restaurar predeterminado", solo para un precio cambiado | Views/SkinsAdmin/Prices.cshtml:192, 219-221 | done |  |
| Modal de precio: "Regalar" abre el modal de regalo con la skin | Views/SkinsAdmin/Prices.cshtml:211-217 | done | Se oculta para una skin que está fuera de venta. |
| Modal de precio: mensaje de precio no válido, campo de precio con el foco y seleccionado | Views/SkinsAdmin/Prices.cshtml:203-206, 223-225 | done |  |
| Reglas de precio: un precio igual al predeterminado quita el cambio; guarda quién lo cambió y cuándo | Core/Skins/Services/SkinService.cs:41-64 | done |  |

## Compras de skins (`/admin/skins/purchases`)

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Ruta `GET /admin/skins/purchases`, solo administradores, menú "Admin" y "Compras de skins" activos | Controllers/SkinsAdminController.cs:24-28 | adapted | La dirección del hub es `/admin/skins`; `/admin/skins/purchases` redirige a ella de forma permanente y conserva la query string. |
| Título "Compras de skins" | Views/SkinsAdmin/Purchases.cshtml:14, 269-270 | done |  |
| Encabezado: botón "Reembolsar todo", solo cuando hay algo que reembolsar | Views/SkinsAdmin/Purchases.cshtml:271-276 | done |  |
| Encabezado: el botón "Regalar skin" abre el modal de regalo | Views/SkinsAdmin/Purchases.cshtml:277-279 | done |  |
| Estadísticas: compras, estrellas gastadas, regalos dados | Views/SkinsAdmin/Purchases.cshtml:282-295 | done |  |
| Aviso: las compras se reembolsan al final de la temporada, con la fecha | Views/SkinsAdmin/Purchases.cshtml:297-300 | done | Se oculta con el reembolso de temporada desactivado. |
| Filtros: búsqueda por jugador, Steam ID o skin; botón "Buscar" | Views/SkinsAdmin/Purchases.cshtml:302-307, Models/SkinPurchaseRowModel.cs:20-31 | done | El nombre es el guardado con la compra, después el nombre del perfil del jugador, después el SteamID64. |
| Filtros: "Filtrar por jugador" abre el selector de jugadores | Views/SkinsAdmin/Purchases.cshtml:308-313, 601-608 | done |  |
| Filtros: chip del jugador elegido con avatar, nombre, cambiar y quitar | Views/SkinsAdmin/Purchases.cshtml:314-326, 22-56 | done |  |
| Filtros: "Limpiar filtros" | Views/SkinsAdmin/Purchases.cshtml:327-330 | done |  |
| Lista ordenada por fecha, la más reciente primero; 20 por página | Controllers/SkinsAdminController.cs:22, 43-54 | done |  |
| Fila: miniatura de la skin, título, "arma · id" | Views/SkinsAdmin/Purchases.cshtml:356-360 | done |  |
| Fila: avatar del jugador, nombre y SteamID64 | Views/SkinsAdmin/Purchases.cshtml:361-367 | done |  |
| Fila: precio, o etiqueta "Regalo" y "por {administrator}"; fecha | Views/SkinsAdmin/Purchases.cshtml:368-382 | adapted | La fecha se muestra en la zona horaria de los ajustes del sitio; el sitio antiguo usaba la hora local del servidor web. |
| "No se encontraron compras." | Views/SkinsAdmin/Purchases.cshtml:334-337 | done |  |
| Paginación que conserva la búsqueda y el jugador | Views/SkinsAdmin/Purchases.cshtml:16, 387 | done |  |
| Modal de detalles: hero en el color de la rareza, portada que enlaza a la skin en la tienda, rareza, insignia de regalo, título, "arma · id", "Ver en la tienda" | Views/SkinsAdmin/Purchases.cshtml:389-419, 558-576, 596-599 | done |  |
| Modal de detalles: recuadro del jugador con avatar, nombre, id, botones "solo las compras de este jugador" y "perfil del jugador" | Views/SkinsAdmin/Purchases.cshtml:420-433, 577-581 | done | El botón del perfil abre `/player/{id}`, portada por el área de jugadores. |
| Modal de detalles: datos de precio, saldo antes y después (compras), regalo dado por (regalos), fecha | Views/SkinsAdmin/Purchases.cshtml:434-453, 582-588 | done |  |
| Modal de detalles: "Reembolsar compra", texto de confirmación, "Confirmar reembolso" (`POST /api/shop/refund`), mensaje y recarga | Views/SkinsAdmin/Purchases.cshtml:455-466, 610-637, Controllers/Api/SkinShopController.cs:98-109 | done |  |
| Modal de reembolsar todo: introducción con los totales, advertencia, checkbox que habilita el botón, etiqueta de ejecución en curso | Views/SkinsAdmin/Purchases.cshtml:471-498, 643-660 | done |  |
| Reembolsar todo: `POST /api/shop/refund-all`, resumen, mensaje de elementos fallidos, recarga | Views/SkinsAdmin/Purchases.cshtml:662-686, Controllers/Api/SkinShopController.cs:111-119 | adapted | El hub reembolsa todo en una sola transacción de base de datos, así que `failed` siempre es 0 y el mensaje "elementos no se pudieron reembolsar", que se conserva en la página, nunca se muestra. |
| Modal de regalo en la página | Views/SkinsAdmin/Purchases.cshtml:500 | done |  |
| Móvil: las filas se ajustan y el jugador pasa a una segunda línea por debajo de 700 px; el hero se apila por debajo de 480 px | Views/Shared/_AdminStyles.cshtml, Views/SkinsAdmin/Purchases.cshtml:254-263 | done |  |

## APIs

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| `GET /api/shop/wallet` para el jugador con sesión iniciada: `balance`, `stars`, `spent` (lo usan las estrellas de la navbar) | Controllers/Api/SkinShopController.cs:18-29 | done | Nuevo en el hub. Nunca responde 503, porque las estrellas son locales. |
| `POST /api/shop/skins/{skinId}/buy`: `status`, `succeeded`, `price`, `balance` | Controllers/Api/SkinShopController.cs:31-41, 121-129 | done |  |
| `POST /api/shop/skins/{skinId}/equip` | Controllers/Api/SkinShopController.cs:43-53 | done |  |
| `POST /api/shop/weapons/{weapon}/unequip` | Controllers/Api/SkinShopController.cs:55-65 | done |  |
| `POST /api/shop/skins/{skinId}/price` (administrador): 400 fuera de rango, 404 skin desconocida, `price`, `defaultPrice`, `custom` | Controllers/Api/SkinShopController.cs:67-83 | done |  |
| `POST /api/shop/gift` (administrador): Steam ID en cualquier formato | Controllers/Api/SkinShopController.cs:85-96, 130-141 | done |  |
| `POST /api/shop/refund` (administrador) | Controllers/Api/SkinShopController.cs:98-109 | done |  |
| `POST /api/shop/refund-all` (administrador): `purchases`, `gifts`, `stars`, `players`, `failed` | Controllers/Api/SkinShopController.cs:111-119, Core/Skins/Results/RefundAllResult.cs | adapted | `failed` siempre es 0, ver la página de compras. |
| Token antiforgery obligatorio en el encabezado `RequestVerificationToken` en todos los POST | Controllers/Api/SkinShopController.cs:32, 44, 56, 69, 87, 100, 113 | done |  |
| `GET /api/skins`: las skins que alguien tiene (`id`, `weapon`), para el servidor de juego | Controllers/Api/SkinsController.cs:10-22 | done | Ya está en el hub, sin cambios. |
| `GET /api/players/skins`: skins y equipamiento de cada jugador, para el servidor de juego | Controllers/Api/PlayersController.cs:33-46 | done | Ya está en el hub, sin cambios. |
| `GET /api/players/{communityId}/wallet`: `mvpSiDamage`, `mvpCommon`, `stars`, `spent`, `balance` | Controllers/Api/PlayersController.cs:48-56 | done | `mvpSiDamage` y `mvpCommon` añadidos a la respuesta. |
| `POST /api/players/{communityId}/skins/equip` desde el servidor de juego: equipar o volver a la predeterminada | Controllers/Api/PlayersController.cs:58-69 | adapted | Misma ruta y cuerpo. El servidor de juego demuestra quién es con su token del hub (encabezado `Authorization`) en lugar del antiguo secreto compartido. |

## Reglas de la tienda y trabajo en segundo plano

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Estrellas: una por cada MVP de daño a SI y por cada MVP de comunes de todo el historial | Core/Skins/Services/SkinShopService.cs:29-38, 373-382 | done |  |
| Compra: comprueba la skin, la propiedad y el saldo; guarda el precio, el saldo anterior y el nombre del jugador; equipa de inmediato | Core/Skins/Services/SkinShopService.cs:69-138 | done | Una sola transacción de base de datos bajo el bloqueo de escritura reemplaza la transacción de table storage con reintentos. |
| Equipar exige tener la skin; una skin por arma | Core/Skins/Services/SkinShopService.cs:140-157 | done |  |
| Volver a la predeterminada quita el equipamiento del arma | Core/Skins/Services/SkinShopService.cs:159-184 | done |  |
| Reembolso: devuelve las estrellas, quita la skin y su equipamiento | Core/Skins/Services/SkinShopService.cs:230-311 | done |  |
| Reembolso de temporada: cada diez minutos se reembolsan las compras hechas antes de la temporada actual; los regalos se conservan | BackgroundServices/SkinSeasonRefundBackgroundService.cs:8-35, Core/Skins/Services/SkinShopService.cs:246-260, 313-316 | adapted | Sigue las temporadas del hub, de cualquier duración, y es un ajuste de la tienda que empieza activado. |
| Catálogo: id, arma, rareza, precio, id del Workshop, fotos, autor, títulos y descripciones en tres idiomas, título y descripción originales | Core/Skins/Skin.cs, Core/Skins/SkinAuthor.cs, Core/Skins/Data/skins.json | adapted | Diferencia deliberada del hub: el catálogo no viene incluido, el administrador importa `skins.json` en la página del catálogo. Se conservan todos los campos. |
| Miniaturas en `/imgs/skins/{id}.jpg`, fotos en `{SkinShotsUrl}/{id}/` | Core/Skins/Skin.cs:33, Views/Skins/Index.cshtml:871-874 | adapted | Diferencia deliberada del hub: `{assets}/skins/{id}.jpg` y `{assets}/skin-shots/{id}/`, con la dirección de assets de los ajustes de la tienda. |

## Páginas exclusivas del hub

| Elemento | Dónde en el hub | Estado | Notas |
|---|---|---|---|
| Página del catálogo de skins (`/admin/skins/catalog`): totales, último cambio, importación desde un archivo, importación desde la dirección configurada | Views/SkinsAdmin/Catalog.cshtml | done | Adaptada a `_AdminStyles` (encabezado, estadísticas, paneles). El último cambio se muestra en la zona horaria del sitio. |
| Página de ajustes de la tienda (`/admin/skins/settings`): reembolso al final de una temporada, dirección de los assets, dirección del catálogo | Views/SkinsAdmin/Settings.cshtml | done | Adaptada a los paneles de Bootstrap 3. Nuevo: las dos direcciones del aviso para autores de skins. |
| Pestañas que enlazan las cuatro páginas de administrador | Views/Shared/_SkinsAdminNav.cshtml | adapted | Las pestañas ya no están: las cuatro páginas están en el grupo Tienda de skins del menú lateral de la administración (`Views/Shared/_AdminMenu.cshtml`), que muestran todas las páginas de administrador, así que el camino al resto de la administración no se pierde en estas páginas. El contenido de las páginas no cambió. |

## Redacción

Los nombres de las rarezas en portugués y en español vuelven a las formas femeninas del sitio antiguo ("Lendária", "Épica", "Rara"; "Legendaria", "Épica", "Rara"), mediante los archivos de fragmentos `pt-BR.skins.json` y `es-ES.skins.json`.

## Cómo se comprobó

- La página de la tienda, sin sesión iniciada y en portugués, se leyó en un navegador en el hub y en https://l4d2.com.br/skins y los dos documentos se compararon nodo por nodo (etiquetas, clases, atributos y textos del cuerpo de la página y de cada modal, unas 13.700 líneas cada uno). Las únicas diferencias que quedan son el token antiforgery, los enlaces del pie de página y el nombre del sitio, que vienen de los ajustes.
- Las capturas de pantalla a 1280 y 400 píxeles de ancho, sin sesión iniciada, muestran la misma página en los dos sitios (solo difieren los elementos del menú de otras áreas).
- Con sesión iniciada como administrador sobre una copia de los datos de demostración: reembolso de una compra, compra con la confirmación, volver a la predeterminada, equipar, regalo con el selector de jugadores y el selector de skins, cambio y restauración de precio, filtros, pestañas conservadas en la dirección, enlaces `#skin-{id}`, fotos al pasar el cursor, galería y lightbox con las fotos de producción, tema claro, portugués y español.
- Las páginas de precios y de compras necesitan un administrador, así que no se pudieron abrir en el sitio en producción; se comprobaron línea por línea frente a las vistas antiguas.
