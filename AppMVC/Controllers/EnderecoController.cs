using AppMVC.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;  // Adicionando esse namespace para usar IConfiguration
using System.Data;
using System.Reflection;

namespace AppMVC.Controllers
{
    public class EnderecosController : Controller
    {
        private readonly IDbConnection _db;
        private readonly IConfiguration _configuration;

        // Injeção de dependência de IConfiguration no construtor
        public EnderecosController(IConfiguration configuration)
        {
            _configuration = configuration;
            _db = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        [HttpPost]
        public ActionResult Cadastro(int pessoaId, Endereco endereco)
        {
            if (!ModelState.IsValid)
            {
                return View("EnderecoCadastro", endereco); // Passando a view correta
            }

            // Verificando se a pessoa existe na tabela Pessoas
            var pessoa = _db.QueryFirstOrDefault<Pessoa>("SELECT * FROM Pessoas WHERE Id = @Id", new { Id = pessoaId });

            if (pessoa == null)
            {
                TempData["ErrorMessage"] = "Pessoa não encontrada!";
                return RedirectToAction("Index", "Pessoas");
            }

            // Associando a pessoa ao endereço
            endereco.PessoaId = pessoaId;

            try
            {
                // Inserindo o endereço no banco de dados
                var query = "INSERT INTO Enderecos (PessoaId, Endereco, CEP, Cidade, Estado) VALUES (@PessoaId, @EnderecoPessoa, @CEP, @Cidade, @Estado)";

                // Certificando-se de que os parâmetros estão corretos
                _db.Execute(query, new
                {
                    PessoaId = endereco.PessoaId,
                    EnderecoPessoa = endereco.EnderecoPessoa,
                    CEP = endereco.CEP,
                    Cidade = endereco.Cidade,
                    Estado = endereco.Estado
                });

                // Redireciona para a tela de edição da pessoa após salvar o endereço
                return RedirectToAction("Editar", "Pessoas", new { id = pessoaId });
            }
            catch (Exception ex)
            {
                // Logando o erro
                TempData["ErrorMessage"] = $"Erro ao salvar o endereço: {ex.Message}";
                return RedirectToAction("Editar", "Pessoas", new { id = pessoaId });
            }
        }


        [HttpGet]
        public IActionResult Cadastro(int id)
        {
            var pessoa = _db.QueryFirstOrDefault<Pessoa>("SELECT * FROM Pessoas WHERE Id = @Id", new { Id = id });

            if (pessoa == null)
            {
                return NotFound();
            }

            var endereco = new Endereco { PessoaId = id };

            // Alterando para renderizar a view EnderecoCadastro.cshtml
            return View("CadastroEndereco", endereco);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExcluirEndereco(int enderecoId, int pessoaId)
        {
            // Verifica se o endereço existe
            var endereco = _db.Query<Endereco>("SELECT * FROM Enderecos WHERE Id = @Id", new { Id = enderecoId }).FirstOrDefault();

            if (endereco == null)
            {
                return NotFound();
            }

            // Exclui o endereço
            var query = "DELETE FROM Enderecos WHERE Id = @Id";
            var rowsAffected = _db.Execute(query, new { Id = enderecoId });

            if (rowsAffected == 0)
            {
                return NotFound();
            }

            // Após a exclusão, recarrega a pessoa e seus endereços
            var pessoa = _db.Query<Pessoa>("SELECT * FROM Pessoas WHERE Id = @Id", new { Id = pessoaId }).FirstOrDefault();
            if (pessoa == null)
            {
                return NotFound();
            }

            // Carrega os endereços da pessoa
            var enderecos = _db.Query<Endereco>("SELECT * FROM Enderecos WHERE PessoaId = @Id", new { Id = pessoaId }).ToList();

            // Passa a pessoa e os endereços para a view
            // Passar tanto a pessoa quanto a lista de endereços para a ViewBag
            ViewBag.Enderecos = enderecos;
            Console.WriteLine($"Endereços encontrados: {enderecos.Count}");
            return View("~/Views/Pessoas/Editar.cshtml", pessoa); // Retorna a mesma view, agora com a pessoa e seus endereços atualizados
        }
        // GET: Enderecos/Editar/{id}
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var endereco = _db.Query<Endereco>("SELECT * FROM Enderecos WHERE Id = @Id", new { Id = id }).FirstOrDefault();

            if (endereco == null)
            {
                return NotFound(); // Retorna um erro 404 se não encontrar o endereço
            }

            return View(endereco); // Retorna a view com os dados do endereço
        }

        // POST: Enderecos/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Endereco endereco)
        {
            if (!ModelState.IsValid)
            {
                return View(endereco); // Se o modelo não for válido, retorna a view com os dados para correção
            }

            // Atualiza os dados no banco
            var query = "UPDATE Enderecos SET Endereco = @EnderecoPessoa, CEP = @CEP, Cidade = @Cidade, Estado = @Estado WHERE Id = @Id";
            var rowsAffected = _db.Execute(query, endereco);

            if (rowsAffected == 0)
            {
                return NotFound(); // Se nenhuma linha for afetada, retorna "NotFound"
            }

            // Após a edição, redireciona para a lista de endereços (geralmente a ação Index)
            return RedirectToAction("Editar", "Pessoas", new { id = endereco.PessoaId  }); // Redireciona para a ação Index, do controlador Enderecos
        }


    }

}

