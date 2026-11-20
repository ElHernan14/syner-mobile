-- U10: agregar coordenadas opcionales a los proveedores.
-- Ejecutar una sola vez en la base publicada.
ALTER TABLE proveedores
    ADD COLUMN Latitud DECIMAL(10,7) NULL,
    ADD COLUMN Longitud DECIMAL(10,7) NULL;

-- U12: imagen pública opcional del lote.
ALTER TABLE lotes
    ADD COLUMN ImagenAvatar VARCHAR(500) NULL;

-- Revisar los IDs reales antes de preparar los UPDATE con las ubicaciones.
SELECT Id, Nombre, Latitud, Longitud
FROM proveedores
ORDER BY Nombre;

-- Una vez que se conozcan las coordenadas reales, usar por proveedor:
-- UPDATE proveedores SET Latitud = <LATITUD>, Longitud = <LONGITUD> WHERE Id = 2; -- Hogar Plus Mayorista
-- UPDATE proveedores SET Latitud = <LATITUD>, Longitud = <LONGITUD> WHERE Id = 3; -- Moda Urbana Mayorista
-- UPDATE proveedores SET Latitud = <LATITUD>, Longitud = <LONGITUD> WHERE Id = 1; -- TecnoSur Distribuciones
