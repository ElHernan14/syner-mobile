using Syner.Api.Domain.Enums;

namespace Syner.Api.Domain.States;

public static class ReglasEstado
{
    public static readonly Dictionary<EstadoLote, HashSet<OperacionEstado>>
        Lotes = new()
        {
            [EstadoLote.Borrador] =
            [
                OperacionEstado.Modificacion,
                OperacionEstado.Baja
            ],

            [EstadoLote.Fondeando] =
            [
                OperacionEstado.Baja
            ],

            [EstadoLote.Completado] =
            [
                OperacionEstado.Baja
            ],

            [EstadoLote.Comprado] =
            [
                OperacionEstado.Baja
            ],

            [EstadoLote.Enviado] =
            [
                OperacionEstado.Baja
            ],

            [EstadoLote.Entregado] =
            [],

            [EstadoLote.Cancelado] =
            []
        };

    public static readonly Dictionary<EstadoPedido, HashSet<OperacionEstado>>
        Pedidos = new()
        {
            [EstadoPedido.Fondeando] =
            [
                OperacionEstado.Modificacion,
                OperacionEstado.Baja
            ],

            [EstadoPedido.Completado] =
            [
                OperacionEstado.Modificacion,
                OperacionEstado.Baja
            ],

            [EstadoPedido.Comprado] =
            [
                OperacionEstado.Modificacion,
                OperacionEstado.Baja
            ],

            [EstadoPedido.Enviado] =
            [
                OperacionEstado.Modificacion,
                OperacionEstado.Baja
            ],

            [EstadoPedido.Entregado] =
            [],

            [EstadoPedido.Cancelado] =
            []
        };

    public static readonly Dictionary<EstadoUsuario, HashSet<OperacionEstado>>
        Usuarios = new()
        {
            [EstadoUsuario.Pendiente] =
            [
                OperacionEstado.Modificacion,
                OperacionEstado.Baja
            ],

            [EstadoUsuario.Verificado] =
            [
                OperacionEstado.Modificacion,
                OperacionEstado.Baja
            ],

            [EstadoUsuario.Bloqueado] =
            [
                OperacionEstado.Modificacion
            ]
        };
}