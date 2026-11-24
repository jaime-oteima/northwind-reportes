# Northwind para el taller de reportes

Se requiere MySQL 8.0.16 o posterior y una base de práctica propia. Si la base Northwind reducida del curso ya está disponible, no es necesario importarla nuevamente.

Para una instalación nueva, el archivo `northwind-reducida-mysql.sql` deberá abrirse en el cliente SQL y ejecutarse completo una sola vez. La importación deberá realizarse en una instancia de práctica que no contenga otra base Northwind con trabajos. El script crea `northwind` y ocho tablas; no elimina tablas. Ante un error, se deberá detener el procedimiento y revisar el mensaje antes de continuar. Una importación parcial puede dejar tablas creadas.

La importación deberá comprobarse mediante `SELECT COUNT(*) FROM northwind.Products;`. La muestra original contiene 77 productos. La ejecución del reporte requiere permisos de lectura sobre `Products` y `Categories`; no requiere permisos de escritura.

La base corresponde a la adaptación educativa para MySQL del [Northwind de Microsoft](https://github.com/microsoft/sql-server-samples/blob/master/samples/databases/northwind-pubs/instnwnd.sql) utilizada en el curso, blob `ae61e5631d7f03029ec15213ce672b45ceb7e629`, consultado el 6 de noviembre de 2026. La licencia de procedencia se incluye en `LICENCIA-MICROSOFT.txt` y deberá conservarse junto con la base.
