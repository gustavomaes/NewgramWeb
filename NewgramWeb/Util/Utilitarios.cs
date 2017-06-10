using NewgramWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace NewgramWeb.Util
{
    public class Utilitarios
    {

        public static int quantidadeDenunciaMaxima() { return 3; }

        public static string URLBase { get { return System.Configuration.ConfigurationManager.AppSettings["URLBase"]; } }

        public static string GET_URL(String URLComplemento)
        {

            String URL = URLBase + (!URLBase.EndsWith("/") ? "/" : "");

            if (URLComplemento.StartsWith("/"))
                URLComplemento = URLComplemento.Remove(0, 1);

            URL = URL + URLComplemento;

            return URL;

        }

        public static string HashPassword(string Password)
        {
            StringBuilder sb = new StringBuilder();

            foreach (byte b in MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Password)))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public static AccessToken GetToken(HttpRequestMessage Request)
        {
            if (Request.Headers.Any(hh => hh.Key == "token"))
            {
                String Token = Request.Headers.First(hh => hh.Key == "token").Value.First();

                AccessToken AT = null;
                using (DB Banco = new DB())
                    AT = Banco.AccessTokens.Where(hh => hh.Token == Token).FirstOrDefault();

                if (AT != null)
                    return AT;
            }

            return null;
        }

        public static int GetTokenUsuarioLogado(HttpRequestMessage Request)
        {
            AccessToken AT = GetToken(Request);

            if (AT != null)
                return AT.UsuarioId;
            
            return -1;
        }

        public static int GetLarguraTela(HttpRequestMessage Request)
        {

            if (Request.Headers.Any(hh => hh.Key.ToLower() == "larguratela"))
            {
                String Token = Request.Headers.First(hh => hh.Key.ToLower() == "larguratela").Value.First();

                int RET = -1;

                int.TryParse(Token, out RET);

                return RET;
            }

            return -1;
        }


    }
}