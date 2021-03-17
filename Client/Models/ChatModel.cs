﻿using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Client.Models
{
    public class ChatModel : BaseModel
    {

        private readonly string username;
        private readonly byte[] credentialsHash;
        private readonly string appLocalDataFolderName;
        private readonly string appPath;
        private readonly string userPath;
        private readonly string invitationKeysFilePath;

        private string invitationUsername;
        private List<Friend> friendsList;

        public string Username { get { return username; } }
        public string InvitationUsername { get { return invitationUsername; } set { invitationUsername = value; } }
        public List<Friend> FriendsList { get { return friendsList; } set { friendsList = value; } }

        public ChatModel(ServerConnection connection, string username, byte[] credentialsHash, string appLocalDataFolderName) : base(connection) {
            this.username = username;
            this.credentialsHash = credentialsHash;
            this.appLocalDataFolderName = appLocalDataFolderName;
            this.appPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appLocalDataFolderName);
            this.userPath = Path.Combine(appPath, username);
            this.invitationKeysFilePath = Path.Combine(userPath, "invitation_keys.json");
            Directory.CreateDirectory(userPath);

        }

        public bool CheckUsernameText(string username) {
            if (!String.IsNullOrEmpty(username) && Regex.Match(username, @"^[\w]{3,}$").Success) return true;
            else return false;
        }

        public string GetFriendsJSON() {
            var response = ServerCommands.GetFriendsCommand(ref connection);
            if (response.error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(response.error));
            return response.friendsJSON;
        }

        public void LogoutUser() {
            int error = ServerCommands.LogoutCommand(ref connection);
            if (error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(error));
        }

        public bool CheckUserExist(string username) {
            int error = ServerCommands.CheckUsernameExistCommand(ref connection, username);
            if (error == (int)ErrorCodes.NO_ERROR) return false;
            else if (error == (int)ErrorCodes.USER_ALREADY_EXISTS) return true;
            else throw new Exception(GetErrorCodeName(error));
        }

        public (string g, string p, string invitationID) SendInvitation(string username) {
            var response = ServerCommands.SendInvitationCommand(ref connection, username);
            if (response.error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(response.error));
            return (response.g, response.p, response.invitationID);
        }

        public void SendPublicDHKey(string invitationID, string publicDHKey) {
            int error = ServerCommands.SendPublicDHKeyCommand(ref connection, invitationID, publicDHKey);
            if (error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(error));
        }

        public (string publicDHKey, byte[] privateDHKey) GenerateDHKeys(string g, string p) {
            var parameters = new DHParameters(new Org.BouncyCastle.Math.BigInteger(p), new Org.BouncyCastle.Math.BigInteger(g));
            var keys = Security.GenerateKeys(parameters);
            return (Security.GetPublicKey(keys), Security.GetPrivateKeyBytes(keys));
        }

        public byte[] AESEncrypt(byte[] bytesToEncrypt, byte[] encryptingKey, byte[] iv) {
            //TODO
            return bytesToEncrypt;
        }

        public byte[] AESDecrypt(byte[] bytesToDecrypt, byte[] decryptingKey, byte[] iv) {
            //TODO
            return bytesToDecrypt;
        }

        public void SaveEncryptedPrivateDHKey(string invitationID, byte[] encryptedPrivateDHKey) {
            Dictionary<string, byte[]> invitationKeys;
            if (!File.Exists(invitationKeysFilePath)) invitationKeys = new Dictionary<string, byte[]>();
            else {
                string invitationKeysFileContent = File.ReadAllText(invitationKeysFilePath);
                invitationKeys = JsonConvert.DeserializeObject<Dictionary<string, byte[]>>(invitationKeysFileContent);
            }
            invitationKeys.Add(invitationID, encryptedPrivateDHKey);
            string invitationKeysFileContentToSave = JsonConvert.SerializeObject(invitationKeys);
            File.WriteAllText(invitationKeysFilePath, invitationKeysFileContentToSave);
        }
    }
}
