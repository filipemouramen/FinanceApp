using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Application.Interfaces
{
    public class Resultado<T>
    {
        public bool Sucesso { get; set; }
        public T? Dados { get; set; }
        public string? Mensagem { get; set; }
        public List<string> Erros { get; set; } = new();
        public int? StatusCode { get; set; }

        public static Resultado<T> Ok(T dados, string? mensagem = null)
            => new() { Sucesso = true, Dados = dados, Mensagem = mensagem, StatusCode = 200 };

        public static Resultado<T> Criado(T dados, string? mensagem = null)
            => new() { Sucesso = true, Dados = dados, Mensagem = mensagem, StatusCode = 201 };

        public static Resultado<T> Falha(string erro, int statusCode = 400)
            => new() { Sucesso = false, Erros = new List<string> { erro }, StatusCode = statusCode };

        public static Resultado<T> Falha(List<string> erros, int statusCode = 400)
            => new() { Sucesso = false, Erros = erros, StatusCode = statusCode };

        public static Resultado<T> NaoEncontrado(string mensagem = "Recurso não encontrado")
            => new() { Sucesso = false, Erros = new List<string> { mensagem }, StatusCode = 404 };

        public static Resultado<T> NaoAutorizado(string mensagem = "Não autorizado")
            => new() { Sucesso = false, Erros = new List<string> { mensagem }, StatusCode = 401 };
    }
}