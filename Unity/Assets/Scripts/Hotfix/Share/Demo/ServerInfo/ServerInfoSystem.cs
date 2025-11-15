namespace ET
{
    [EntitySystemOf(typeof(ServerInfo))]
    [FriendOfAttribute(typeof(ET.ServerInfo))]
    public static partial class ServerInfoSystem
    {
        [EntitySystem]
        private static void Awake(this ServerInfo self)
        {
            
        }
        
        public static void FromMessage(this ServerInfo self, ServerInfoProto proto)
        {
            self.Status = proto.Status;
            self.ServerName = proto.ServerName;
        }

        public static ServerInfoProto ToMessage(this ServerInfo self)
        {
            ServerInfoProto proto = ServerInfoProto.Create();
            proto.Id = (int)self.Id;
            proto.ServerName = self.ServerName;
            proto.Status = self.Status;

            return proto;
        }
    }
}

