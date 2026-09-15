using gerenciador_pagamentos.webapi.Interface;

namespace gerenciador_pagamentos.webapi.Repository;

public class JogadorCommandRepository : IJogadorCommandRepository
{
    private readonly string _connectionString;
    public JogadorCommandRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
}
