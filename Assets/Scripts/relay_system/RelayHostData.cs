using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace Jacobs.Core
{
    public struct RelayHostData
    {
        //Attributes:
        public string m_joinCode;
        public string m_ipv4Address;
        public ushort m_port;
        public Guid m_allocationID;
        public byte[] m_allocationIDBytes;
        public byte[] m_connectionData;
        public byte[] m_key;

        //Constructors:
        public RelayHostData(Allocation p_allocation, string p_joinCode = "")
        {
            m_key = p_allocation.Key;
            m_port = (ushort)p_allocation.RelayServer.Port;
            m_allocationID = p_allocation.AllocationId;
            m_allocationIDBytes = p_allocation.AllocationIdBytes;
            m_ipv4Address = p_allocation.RelayServer.IpV4;
            m_connectionData = p_allocation.ConnectionData;
            m_joinCode = p_joinCode;
        }
    }
}