using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace NewgramWeb.Models
{
    public class PostInteracao
    {

        [Key]
        public int Id { get; set; }

        [Index]
        public int PostId { get; set; }

        [Index]
        public int ComentarioId { get; set; }

        [Index]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario UsuarioDados { get; set; }

        public enum TipoInteracao
        {
            Curtida = 1,
            Comentario = 2,
            DenunciaPost = 3,
            DenunciaComentario = 4
        }

        [Index]
        public TipoInteracao Tipo { get; set; }

        public string Texto { get; set; }
        public DateTime Data { get; set; }
        
        //Para comentarios
        public int QuantidadeDenuncias { get; set; }
        public bool Denunciado { get; set; }
        public bool Bloqueado { get; set; }

        [NotMapped]
        public bool Meu { get; set; }
        [NotMapped]
        public bool EuDenunciei { get; set; }
    }
}