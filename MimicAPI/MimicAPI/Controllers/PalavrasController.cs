using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimicAPI.Database;
using MimicAPI.Models;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace MimicAPI.Controllers
{
    [Route("api/palavras")]


    public class PalavrasController : ControllerBase
    {
        private readonly MimicContext _banco;

        public PalavrasController(MimicContext banco)
        {
            _banco = banco;
        }

        //APP -- /api/palavras?data=2019-05-30
        [Route("")]
        [HttpGet]
        public ActionResult ObterTodas([FromQuery]Helpers.PalavraUrlQuery query)
        {

            var item = _banco.Palavras.AsQueryable();
            if (query.Data.HasValue)
            {
                item = item.Where(x => x.Criado > query.Data.Value || x.Atualizado > query.Data.Value);
            }
            if (query.PagNumero.HasValue)
            {
                var quantidadeTotalRegistros = item.Count();
                item = item.Skip((query.PagNumero.Value - 1) * query.PagRegistros.Value).Take(query.PagRegistros.Value);

                var paginacao = new Helpers.Paginacao();
                paginacao.NumeroPagina = query.PagNumero.Value;
                paginacao.RegistrosPorPagina = query.PagRegistros.Value;
                paginacao.TotalRegistros = quantidadeTotalRegistros;
                paginacao.TotalPaginas = (int)Math.Ceiling((double)quantidadeTotalRegistros / (double)query.PagRegistros.Value);


                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginacao));
                if (query.PagNumero > paginacao.TotalPaginas)
                {
                    return NotFound();
                }

            
            }


            return Ok(item);
        }

        //WEB
        [Route("{id}")]
        [HttpGet]
        public ActionResult Obter(int id)
        {
            var obj = _banco.Palavras.Find(id);
            if (obj == null)
                return NotFound();

            return Ok(_banco.Palavras.Find(id));
        }

        [Route("")]
        [HttpPost]
        public ActionResult Cadastrar([FromBody]Palavra palavra)
        {
            _banco.Palavras.Add(palavra);
            _banco.SaveChanges();
            return Created($"/api/palavras/{palavra.Id}",palavra);
        }

        [Route("{id}")]
        [HttpPut]
        public ActionResult Atualizar(int id, [FromBody]Palavra palavra)
        {
            var obj = _banco.Palavras.AsNoTracking().FirstOrDefault(a => a.Id == id);
            if (obj == null)
                return NotFound();

            palavra.Id = id;
            _banco.Palavras.Update(palavra);
            _banco.SaveChanges();

            return NoContent();
        }

        [Route("{id}")]
        [HttpDelete]
        public ActionResult Deletar(int id)
        {
            var palavra = _banco.Palavras.Find(id);
            if (palavra == null)
                return NotFound();

            palavra.Ativo = false;
            _banco.Palavras.Update(palavra);
            _banco.SaveChanges();
            return NoContent();
        }

    }
}
