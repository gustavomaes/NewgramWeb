using NewgramWeb.Models;
using NewgramWeb.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Web.Http;

namespace NewgramWeb.API
{
    public class PostsController : ApiController
    {

        private DB db = new DB();

        //Feed
        [APIAutorizacao]
        [Route("api/posts/feed")]
        [HttpGet]
        public IHttpActionResult Feed(int Pagina, int QuantidadePagina)
        {

            int UsuarioId = Utilitarios.GetTokenUsuarioLogado(Request);

            List<int> seguidores = db.Seguidores.Where(ss => ss.SeguidorID == UsuarioId).Select(ss => ss.SeguidoID).ToList();

            List<Post> posts = db.Posts
                .Where(pp => seguidores.Contains(pp.UsuarioId) && pp.Bloqueado == false)
                .OrderByDescending(pp => pp.Data)
                .Skip((Pagina - 1) * QuantidadePagina)
                .Take(QuantidadePagina).ToList();

            foreach (var p in posts)
            {
                p.AtualizaDadosFoto(Utilitarios.GetLarguraTela(Request));

                p.Meu = p.UsuarioId == UsuarioId;
                p.EuCurti = db.PostsInteracoes.Any(pp => pp.PostId == p.Id && pp.Tipo == PostInteracao.TipoInteracao.Curtida && pp.UsuarioId == UsuarioId);
                p.EuComentei = db.PostsInteracoes.Any(pp => pp.PostId == p.Id && pp.Tipo == PostInteracao.TipoInteracao.Comentario && pp.UsuarioId == UsuarioId);

                if (!p.Meu)
                    p.EuDenunciei = db.PostsInteracoes.Any(pp => pp.PostId == p.Id && pp.Tipo == PostInteracao.TipoInteracao.DenunciaPost&& pp.UsuarioId == UsuarioId);
            }

            return Ok(posts);
        }

        //Feed Usuário
        [APIAutorizacao]
        [Route("api/posts/feed-usuario/{UsuarioID}")]
        [HttpGet]
        public IHttpActionResult FeedUsuario(int UsuarioID)
        {
            int UsuarioId = Utilitarios.GetTokenUsuarioLogado(Request);
            
            List<Post> posts = db.Posts
                .Where(pp => pp.UsuarioId == UsuarioID && pp.Bloqueado == false)
                .OrderByDescending(pp => pp.Data).ToList();

            foreach (var p in posts)
            {
                p.AtualizaDadosFoto(Utilitarios.GetLarguraTela(Request));

                p.Meu = p.UsuarioId == UsuarioId;
                p.EuCurti = db.PostsInteracoes.Any(pp => pp.PostId == p.Id && pp.Tipo == PostInteracao.TipoInteracao.Curtida && pp.UsuarioId == UsuarioId);
                p.EuComentei = db.PostsInteracoes.Any(pp => pp.PostId == p.Id && pp.Tipo == PostInteracao.TipoInteracao.Comentario && pp.UsuarioId == UsuarioId);

                if (!p.Meu)
                    p.EuDenunciei = db.PostsInteracoes.Any(pp => pp.PostId == p.Id && pp.Tipo == PostInteracao.TipoInteracao.DenunciaPost && pp.UsuarioId == UsuarioId);
            }

            return Ok(posts);
        }

        //Feed Mapa
        [APIAutorizacao]
        [Route("api/posts/feed/mapa")]
        [HttpGet]
        public IHttpActionResult FeedMapa()
        {
            int UsuarioId = Utilitarios.GetTokenUsuarioLogado(Request);

            List<int> Seguidores = db.Seguidores.Where(ss => ss.SeguidorID == UsuarioId).Select(ss => ss.SeguidoID).ToList();

            List<Post> posts = db.Posts.Where
                (pp => Seguidores.Contains(pp.UsuarioId) 
                && pp.Bloqueado == false 
                && (pp.Latitude != 0 && pp.Longitude != 0))
                .Take(100)
                .ToList();

            foreach (var p in posts)
            {
                p.AtualizaDadosFoto(Utilitarios.GetLarguraTela(Request));

                p.Meu = p.UsuarioId == UsuarioId;
                p.EuCurti = db.PostsInteracoes.Any(pp => pp.PostId == p.Id && pp.Tipo == PostInteracao.TipoInteracao.Curtida && pp.UsuarioId == UsuarioId);
                p.EuComentei = db.PostsInteracoes.Any(pp => pp.PostId == p.Id && pp.Tipo == PostInteracao.TipoInteracao.Comentario && pp.UsuarioId == UsuarioId);

                if (!p.Meu)
                    p.EuDenunciei = db.PostsInteracoes.Any(pp => pp.PostId == p.Id && pp.Tipo == PostInteracao.TipoInteracao.DenunciaPost && pp.UsuarioId == UsuarioId);
            }

            return Ok(posts);
        }

        //Get Foto
        [APIAutorizacao]
        [Route("api/posts/{PostID}/foto/{LarguraTela}")]
        [HttpGet]
        public HttpResponseMessage Foto(int PostID, int LarguraTela)
        {

            Post post = db.Posts.Find(PostID);

            if (post != null && post.Foto != null)
            {

                post.Foto = post.Foto.RedimencionaProporcional(LarguraTela);

                HttpResponseMessage Result = new HttpResponseMessage(HttpStatusCode.OK);
                Result.Content = new StreamContent(new MemoryStream(post.Foto));
                Result.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Image.Jpeg);

                return Result;
            }

            return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
        }

        //Postar
        [APIAutorizacao]
        [Route("api/posts")]
        [HttpPost]
        public IHttpActionResult Postar(Post post)
        {

            post.Data = DateTime.Now;

            db.Posts.Add(post);

            Usuario usuario = db.Usuarios.Find(post.UsuarioId);
            usuario.QuantidadePosts++;
            db.SaveChanges();

            post.AtualizaDadosFoto(Utilitarios.GetLarguraTela(Request));

            return Ok(post);
        }

        //Excluir post
        [APIAutorizacao]
        [Route("api/posts/{id}")]
        [HttpDelete]
        public IHttpActionResult ApagaPost(int id)
        {

            Post post = db.Posts.Find(id);

            int UsuarioId = Utilitarios.GetTokenUsuarioLogado(Request);
            
            if (post == null || post.UsuarioId != UsuarioId)
            {
                return NotFound();
            }

            Usuario usuario = db.Usuarios.Find(post.UsuarioId);
            usuario.QuantidadePosts--;

            db.Posts.Remove(post);
            db.SaveChanges();
            
            return StatusCode(HttpStatusCode.NoContent);
        }

        //Curtir
        [APIAutorizacao]
        [Route("api/posts/curtir/{PostID}")]
        [HttpPut]
        public IHttpActionResult Curtir(int PostID, bool Curtir)
        {

            int UsuarioId = Utilitarios.GetTokenUsuarioLogado(Request);

            PostInteracao postInteracao = db.PostsInteracoes.FirstOrDefault(pp => pp.PostId == PostID 
                && pp.UsuarioId == UsuarioId 
                && pp.Tipo == PostInteracao.TipoInteracao.Curtida);

            Post post = db.Posts.Find(PostID);

            if (postInteracao == null)
            {

                if (Curtir)
                {

                    postInteracao = new PostInteracao();

                    postInteracao.PostId = PostID;
                    postInteracao.UsuarioId = UsuarioId;
                    postInteracao.Tipo = PostInteracao.TipoInteracao.Curtida;
                    postInteracao.Data = DateTime.Now;

                    db.PostsInteracoes.Add(postInteracao);
                    post.QuantidadeCurtidas++;
                    
                }
            }
            else
            {
                if (!Curtir)
                {

                    db.PostsInteracoes.Remove(postInteracao);
                    post.QuantidadeCurtidas--;

                }
            }

            db.SaveChanges();

            return Ok();
        }

        //Lista curtidas
        [APIAutorizacao]
        [Route("api/posts/curtidas/{PostID}")]
        [HttpGet]
        public IHttpActionResult Curtidas(int PostID)
        {

            List<PostInteracao> ListaInteracoes = db.PostsInteracoes
                .Where(pp => pp.PostId == PostID
                && pp.Tipo == PostInteracao.TipoInteracao.Curtida).ToList();

            return Ok(ListaInteracoes );
        }

        //Comentar
        [APIAutorizacao]
        [Route("api/posts/comentar/{PostID}")]
        [HttpPut]
        public IHttpActionResult Comentar(int PostID, string Texto)
        {

            PostInteracao postInteracao = new PostInteracao();
            postInteracao.PostId = PostID;
            postInteracao.UsuarioId = Utilitarios.GetTokenUsuarioLogado(Request);
            postInteracao.Tipo = PostInteracao.TipoInteracao.Comentario;
            postInteracao.Data = DateTime.Now;
            postInteracao.Texto = Texto;

            db.PostsInteracoes.Add(postInteracao);

            Post post = db.Posts.Find(PostID);
            post.QuantidadeComentarios++;

            db.SaveChanges();

            return Ok();
        }

        //Lista comentarios
        [APIAutorizacao]
        [Route("api/posts/comentarios/{PostID}")]
        [HttpGet]
        public IHttpActionResult Comentarios(int PostID)
        {
            int UsuarioId = Utilitarios.GetTokenUsuarioLogado(Request);

            List<PostInteracao> ListaInteracoes = db.PostsInteracoes
                .Where(pp => pp.PostId == PostID
                && pp.Tipo == PostInteracao.TipoInteracao.Comentario
                && pp.Bloqueado == false).ToList();

            foreach (var p in ListaInteracoes)
            {
                p.Meu = p.UsuarioId == UsuarioId;

                if (!p.Meu)
                    p.EuDenunciei = db.PostsInteracoes.Any(pp => pp.Id == p.ComentarioId && pp.Tipo == PostInteracao.TipoInteracao.DenunciaComentario && pp.UsuarioId == UsuarioId);
            }

            return Ok(ListaInteracoes);
        }

        //Denunciar
        [APIAutorizacao]
        [Route("api/posts/denunciar/{PostID}")]
        [HttpPut]
        public IHttpActionResult Denunciar(int PostID, [FromBody]String Texto)
        {

            PostInteracao postInteracao = new PostInteracao();
            postInteracao.PostId = PostID;
            postInteracao.UsuarioId = Utilitarios.GetTokenUsuarioLogado(Request);
            postInteracao.Tipo = PostInteracao.TipoInteracao.DenunciaPost;
            postInteracao.Texto = Texto;
            postInteracao.Data = DateTime.Now;

            db.PostsInteracoes.Add(postInteracao);

            Post post = db.Posts.Find(PostID);
            post.QuantidadeDenuncias++;
            post.Denunciado = true;

            if (post.QuantidadeDenuncias > Utilitarios.quantidadeDenunciaMaxima())
                post.Bloqueado = true;

            db.SaveChanges();

            return Ok();
        }

        //Denunciar Comentario
        [APIAutorizacao]
        [Route("api/posts/denunciar-comentario/{ComentarioID}")]
        [HttpPut]
        public IHttpActionResult DenunciarComentario(int ComentarioID, [FromBody]String Texto)
        {

            PostInteracao postInteracao = new PostInteracao();
            postInteracao.ComentarioId = ComentarioID;
            postInteracao.UsuarioId = Utilitarios.GetTokenUsuarioLogado(Request);
            postInteracao.Tipo = PostInteracao.TipoInteracao.DenunciaComentario;
            postInteracao.Texto = Texto;
            postInteracao.Data = DateTime.Now;

            db.PostsInteracoes.Add(postInteracao);

            PostInteracao comentario = db.PostsInteracoes.Find(ComentarioID);
            comentario.QuantidadeDenuncias++;
            comentario.Denunciado = true;

            if (comentario.QuantidadeDenuncias > Utilitarios.quantidadeDenunciaMaxima())
                comentario.Bloqueado = true;

            db.SaveChanges();

            return Ok();
        }
        
    }
}
