# Configuración pendiente para U10, U11 y U12

## U10: ubicaciones de proveedores

El esquema EF ya incluye `Proveedor.Latitud` y `Proveedor.Longitud`, pero la
migración U11/U12 no agrega esas columnas: se agregan manualmente como pide la
actividad. Ejecutá el `ALTER TABLE proveedores` de
[u10-u12-publicacion.sql](./u10-u12-publicacion.sql) en la base publicada y
revisá los IDs con el `SELECT` incluido. Las coordenadas reales aún no están en
el repositorio; no se deben completar con ubicaciones inventadas.

## U12: imágenes de lotes

El formulario permite elegir una imagen JPG, PNG o WEBP (hasta 5 MB) al crear o
editar un lote. La API la valida, la sube primero y solo después se persiste su
URL en `lotes.ImagenAvatar`.

En desarrollo, si no hay configuración de Supabase, la API usa
`wwwroot/uploads/lotes`. En producción, Supabase es obligatorio: si no está
configurado, la subida responde HTTP 502 y no persiste el lote. Para activar
Supabase en producción, crear un bucket público llamado `imagenes` y
configurar únicamente en el servidor:

- `SupabaseStorage__Url`: URL HTTPS del proyecto.
- `SupabaseStorage__Bucket`: nombre del bucket.
- `SupabaseStorage__ServiceRoleKey`: clave de servicio, guardada como secreto
  en el panel del hosting; nunca incluirla en el frontend, el repositorio ni el
  chat.

Si hay configuración parcial, la API falla explícitamente al iniciar. Si la
carga remota falla, responde HTTP 502 y no actualiza la base de datos.

El nombre de bucket configurado es `imagenes`. `ImagenAvatar` se agrega con el
SQL manual de [u10-u12-publicacion.sql](./u10-u12-publicacion.sql) y no forma
parte de las migraciones. Ejecutá ambos `ALTER TABLE` antes de desplegar la API.

## U12: Google y App Links

El botón nativo de Google aparece solo en Android. Crear un cliente OAuth Web y
clientes Android para el paquete `com.syner.app` con las huellas SHA-1
correspondientes. Configurar el mismo ID de cliente Web en:

- `.env.release` como `VITE_GOOGLE_WEB_CLIENT_ID` (lo usa Capacitor al
  sincronizar Android).
- El hosting de la API como `GoogleAuth__WebClientId`.

Además, configurar en el hosting `AndroidAppLinks__Sha256CertFingerprints` con
las huellas SHA-256 separadas por coma o punto y coma. La API las publica en
`/.well-known/assetlinks.json`; para verificar una instalación de Play también
se necesita la huella de firma de Play.

El archivo local `android/app/google-services.json` también debe corresponder al
paquete Android `com.syner.app` para que Gradle compile y Firebase Push use el
cliente correcto. Descargalo de Firebase Console; no es necesario pegarlo en el
chat.

El QR de pedido abre `/p/{token}`. Con la app instalada, el intent-filter de
Android la abre; sin la app, la API sirve la página pública sin datos del
solicitante. El catálogo está disponible en `/catalogo`.

La política publicada en `/privacidad.html` identifica como responsable a
Hernán Gonzalo Constante y permite solicitar acceso, corrección o eliminación
de datos en `hernanbonne98@gmail.com`.
