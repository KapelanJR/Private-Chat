﻿using Shared;
using System;
using System.IO;
using System.Text;

namespace Client.Models
{
    public class UserKeyInputModel : BaseModel
    {
        private readonly byte[] userKeyHash;
        private readonly byte[] userIV;
        private readonly byte[] credentialsHash;
        private readonly string username;

        private byte[] userKey;
        private string userKeyHexStringFromInput;

        public byte[] UserIV { get { return userIV; } }
        public byte[] CredentialsHash { get { return credentialsHash; } }
        public byte[] UserKey { get { return userKey; } }
        public string Username { get { return username; } }

        public string UserKeyHexStringFromInput { get { return userKeyHexStringFromInput; } set { userKeyHexStringFromInput = value; } }

        public UserKeyInputModel(ServerConnection connection, string username, byte[] userKeyHash, byte[] userIV, byte[] credentialsHash) : base(connection) {
            this.username = username;
            this.userKeyHash = userKeyHash;
            this.userIV = userIV;
            this.credentialsHash = credentialsHash;
            this.userKeyHexStringFromInput = "";
            this.userKey = null;
        }

        public bool IsUserKeyGood() {
            if (userKeyHexStringFromInput.Length != 64) return false;
            byte[] userKeyFromInput = Security.HexStringToByteArray(userKeyHexStringFromInput);
            byte[] userKeyFromInputHash = Security.CreateSHA256Hash(userKeyFromInput);
            if (Security.CompareByteArrays(userKeyFromInputHash, userKeyHash)) {
                userKey = userKeyFromInput;
                return true;
            }
            else return false;
        }

        public void SaveEncryptedUserKey() {
            //Przypisz do zmiennej ścieżke do folderu użytkownika
            string userPath = Path.Combine(appLocalDataFolderPath, username);
            //Przypisz do zmiennej ścieżke do pliku w którym ma być umieszczony zaszyfrowany klucz użytkownika
            string encryptedUserKeyFilePath = Path.Combine(userPath, encryptedUserKeyFileName);
            //Zaszyfruj klucz użytkownika
            byte[] encryptedUserKey = Security.AESEncrypt(userKey, credentialsHash, userIV);
            //Zamień zaszyfrowane bajty klucza użytkownika na tekstowy zapis szesnastkowy
            string encryptedUserKeyHexString = Security.ByteArrayToHexString(encryptedUserKey);
            //Jeśli folder użytkownika nie istnieje, utwórz go
            Directory.CreateDirectory(userPath);
            //Zapisz zawartość zmiennej "encryptedUserKeyHexString" do pliku o ścieżce "encryptedUserKeyFilePath"
            File.WriteAllText(encryptedUserKeyFilePath, encryptedUserKeyHexString);
        }
    }
}
