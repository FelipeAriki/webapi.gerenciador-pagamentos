namespace gerenciador_pagamentos.webapi.Interface;

public interface IJogadorCommandRepository
{
    Task<bool> ExcluirJogador(int id);
}
