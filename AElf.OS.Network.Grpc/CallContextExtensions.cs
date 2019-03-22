using System;
using System.Linq;
using Grpc.Core;

namespace AElf.OS.Network.Grpc
{
    public static class CallContextExtensions
    {
        public static string GetPublicKey(this ServerCallContext context)
        {
            return context.RequestHeaders
                .FirstOrDefault(entry => entry.Key == GrpcConsts.PubkeyMetadataKey)?.Value;
        }
        
        public static string GetPeerInfo(this ServerCallContext context)
        {
            return context.RequestHeaders
                .FirstOrDefault(entry => entry.Key == GrpcConsts.PeerInfoMetadataKey)?.Value;
        }
    }
}