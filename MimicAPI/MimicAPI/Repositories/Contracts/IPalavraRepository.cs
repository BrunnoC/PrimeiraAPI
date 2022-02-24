using MimicAPI.Helpers;
using MimicAPI.Models;
using System.Collections.Generic;

namespace MimicAPI.Repositories.Contracts
{
    public interface IPalavraRepository
    {
        PaginationList<Palavra> ObterPalavras(PalavraUrlQuery query);
        Palavra Obter(int id);
        void Cadastrar(Palavra palavra);
        void Atualizar(Palavra palavra);
        void Deletar(int id);



    }
}
