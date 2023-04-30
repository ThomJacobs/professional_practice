using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public struct RelayJoinData
{
    public string joinCode;
    public string ipv4Address;
    public ushort port;
    public Guid allocationID;
    public byte[] allocationIDBtytes;
    public byte[] connectionData;
    public byte[] hostConnectionData;
    public byte[] key;
}
