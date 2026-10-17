# U8: offline, API publicada y distribución interna

## Estado del proyecto

- **Lectura offline:** se guarda una copia local de las páginas/listas de lotes consultadas. Se conservan hasta 15 consultas recientes por página, búsqueda y orden.
- **Escritura offline:** solo se encola el cambio de estado de un lote. Crear, editar, dar de baja, comprar cupos y otras escrituras requieren conexión.
- **Cambio pendiente:** el estado local se superpone a la respuesta/cache y muestra `Sin enviar`, también después de reabrir la aplicación.
- **Sincronización:** Capacitor Network detecta el regreso de la conexión. Los errores temporales quedan en cola; los rechazos definitivos se quitan. Si la API ya aplicó el cambio antes de perderse la respuesta, se consulta el lote antes de quitarlo.
- **Acceso offline:** la sesión se conserva localmente y las rutas siguen protegidas por autenticación/rol. Solo se garantiza contenido offline para lotes que ya se hayan cargado; las otras pantallas pueden necesitar conexión.
- **Firma:** Gradle usa `android/keystore.properties` si existe. Sin ese archivo genera un APK release sin firma; no sirve para entregar.
- **API:** el proyecto apunta a .NET 10 y MySQL. En producción las migraciones se aplican al iniciar, pero no se crean cuentas ni lotes de demostración. El primer administrador se configura con secretos del hosting.

## Lo que tenés que hacer

### 1. Crear y respaldar el keystore

Generalo una sola vez en una carpeta fuera del repositorio y de OneDrive. Guardá el alias y las contraseñas en un gestor seguro. Copiá el keystore a un respaldo externo protegido ese mismo día. Sin este archivo no vas a poder firmar futuras actualizaciones con la misma identidad.

En PowerShell, usando un alias propio:

```powershell
keytool -genkeypair -v -keystore C:\secure\syner-release.jks -alias syner-release -keyalg RSA -keysize 2048 -validity 10000
```

Creá `android/keystore.properties` localmente (está ignorado por Git):

```properties
storeFile=C:/secure/syner-release.jks
storePassword=LA_CLAVE_DEL_KEYSTORE
keyAlias=syner-release
keyPassword=LA_CLAVE_DE_LA_CLAVE
```

No subas ni el `.jks` ni `keystore.properties` al repositorio, no los mandes por chat y no compartas sus valores. Mantené el respaldo cifrado y separado de esta PC.

### 2. Crear la API y la base en MonsterASP

En el panel del hosting, creá la aplicación web con runtime **.NET 10** y una base **MySQL nueva y vacía**. No copies la base de desarrollo: el seed productivo no agrega más demos, pero tampoco borra usuarios/datos de una base existente. Conservá los datos de conexión que entrega el panel. Configurá estas variables de entorno en la aplicación, no en archivos versionados:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__Default=<cadena MySQL entregada por el hosting>
Jwt__Key=<secreto aleatorio de al menos 64 bytes, codificado en Base64>
Jwt__Issuer=Syner.Api
Jwt__Audience=Syner.App
Jwt__ExpirationMinutes=15
Jwt__RefreshTokenExpirationDays=30
BootstrapAdmin__Name=<nombre del administrador inicial>
BootstrapAdmin__Email=<correo del administrador inicial>
BootstrapAdmin__Phone=<teléfono del administrador inicial>
BootstrapAdmin__Dni=<DNI del administrador inicial>
BootstrapAdmin__Password=<contraseña fuerte y única>
```

Para generar `Jwt__Key` desde PowerShell:

```powershell
$bytes = New-Object byte[] 64
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

La primera puesta en marcha crea el administrador solo si todavía no hay uno. Después de verificar el acceso, podés quitar `BootstrapAdmin__Password` del panel; conservá el resto solo si necesitás volver a inicializar una base vacía. No uses las contraseñas demo de desarrollo.

Opcionalmente, para una web publicada que llame a la API, configurá `Cors__AllowedOrigins__0=https://<dominio-web>`. La app Android usa `http://localhost`, permitido por la API. Para habilitar push, configurá `Firebase__ServiceAccountPath` con una credencial guardada fuera de la carpeta pública del sitio y fuera de Git; push no es requisito para probar offline.

### 3. Publicar la API

Desde la raíz del repositorio:

```powershell
dotnet publish .\Syner.Api\Syner.Api.csproj -c Release -o .\Syner.Api\publish
```

Subí el contenido generado dentro de `Syner.Api/publish` al directorio de publicación configurado para la app en MonsterASP y reiniciala. Confirmá en los logs que terminó el arranque y que aplicó migraciones. En producción no hay datos ficticios: iniciá sesión con el administrador inicial y cargá desde la app los datos reales de lotes/proveedores que vas a mostrar. Probá primero el login y la lista con HTTPS desde un dispositivo conectado.

### 4. Preparar y firmar el APK

Copiá `.env.release.example` a `.env.release` y reemplazá el ejemplo por la URL HTTPS real, sin barra final:

```dotenv
VITE_API_URL=https://<host-publico-monsterasp>/api
```

El build de release falla si falta la URL o no es HTTPS. Después, desde la raíz:

```powershell
npm install
npm run build:release
npx cap sync android
cd android
.\gradlew.bat assembleRelease
```

Con `keystore.properties` presente, Gradle firma el APK. El resultado es `android/app/build/outputs/apk/release/app-release.apk`. Antes de instalarlo, verificá la firma con `apksigner verify --verbose --print-certs <ruta-al-apk>` (la herramienta viene con Android SDK Build Tools).

### 5. Instalar y grabar la prueba

Copiá el APK al teléfono e instalalo. Iniciá la app desde su ícono e ingresá con el administrador. Con conexión, abrí Lotes y asegurate de que haya lotes reales cargados.

Para el video:

1. Mostrá el ícono y abrí la app instalada.
2. Mostrá que Lotes carga datos de tu API publicada.
3. Activá modo avión, volvé a Lotes y comprobá que aparecen los datos guardados y el aviso de desconexión.
4. Cambiá el estado de un lote permitido: debe verse el estado actualizado y la marca `Sin enviar`.
5. Desactivá modo avión y esperá la sincronización automática; la marca debe desaparecer y el estado quedar actualizado.

## Diagnóstico rápido

- **Build release reclama URL:** verificá que exista `.env.release`, contenga `VITE_API_URL=https://.../api` y no tenga un dominio de ejemplo.
- **APK sin firma:** verificá `android/keystore.properties`, la ruta a `.jks`, alias y contraseñas. No generes otro keystore para reemplazar el original.
- **La app no conecta:** revisá HTTPS, `VITE_API_URL`, logs de MonsterASP, conexión MySQL y variables `Jwt__*`; luego repetí build release y `cap sync`.
- **No entra el administrador:** revisá que `BootstrapAdmin__*` estuviera configurado al primer arranque y consultá los logs. Nunca pegues contraseñas ni cadenas de conexión en un mensaje de soporte.
- **No sincroniza el estado:** confirmá que el usuario sea admin, que la transición esté permitida por la API y que el teléfono tenga conexión real.

Al pedir ayuda, incluí el comando, el error completo y el resultado esperado, ocultando contraseñas, tokens, cadenas de conexión y datos privados.
