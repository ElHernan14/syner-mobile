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

-- Puntos sintéticos de demostración en San Luis; no son domicilios reales.
-- El esquema ya fue creado: ejecutar solo estos UPDATE, no volver a correr los ALTER.
UPDATE proveedores SET Latitud = -33.2948000, Longitud = -66.3425000 WHERE Id = 2; -- Hogar Plus Mayorista
UPDATE proveedores SET Latitud = -33.3054000, Longitud = -66.3258000 WHERE Id = 3; -- Moda Urbana Mayorista
UPDATE proveedores SET Latitud = -33.3142000, Longitud = -66.3461000 WHERE Id = 1; -- TecnoSur Distribuciones
