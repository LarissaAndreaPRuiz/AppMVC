using AppMVC.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Linq;

namespace AppMVC.Controllers
{
    public class PessoasController : Controller
    {
        private readonly IDbConnection _db;
        private readonly IConfiguration _configuration;

        public PessoasController(IConfiguration configuration)
        {
            _configuration = configuration;
            _db = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        // Index
        public ActionResult Index()
        {
            var pessoas = _db.Query<Pessoa>("SELECT * FROM Pessoas").ToList();
            if (pessoas == null || !pessoas.Any())
            {
                // Opcional: Log ou mensagem se não houver pessoas
                return View("NoPeople");  // Caso você queira uma view alternativa
            }
            return View(pessoas);
        }

        // Cadastro
        public ActionResult Cadastro()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Cadastro(Pessoa pessoa)
        {
            var query = "INSERT INTO Pessoas (Nome, Telefone, CPF) VALUES (@Nome, @Telefone, @CPF); SELECT CAST(SCOPE_IDENTITY() AS INT);";
            var pessoaId = _db.Query<int>(query, pessoa).FirstOrDefault();  // Recuperando o Id gerado

            // Atualizando o Id da pessoa com o valor gerado
            pessoa.Id = pessoaId;

            // Redireciona para a tela inicial (Index) com o Id da nova pessoa
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Proteção contra CSRF
        public IActionResult Editar(Pessoa pessoa)
        {
            if (!ModelState.IsValid)
            {
                return View(pessoa);
            }

            // Código para atualizar os dados no banco
            var query = "UPDATE Pessoas SET Nome = @Nome, Telefone = @Telefone, CPF = @CPF WHERE Id = @Id";
            var rowsAffected = _db.Execute(query, pessoa);

            if (rowsAffected == 0)
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }



        [HttpGet]
        public IActionResult Editar(int id)
        {
            var pessoa = _db.Query<Pessoa>("SELECT * FROM Pessoas WHERE Id = @Id", new { Id = id }).FirstOrDefault();

            if (pessoa == null)
            {
                return NotFound();
            }

            var enderecos = _db.Query<Endereco>("SELECT * FROM Enderecos WHERE PessoaId = @Id", new { Id = id }).ToList();
            ViewBag.Enderecos = enderecos;

            return View(pessoa);
        }

        [HttpPost] // Este método será chamado com uma requisição POST
        [ValidateAntiForgeryToken] // Proteção contra CSRF
        public IActionResult Excluir(int id)
        {
            // Primeiro, verificamos se a pessoa existe no banco de dados
            var pessoa = _db.Query<Pessoa>("SELECT * FROM Pessoas WHERE Id = @Id", new { Id = id }).FirstOrDefault();

            if (pessoa == null)
            {
                return NotFound(); // Se não encontrar a pessoa, retorna erro 404
            }

            // Se a pessoa for encontrada, fazemos a exclusão no banco
            var query = "DELETE FROM Pessoas WHERE Id = @Id";
            var rowsAffected = _db.Execute(query, new { Id = id });

            if (rowsAffected == 0)
            {
                return NotFound(); // Se a exclusão não afetar nenhuma linha, algo deu errado
            }

            // Após a exclusão, redireciona de volta para a página de listagem (Index)
            return RedirectToAction("Index");
        }



    }
}
