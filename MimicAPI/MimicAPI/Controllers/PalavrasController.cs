using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimicAPI.Models;
using MimicAPI.Models.DTO;
using MimicAPI.Repositories.Contracts;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace MimicAPI.Controllers
{
    [Route("api/palavras")]


    public class PalavrasController : ControllerBase
    {
        private readonly IPalavraRepository _repository;
        private readonly IMapper _mapper;

        public PalavrasController(IPalavraRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        //APP -- /api/palavras?data=2019-05-30
        [Route("")]
        [HttpGet]
        public ActionResult ObterTodas([FromQuery]Helpers.PalavraUrlQuery query)
        {
            var itens = _repository.ObterPalavras(query);

            if (itens.Count == 0)
            {
                return NotFound();
            }


            if (itens.Paginacao != null)
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(itens.Paginacao));



            return Ok(itens.ToList());
        }

        //WEB

        [HttpGet("{id}",Name = "ObterPalavra")]
        public ActionResult Obter(int id)
        {
            var obj = _repository.Obter(id);
            if (obj == null)
                return NotFound();

            PalavraDTO palavraDTO = _mapper.Map<Palavra,PalavraDTO>(obj);
            palavraDTO.Links = new System.Collections.Generic.List<LinkDTO>();
            palavraDTO.Links.Add(new LinkDTO("self", Url.Link("ObterPalavra", new {id = palavraDTO.Id }), "GET"));
            palavraDTO.Links.Add(new LinkDTO("update", Url.Link("AtualizarPalavra", new { id = palavraDTO.Id }), "PUT"));
            palavraDTO.Links.Add(new LinkDTO("delete", Url.Link("ExcluirPalavra", new { id = palavraDTO.Id }), "DELETE"));


            return Ok(palavraDTO);
        }


        [HttpPost("{id}", Name = "AtualizarPalavra")]
        public ActionResult Cadastrar([FromBody]Palavra palavra)
        {
            _repository.Cadastrar(palavra);
            return Created($"/api/palavras/{palavra.Id}",palavra);
        }

        [Route("{id}")]
        [HttpPut]
        public ActionResult Atualizar(int id, [FromBody]Palavra palavra)
        {
            var obj = _repository.Obter(id);
            if (obj == null)
                return NotFound();

            palavra.Id = id;
            _repository.Atualizar(palavra);


            return Ok();
        }


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
