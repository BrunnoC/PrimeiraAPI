using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimicAPI.Helpers;
using MimicAPI.V1.Models;
using MimicAPI.V1.Models.DTO;
using MimicAPI.V1.Repositories.Contracts;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace MimicAPI.V1.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    //[Route("api/[controller]")]
    [ApiVersion("1.0",Deprecated = true)]
    [ApiVersion("1.1")]

    public class PalavrasController : ControllerBase
    {
        private readonly IPalavraRepository _repository;
        private readonly IMapper _mapper;

        public PalavrasController(IPalavraRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        //APP -- /api/palavras?data=2019-05-30
        [HttpGet("",Name = "ObterTodas")]
        public ActionResult ObterTodas([FromQuery]Helpers.PalavraUrlQuery query)
        {
            var itens = _repository.ObterPalavras(query);



            if (itens.Results.Count == 0)
            {
                return NotFound();
            }

            PaginationList<PalavraDTO> lista = CriarLinksListPalavraDTO(query, itens);

            return Ok(lista);
        }

        private PaginationList<PalavraDTO> CriarLinksListPalavraDTO(PalavraUrlQuery query, PaginationList<Palavra> itens)
        {
            var lista = _mapper.Map<PaginationList<Palavra>, PaginationList<PalavraDTO>>(itens);

            foreach (var palavra in lista.Results)
            {
                palavra.Links = new System.Collections.Generic.List<LinkDTO>();
                palavra.Links.Add(new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavra.Id }), "GET"));
            }

            lista.Links.Add(new LinkDTO("self", Url.Link("ObterTodas", query), "GET"));

            if (itens.Paginacao != null)
            {
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(itens.Paginacao));

                if (query.PagNumero + 1 <= itens.Paginacao.TotalPaginas)
                {
                    var queryString = new PalavraUrlQuery() { PagNumero = query.PagNumero + 1, PagRegistros = query.PagRegistros, Data = query.Data };
                    lista.Links.Add(new LinkDTO("next", Url.Link("ObterTodas", queryString), "GET"));
                }
                if (query.PagNumero - 1 > 0)
                {
                    var queryString = new PalavraUrlQuery() { PagNumero = query.PagNumero - 1, PagRegistros = query.PagRegistros, Data = query.Data };
                    lista.Links.Add(new LinkDTO("previous", Url.Link("ObterTodas", queryString), "GET"));
                }



            }

            return lista;
        }

        //WEB
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [HttpGet("{id}",Name = "ObterPalavra")]
        public ActionResult Obter(int id)
        {
            var obj = _repository.Obter(id);
            if (obj == null)
                return NotFound();

            PalavraDTO palavraDTO = _mapper.Map<Palavra,PalavraDTO>(obj);
            palavraDTO.Links.Add(new LinkDTO("self", Url.Link("ObterPalavra", new {id = palavraDTO.Id }), "GET"));
            palavraDTO.Links.Add(new LinkDTO("update", Url.Link("AtualizarPalavra", new { id = palavraDTO.Id }), "PUT"));
            palavraDTO.Links.Add(new LinkDTO("delete", Url.Link("ExcluirPalavra", new { id = palavraDTO.Id }), "DELETE"));


            return Ok(palavraDTO);
        }
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [Route("")]
        [HttpPost]
        public ActionResult Cadastrar([FromBody]Palavra palavra)
        {
            if (palavra == null)
                return BadRequest();

            if(!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            palavra.Ativo = true;
            palavra.Criado = DateTime.Now;


            _repository.Cadastrar(palavra);

            PalavraDTO palavraDTO = _mapper.Map<Palavra,PalavraDTO>(palavra);
            palavraDTO.Links.Add(new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavraDTO.Id }), "GET"));




            return Created($"/api/palavras/{palavra.Id}", palavraDTO);
        }
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [Route("{id}")]
        [HttpPut]
        public ActionResult Atualizar(int id, [FromBody]Palavra palavra)
        {
                        if (palavra == null)
                return BadRequest();
            var obj = _repository.Obter(id);
            if (obj == null)
                return NotFound();

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);


            palavra.Id = id;
            palavra.Ativo = obj.Ativo;
            palavra.Criado = obj.Criado;
            palavra.Atualizado = DateTime.Now;
            _repository.Atualizar(palavra);


            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(palavra);
            palavraDTO.Links.Add(new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavraDTO.Id }), "GET"));



            return Ok();
        }

        [MapToApiVersion("1.1")]
        [HttpDelete("{id}", Name = "ExcluirPalavra")]
        public ActionResult Deletar(int id)
        {
            var palavra = _repository.Obter(id);
            if (palavra == null)
                return NotFound();

            _repository.Deletar(id);

            return NoContent();
        }

    }
}
