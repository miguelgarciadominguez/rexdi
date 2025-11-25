# Configuración de User Secrets para la Cadena de Conexión

Este proyecto utiliza User Secrets de .NET para almacenar de forma segura la cadena de conexión a la base de datos durante el desarrollo.

## Configurar la Cadena de Conexión

Para configurar la cadena de conexión en tu entorno de desarrollo, ejecuta el siguiente comando desde la carpeta del proyecto DRAPI:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=tu_servidor;Database=tu_base_de_datos;User Id=tu_usuario;Password=tu_password;TrustServerCertificate=true;"
```

## Ejemplo de Cadenas de Conexión

### SQL Server con Windows Authentication
```
Server=localhost;Database=MiBaseDatos;Trusted_Connection=true;TrustServerCertificate=true;
```

### SQL Server con SQL Authentication
```
Server=localhost;Database=MiBaseDatos;User Id=usuario;Password=password;TrustServerCertificate=true;
```

### SQL Server Express LocalDB
```
Server=(localdb)\\mssqllocaldb;Database=MiBaseDatos;Trusted_Connection=true;
```

## Verificar Configuración

Para ver los secretos configurados:
```bash
dotnet user-secrets list
```

Para eliminar un secreto:
```bash
dotnet user-secrets remove "ConnectionStrings:DefaultConnection"
```

## Inyección de Dependencias Configurada

El proyecto ya está configurado con:
- `IDBHelper` - Interface para el helper de base de datos
- `DBHelper` - Implementación que usa `IConfiguration` para obtener la cadena de conexión
- `ICFDI` - Interface para el servicio de CFDI
- `DRCFDI` - Implementación del servicio de CFDI que usa `IDBHelper`

Estos servicios están registrados en `Program.cs` y pueden ser inyectados en cualquier controlador o servicio.