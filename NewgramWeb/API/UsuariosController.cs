using NewgramWeb.Models;
using NewgramWeb.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Web.Http;

namespace NewgramWeb.API
{
    public class UsuariosController : ApiController
    {

        private DB db = new DB();

        //Login
        [Route("api/usuario/login")]
        [HttpPost]
        public HttpResponseMessage Login(dynamic DadosLogin)
        {
            String Email = DadosLogin.Email.ToString();
            String Senha = Utilitarios.HashPassword(DadosLogin.Senha.ToString());

            Usuario usuario = db.Usuarios.FirstOrDefault(uu => uu.Email == Email && uu.Senha == Senha && uu.Bloqueado == false);
            
            if (usuario != null)
            {
                usuario.Meu = true;

                AccessToken AT = new AccessToken();
                AT.UsuarioId = usuario.Id;
                AT.Token = Guid.NewGuid().ToString();
                AT.Data = DateTime.Now;
                
                db.AccessTokens.Add(AT);
                db.SaveChanges();

                HttpResponseMessage Response = Request.CreateResponse(HttpStatusCode.OK, usuario);
                Response.Headers.Add("token", AT.Token);

                return Response;
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        
        //Logoff
        [APIAutorizacao]
        [Route("api/usuario/logoff")]
        [HttpPost]
        public HttpResponseMessage LogOff()
        {
            AccessToken AT = Util.Utilitarios.GetToken(Request);

            if (AT != null)
            {

                db.Entry(AT).State = EntityState.Deleted;
                db.SaveChanges();

                HttpResponseMessage Response = Request.CreateResponse(HttpStatusCode.OK);
                Response.Headers.Add("token", "");

                return Response;
            }
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        //Cadastro Usuário
        [Route("api/usuario")]
        [HttpPost]
        public HttpResponseMessage Cadastro(dynamic DadosCadastro)
        {
            String Email = DadosCadastro.Email.ToString();

            Usuario usuario = db.Usuarios.FirstOrDefault(uu => uu.Email == Email);
            if (usuario != null)
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, "E-mail já esta sendo usado!");

            usuario = new Usuario();

            usuario.Nome = DadosCadastro.Nome.ToString();
            usuario.Email = Email;
            usuario.Senha = Utilitarios.HashPassword(DadosCadastro.Senha.ToString());
            usuario.DataCadastro = DateTime.Now;
            usuario.FotoDataAtualizacao = DateTime.Parse("2015-01-01 00:00:00.000"); 

            db.Usuarios.Add(usuario);
            db.SaveChanges();

            AccessToken AT = new AccessToken();
            AT.UsuarioId = usuario.Id;
            AT.Data = DateTime.Now;
            AT.Token = Guid.NewGuid().ToString();

            db.AccessTokens.Add(AT);
            db.SaveChanges();

            HttpResponseMessage Response = Request.CreateResponse(HttpStatusCode.OK, usuario);
            Response.Headers.Add("token", AT.Token);

            return Response;
        }

        //Alterar dados Usuario
        [APIAutorizacao]
        [Route("api/usuario")]
        [HttpPut]
        public IHttpActionResult AlteraDados([FromBody]dynamic usuario)
        {
            int IdUsuario = Utilitarios.GetTokenUsuarioLogado(Request);
            Usuario UsuarioLogado = db.Usuarios.Find(IdUsuario);

            UsuarioLogado.Nome = usuario.Nome.ToString();
            UsuarioLogado.Email = usuario.Email.ToString();
            if (!String.IsNullOrEmpty(usuario.Senha.ToString()))
                UsuarioLogado.Senha = Utilitarios.HashPassword(usuario.Senha.ToString());

            db.SaveChanges();

            return Ok(UsuarioLogado);
        }

        //Get Usuario
        [APIAutorizacao]
        [Route("api/usuario/{UsuarioID}")]
        [HttpGet]
        public HttpResponseMessage DadosUsuario(int UsuarioID)
        {
            int IdUsuarioLogado = Utilitarios.GetTokenUsuarioLogado(Request);

            Usuario Retorno = db.Usuarios.Find(UsuarioID);

            Retorno.Meu = UsuarioID == IdUsuarioLogado;
            if (!Retorno.Meu)
                Retorno.Sigo = db.Seguidores.Any(ss => ss.SeguidoID == Retorno.Id && ss.SeguidorID == IdUsuarioLogado);

            return Request.CreateResponse(HttpStatusCode.OK, Retorno);
        }

        //Alterar foto Usuario
        [APIAutorizacao]
        [Route("api/usuario/foto")]
        [HttpPut]
        public IHttpActionResult AlteraFoto([FromBody]String FotoBase64)
        {

            int IdUsuario = Utilitarios.GetTokenUsuarioLogado(Request);
            Usuario UsuarioLogado = db.Usuarios.Find(IdUsuario);

            try
            {
                UsuarioLogado.Foto = FotoBase64.ToByteArray();
                UsuarioLogado.FotoDataAtualizacao = DateTime.Now;
                db.SaveChanges();

                return Ok(UsuarioLogado.URLFoto);
            }
            catch (Exception Ex)
            {
                return InternalServerError(Ex);
            }
        }

        //Get Foto
        [Route("api/usuario/{UsuarioID}/foto")]
        [HttpGet]
        public HttpResponseMessage Foto(int UsuarioID)
        {
            Usuario usuario = db.Usuarios.Find(UsuarioID);

            if (usuario != null && usuario.Foto != null)
            {
                HttpResponseMessage Result = new HttpResponseMessage(HttpStatusCode.OK);
                Result.Content = new StreamContent(new MemoryStream(usuario.Foto));
                Result.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Image.Jpeg);

                return Result;
            }

            return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
        }

        //Adiciona token de notificação
        [APIAutorizacao]
        [Route("api/usuario/add_token_push")]
        [HttpPut]
        public HttpResponseMessage AddToken([FromBody]UsuarioToken UT)
        {
            int UsuarioId = Utilitarios.GetTokenUsuarioLogado(Request);

            Usuario UsuarioLogado = db.Usuarios.Find(UsuarioId);

            if (UsuarioLogado != null)
            {
                UT.UsuarioID = UsuarioLogado.Id;
                UT.Data = DateTime.Now;

                db.UsuarioTokens.Add(UT);
                db.SaveChanges();

                HttpResponseMessage Response = Request.CreateResponse(HttpStatusCode.OK, UT);
                return Response;
            }
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);

        }

        //Cancela Conta
        [APIAutorizacao]
        [Route("api/usuario")]
        [HttpDelete]
        public IHttpActionResult ApagaUsuario()
        {
            int UsuarioId = Utilitarios.GetTokenUsuarioLogado(Request);

            Usuario UsuarioLogado = db.Usuarios.Find(UsuarioId);

            if (UsuarioLogado == null)
                return NotFound();

            db.Usuarios.Remove(UsuarioLogado);
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        //Seguir
        [APIAutorizacao]
        [Route("api/usuario/seguir/{SeguidoID}")]
        [HttpPut]
        public IHttpActionResult Seguir(int SeguidoID)
        {
            int UsuarioId = Utilitarios.GetTokenUsuarioLogado(Request);

            Seguidor seguidor = db.Seguidores.Find(UsuarioId,SeguidoID);

            if (seguidor == null)
            {
                seguidor = new Seguidor();
                seguidor.SeguidorID = UsuarioId;
                seguidor.SeguidoID = SeguidoID;

                db.Seguidores.Add(seguidor);

                Usuario UsuarioLogado = db.Usuarios.Find(UsuarioId);
                UsuarioLogado.QuantidadeSeguindo++;

                Usuario UsuarioSeguido = db.Usuarios.Find(SeguidoID);
                UsuarioSeguido.QuantidadeSeguidores++;

                db.SaveChanges();

            }

            return Ok();
        }

        //Deixar de seguir
        [APIAutorizacao]
        [Route("api/usuario/deixar-seguir/{SeguidoID}")]
        [HttpPut]
        public IHttpActionResult DeixarSeguir(int SeguidoID)
        {

            int UsuarioId = Utilitarios.GetTokenUsuarioLogado(Request);

            Seguidor seguidor = db.Seguidores.Find(UsuarioId, SeguidoID);

            if (seguidor != null)
            {
                Usuario UsuarioLogado = db.Usuarios.Find(seguidor.SeguidorID);
                UsuarioLogado.QuantidadeSeguindo--;

                Usuario UsuarioSeguido = db.Usuarios.Find(seguidor.SeguidoID);
                UsuarioSeguido.QuantidadeSeguidores--;

                db.Seguidores.Remove(seguidor);
                db.SaveChanges();

            }

            return Ok();
        }

        //Lista Seguidores
        [APIAutorizacao]
        [Route("api/usuario/{UsuarioID}/seguidores")]
        [HttpGet]
        public IHttpActionResult ListaSeguidores(int UsuarioID)
        {
            List<int> SeguidoresIDs = db.Seguidores.Where(ss => ss.SeguidoID == UsuarioID).Select(ss => ss.SeguidorID).ToList();

            List<Usuario> Seguidores = db.Usuarios.Where(uu => SeguidoresIDs.Contains(uu.Id)).ToList();

            int UsuarioLogado = Utilitarios.GetTokenUsuarioLogado(Request);

            foreach (var U in Seguidores)
            {
                U.Sigo = db.Seguidores.Any(ss => ss.SeguidorID == UsuarioLogado && ss.SeguidoID == U.Id);
                U.Meu = U.Id == UsuarioLogado;
            }
            

            return Ok(Seguidores);
        }

        //Lista Seguidos
        [APIAutorizacao]
        [Route("api/usuario/{UsuarioID}/seguidos")]
        [HttpGet]
        public IHttpActionResult ListaSeguidos(int UsuarioID)
        {
            List<int> SeguidosIDs = db.Seguidores.Where(ss => ss.SeguidorID == UsuarioID).Select(ss => ss.SeguidoID).ToList();

            List<Usuario> Seguidos = db.Usuarios.Where(uu => SeguidosIDs.Contains(uu.Id)).ToList();

            int UsuarioLogado = Utilitarios.GetTokenUsuarioLogado(Request);

            foreach (var U in Seguidos)
            {
                U.Sigo = db.Seguidores.Any(ss => ss.SeguidorID == UsuarioLogado && ss.SeguidoID == U.Id);
                U.Meu = U.Id == UsuarioLogado;
            }

            return Ok(Seguidos);
        }

        //Pesquisa
        [APIAutorizacao]
        [Route("api/usuario/pesquisa")]
        [HttpGet]
        public IHttpActionResult PesquisaIUsuarios(string termo)
        {
            List<Usuario> ResultadoBusca = db.Usuarios.Where(uu => uu.Nome.ToLower().Contains(termo.ToLower())).ToList();

            int UsuarioLogado = Utilitarios.GetTokenUsuarioLogado(Request);

            foreach (var U in ResultadoBusca)
                U.Sigo = db.Seguidores.Any(ss => ss.SeguidorID == UsuarioLogado && ss.SeguidoID == U.Id);

            return Ok(ResultadoBusca);
        }


    }
}
