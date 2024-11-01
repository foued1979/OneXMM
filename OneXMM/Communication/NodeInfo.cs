using System;

namespace OneXMM.Communication
{
    public struct NodeInfo
    {
        public Nodes Node { get; }
        public string ApiKey { get; }

        public NodeInfo(Nodes node, string apiKey)
        {  
            Node = node;
            ApiKey = apiKey; 
        }
    }
}
