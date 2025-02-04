using System.ComponentModel.DataAnnotations.Schema;

namespace AppMVC.Models
{
    public class Endereco
    {
        public int Id { get; set; }
        public int PessoaId { get; set; }  

        [Column("Endereco")]
        public string EnderecoPessoa { get; set; }  
        public string CEP { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
    }
}
