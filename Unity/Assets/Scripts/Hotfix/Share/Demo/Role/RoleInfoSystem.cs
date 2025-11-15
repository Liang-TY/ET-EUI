namespace ET
{
    [EntitySystemOf(typeof(RoleInfo))]
    [FriendOfAttribute(typeof(ET.RoleInfo))]
    public static partial class RoleInfoSystem
    {
        [EntitySystem]
        private static void Awake(this ET.RoleInfo self)
        {

        }

        public static void FromMessage(this RoleInfo self, RoleInfoProto proto)
        {
            self.Name = proto.Name;
            self.State = proto.State;
            self.Account = proto.Account;
            self.CreateTime = proto.CreateTime;
            self.ServerId = proto.ServerId;
            self.LastLoginTime = proto.LastLoginTime;
        }
        public static RoleInfoProto ToMessage(this RoleInfo self)
        {
            RoleInfoProto proto = RoleInfoProto.Create();
            proto.Id = self.Id;
            proto.Name = self.Name;
            proto.State = self.State;
            proto.Account = self.Account;
            proto.CreateTime = self.CreateTime;
            proto.ServerId = self.ServerId;
            proto.LastLoginTime = self.LastLoginTime;
            return proto;
            
        }
    }
}

