using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace NewgramWeb.Models
{
    public class Post
    {

        [Key]
        public int Id { get; set; }

        [Index]
        public DateTime Data { get; set; }

        [Index]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario UsuarioDados { get; set; }

        public byte[] Foto { get; set; }

        [NotMapped]
        public string FotoURL { get; set; }
        [NotMapped]
        public string FotoURLPequena { get; set; }
        [NotMapped]
        public string FotoURLMiniatura { get; set; }

        public void AtualizaDadosFoto(int LarguraTela)
        {
            FotoURL = Util.Utilitarios.GET_URL("api/posts/" + Id + "/foto/" + LarguraTela.ToString("0"));
            FotoURLPequena = Util.Utilitarios.GET_URL("api/posts/" + Id + "/foto/" + (LarguraTela * 0.43).ToString("0"));
            FotoURLMiniatura = Util.Utilitarios.GET_URL("api/posts/" + Id + "/foto/" + (LarguraTela / 5).ToString("0"));
            Foto = null;
        }

        public string Descricao { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public int QuantidadeCurtidas { get; set; }
        public int QuantidadeComentarios { get; set; }
        public int QuantidadeDenuncias { get; set; }

        public bool Denunciado { get; set; }

        [Index]
        public bool Bloqueado { get; set; }

        [NotMapped]
        public bool Meu { get; set; }
        [NotMapped]
        public bool EuCurti { get; set; }
        [NotMapped]
        public bool EuComentei { get; set; }
        [NotMapped]
        public bool EuDenunciei { get; set; }

    }
}