-- Ejecutar una sola vez en la base publicada.
-- Los usuarios de SYNER usan Id BIGINT; UsuarioId conserva ese tipo para la FK.
CREATE TABLE `direcciones` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `UsuarioId` BIGINT NOT NULL,
    `Calle` VARCHAR(150) NOT NULL,
    `Numero` VARCHAR(20) NOT NULL,
    `Ciudad` VARCHAR(100) NOT NULL,
    `Provincia` VARCHAR(100) NOT NULL,
    `CodigoPostal` VARCHAR(20) NOT NULL,
    `Referencia` VARCHAR(255) NULL,
    `Latitud` DECIMAL(10,7) NOT NULL,
    `Longitud` DECIMAL(10,7) NOT NULL,
    `EsPrincipal` BOOLEAN NOT NULL,
    `FechaCreacion` DATETIME(6) NOT NULL,
    `FechaActualizacion` DATETIME(6) NOT NULL,
    `FechaEliminacion` DATETIME(6) NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_direcciones_UsuarioId_EsPrincipal_FechaEliminacion` (`UsuarioId`, `EsPrincipal`, `FechaEliminacion`),
    CONSTRAINT `FK_direcciones_usuarios_UsuarioId`
        FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`)
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

SHOW CREATE TABLE `direcciones`;
