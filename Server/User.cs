﻿using DbLibrary;


namespace Server
{
    public class User
    {
        public string userName { get; set; }
        public DbMethods dbConnection { get; set; }
        public Redis redis { get; set; }
        public bool logged { get; set; }

        public int activeConversation { get; set; }

        public int userId { get; set; }

        public User()
        {
            userName = "";
            dbConnection = new DbMethods();
            redis = new Redis();
            activeConversation = -1;
            logged = false;
        }

    }
}
