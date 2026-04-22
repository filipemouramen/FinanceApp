using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Application.DTOs.Categorias
{
    public class CategoriaResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Icone { get; set; } = string.Empty;
        public string Cor { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public bool Padrao { get; set; }
        public int TotalTransacoes { get; set; }
    }

    public class CriarCategoriaRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(80, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Icone { get; set; } = string.Empty;

        [StringLength(7)]
        public string Cor { get; set; } = "#6C63FF";

        [Required]
        public string Tipo { get; set; } = "DESPESA";
    }
    public class AtualizarCategoriaRequest
    {
        [StringLength(80, MinimumLength = 2)]
        public string? Nome { get; set; }

        [StringLength(50)]
        public string? Icone { get; set; }

        [StringLength(7)]
        public string? Cor { get; set; }
    }

}