namespace ET
{
    public enum RoleInfoState
    {
        Normal = 0,
        Freeze = 1,
    }
    [ChildOf]
    public class RoleInfo:Entity,IAwake
    {
        public string Name;
        public int ServerId;
        public int State;
        public string Account;
        public int RoleId;
        public long LastLoginTime;
        public long CreateTime;
    }
}

