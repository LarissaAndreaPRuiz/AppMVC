using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper; // Add this using directive

namespace AppMVC.Controllers
{
    public class EnderecoController : Controller
    {
        private readonly IDbConnection _db;
        private readonly IConfiguration _configuration;

        // Injeção de dependência de IConfiguration no construtor
        public EnderecoController(IConfiguration configuration)
        {
            _configuration = configuration;
            _db = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        public ActionResult Index()
        {
            // Aqui você deve obter a lista de Endereços usando o Dapper e a tabela "Enderecos"
            var enderecos = _db.Query<Endereco>("SELECT * FROM Enderecos").ToList(); // Use Dapper to query the database

            // Verifique no console a quantidade de endereços
            Console.WriteLine($"Total de Endereços: {enderecos.Count}");

            // Retorne a View com o modelo
            return View(enderecos); // Passe os dados para a View
        }
    }